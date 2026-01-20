using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Controllers
{
    [Authorize(Policy = "OtelCrwAuthPolicy")]
    public class RedemptionsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public RedemptionsController(
            RzvnUmvUmvKrmnBlzrContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;

            _signInManager = signInManager;
        }


        // GET: Redemptions
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1️⃣ Giriş yapan kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // 2️⃣ Kullanıcının rollerini al
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
                return Forbid();

            // 3️⃣ Bu rollere ait roleId'leri al
            var roleIds = await _context.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            // 4️⃣ Bu rollerin claim'lerinden TenantId'yi al
            var tenantIdClaim = await _context.RoleClaims
                .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == "TenantId")
                .Select(rc => rc.ClaimValue)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(tenantIdClaim))
                return Forbid();

            int tenantId = int.Parse(tenantIdClaim);

            // 5️⃣ Redemptions verilerini TenantId'ye göre filtrele
            var groupedData = await _context.Redemptions
                .Where(r => r.TenantId == tenantId)
                .GroupBy(r => r.RoomType)
                .Select(g => new
                {
                    RoomType = g.Key,
                    CurrentPoint = g.OrderByDescending(r => r.CreatedAt).FirstOrDefault().RequiredPoints,
                    Periods = g.Select(r => new
                    {
                        r.Id,
                        r.StartDate,
                        r.EndDate,
                        r.RequiredPoints,
                        r.IsActive
                    })
                    .OrderBy(p => p.StartDate)
                    .ToList()
                })
                .ToListAsync();

            return View(groupedData);
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePeriods([FromBody] RoomTypePeriodUpdateRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Get user's claims
            var claims = await _userManager.GetClaimsAsync(user);

            // Find TenantId claim value
            var tenantClaim = claims.FirstOrDefault(c => c.Type == "TenantId");
            string tenantId = tenantClaim?.Value;

            var existingPeriods = await _context.Redemptions
                .Where(r => r.RoomType == request.RoomType)
                .ToListAsync();

            _context.Redemptions.RemoveRange(existingPeriods);


            var newPeriods = request.Periods.Select(p => new Redemption
            {
                RoomType = request.RoomType,
                TenantId = int.Parse(tenantId),
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                RequiredPoints = p.RequiredPoints,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await _context.Redemptions.AddRangeAsync(newPeriods);
            await _context.SaveChangesAsync();

            return Ok();
        }


        public class RoomTypePeriodUpdateRequest
        {
            public string RoomType { get; set; }
            public List<PeriodDto> Periods { get; set; }
        }

        public class PeriodDto
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int RequiredPoints { get; set; }
        }

        // GET: Redemptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redemption = await _context.Redemptions
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redemption == null)
            {
                return NotFound();
            }

            return View(redemption);
        }

        // GET: Redemptions/Create
        public IActionResult Create()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var roles = _userManager.GetRolesAsync(user).Result;

            string? tenantIdStr = null;

            foreach (var role in roles)
            {
                var roleObj = _roleManager.FindByNameAsync(role).Result;
                var claims = _roleManager.GetClaimsAsync(roleObj).Result;
                var claim = claims.FirstOrDefault(c => c.Type == "TenantId");

                if (claim != null)
                {
                    tenantIdStr = claim.Value;
                    break;
                }
            }

            if (int.TryParse(tenantIdStr, out int tenantId))
            {
                var model = new Redemption { TenantId = tenantId };

                // Oda tiplerini getir
                var roomTypes = _context.RoomTypes
                    .Where(rt => rt.TenantId == tenantId)
                    .Select(rt => rt.Name)
                    .Distinct()
                    .ToList();

                ViewBag.RoomTypeList = new SelectList(roomTypes);

                return View(model);
            }

            return Forbid();
        }


        // POST: Redemptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Redemption redemption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(redemption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", redemption.TenantId);
            return View(redemption);
        }

        // GET: Redemptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redemption = await _context.Redemptions.FindAsync(id);
            if (redemption == null)
            {
                return NotFound();
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", redemption.TenantId);
            return View(redemption);
        }

        // POST: Redemptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,RequiredPoints,IsActive,CreatedAt,RoomType,StartDate,EndDate,UseEarningPeriod")] Redemption redemption)
        {
            if (id != redemption.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redemption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedemptionExists(redemption.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", redemption.TenantId);
            return View(redemption);
        }

        // GET: Redemptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redemption = await _context.Redemptions
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redemption == null)
            {
                return NotFound();
            }

            return View(redemption);
        }

        // POST: Redemptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redemption = await _context.Redemptions.FindAsync(id);
            if (redemption != null)
            {
                _context.Redemptions.Remove(redemption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedemptionExists(int id)
        {
            return _context.Redemptions.Any(e => e.Id == id);
        }
    }
}