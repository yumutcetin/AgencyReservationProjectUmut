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
    public class RoomTypesController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;

        public RoomTypesController(
             RzvnUmvUmvKrmnBlzrContext context,
             UserManager<ApplicationUser> userManager,
             RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: RoomTypes
        public async Task<IActionResult> Index()
        {
            // 1️⃣ Giriş yapan kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // 2️⃣ Kullanıcının rollerini al
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || !userRoles.Any())
                return Forbid();

            // 3️⃣ Roller üzerinden RoleClaims tablosundan TenantId al
            var roleIds = await _context.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            var tenantIdStr = await _context.RoleClaims
                .Where(rc => rc.ClaimType == "TenantId" && roleIds.Contains(rc.RoleId))
                .Select(rc => rc.ClaimValue)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(tenantIdStr) || !int.TryParse(tenantIdStr, out int tenantId))
                return Forbid(); // TenantId yoksa erişim engellenir

            // 4️⃣ İlgili RoomType kayıtlarını getir
            var roomTypes = await _context.RoomTypes
                .Include(r => r.Tenant)
                .Where(r => r.TenantId == tenantId)
                .ToListAsync();

            return View(roomTypes);
        }

        // GET: RoomTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // GET: RoomTypes/Create
        public async Task<IActionResult> Create()
        {
            // Giriş yapan kullanıcıyı al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Rol Id'leri al
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = await _context.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            // RoleClaims'ten TenantId al
            var tenantIdStr = await _context.RoleClaims
                .Where(rc => rc.ClaimType == "TenantId" && roleIds.Contains(rc.RoleId))
                .Select(rc => rc.ClaimValue)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(tenantIdStr) || !int.TryParse(tenantIdStr, out int tenantId))
                return Forbid();

            ViewBag.TenantId = tenantId;
            return View();
        }


        // POST: RoomTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,PricePerNight,Capacity,BedCount,TenantId")] RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", roomType.TenantId);
            return View(roomType);
        }

        // GET: RoomTypes/Edit/5

public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var roomType = await _context.RoomTypes.FindAsync(id);
        if (roomType == null)
        {
            return NotFound();
        }

        // Kullanıcının rollerini al
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                // RoleClaim içinden TenantId ara
                var tenantClaim = roleClaims.FirstOrDefault(c => c.Type == "TenantId");
                if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var tenantId))
                {
                    roomType.TenantId = tenantId;
                    break; // bulunca çık
                }
            }
        }

        return View(roomType);
    }



    // POST: RoomTypes/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Capacity,BedCount,PricePerNight,IsActive,CreatedAt,TenantId")] RoomType roomType)
        {
            if (id != roomType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeExists(roomType.Id))
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
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", roomType.TenantId);
            return View(roomType);
        }

        // GET: RoomTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomType = await _context.RoomTypes
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomType == null)
            {
                return NotFound();
            }

            return View(roomType);
        }

        // POST: RoomTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomTypeExists(int id)
        {
            return _context.RoomTypes.Any(e => e.Id == id);
        }
    }
}
