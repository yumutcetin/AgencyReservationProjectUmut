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
    public class RewardCatalogsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public RewardCatalogsController(
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

        [Authorize(Policy = "OtelCrwAuthPolicy")]

        // GET: RewardCatalogs
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

            // 5️⃣ RewardCatalogs verilerini TenantId'ye göre filtrele
            var groupedData = await _context.RewardCatalogs
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



            var existingPeriods = await _context.RewardCatalogs
                .Where(r => r.RoomType == request.RoomType)
                .ToListAsync();

            _context.RewardCatalogs.RemoveRange(existingPeriods);


            var newPeriods = request.Periods.Select(p => new RewardCatalog
            {
                RoomType = request.RoomType,
                TenantId = int.Parse(tenantId),
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                RequiredPoints = p.RequiredPoints,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await _context.RewardCatalogs.AddRangeAsync(newPeriods);
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


        // GET: RewardCatalogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rewardCatalog = await _context.RewardCatalogs
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rewardCatalog == null)
            {
                return NotFound();
            }

            return View(rewardCatalog);
        }

        // GET: RewardCatalogs/Create
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
                var model = new RewardCatalog { TenantId = tenantId };

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



        // POST: RewardCatalogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RewardCatalog rewardCatalog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rewardCatalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", rewardCatalog.TenantId);
            return View(rewardCatalog);
        }

        // GET: RewardCatalogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rewardCatalog = await _context.RewardCatalogs.FindAsync(id);
            if (rewardCatalog == null)
            {
                return NotFound();
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", rewardCatalog.TenantId);
            return View(rewardCatalog);
        }

        // POST: RewardCatalogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,RequiredPoints,IsActive,CreatedAt,RoomType,StartDate,EndDate,UseEarningPeriod")] RewardCatalog rewardCatalog)
        {
            if (id != rewardCatalog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rewardCatalog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RewardCatalogExists(rewardCatalog.Id))
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
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", rewardCatalog.TenantId);
            return View(rewardCatalog);
        }

        // GET: RewardCatalogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rewardCatalog = await _context.RewardCatalogs
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rewardCatalog == null)
            {
                return NotFound();
            }

            return View(rewardCatalog);
        }

        // POST: RewardCatalogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rewardCatalog = await _context.RewardCatalogs.FindAsync(id);
            if (rewardCatalog != null)
            {
                _context.RewardCatalogs.Remove(rewardCatalog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RewardCatalogExists(int id)
        {
            return _context.RewardCatalogs.Any(e => e.Id == id);
        }
    }
}
