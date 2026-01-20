using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Controllers
{
    public class FacilitiesController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;
        public FacilitiesController(RzvnUmvUmvKrmnBlzrContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }



        [HttpGet]
        public async Task<IActionResult> CreateMenuItem()
        {
            var user = User;
            var facilityClaims = user?.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList() ?? new List<string>();

            var facilities = await _context.Facilities
                .Where(f => facilityClaims.Contains(f.Id))
                .Select(f => new { f.Id, f.Name })
                .ToListAsync();

            ViewBag.Facilities = facilities;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItem item)
        {
            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            var tenantClaim = User.Claims
                .Where(c => c.Type == "TenantId")
                .Select(c => c.Value)
                .FirstOrDefault();

            if (!facilityClaims.Contains(item.FacilityId))
            {
                return Forbid("You do not have permission to add menu items for this facility.");
            }

            item.Id = Guid.NewGuid().ToString();
            item.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            item.TenantId = tenantClaim;

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Menü öğesi başarıyla eklendi.";
            return RedirectToAction("GetMenu");
        }


        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            var user = User;
            var facilityClaims = user?.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(facilityClaims))
                return BadRequest("Facility ID is required.");

            var menu = await _context.MenuItems
                .Where(m => m.FacilityId == facilityClaims)
                .OrderBy(m => m.Name)
                .ToListAsync();

            return View(menu);
        }

        [HttpGet]
        public async Task<IActionResult> EditMenuItem(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid menu item ID.");

            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
                return NotFound("Menu item not found.");

            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            if (!facilityClaims.Contains(item.FacilityId))
                return Forbid("You do not have permission to edit items for this facility.");
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(MenuItem updated)
        {
            if (updated == null || string.IsNullOrEmpty(updated.Id))
                return BadRequest("Invalid menu item.");

            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            var existing = await _context.MenuItems.FindAsync(updated.Id);
            if (existing == null)
                return NotFound("Menu item not found.");

            if (!facilityClaims.Contains(existing.FacilityId))
                return Forbid("You do not have permission to edit this menu item.");

            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Price = updated.Price;

            _context.MenuItems.Update(existing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Menü öğesi başarıyla güncellendi.";
            return RedirectToAction("GetMenu");
        }


        [HttpGet]
        public async Task<IActionResult> DeleteMenuItem(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Invalid menu item ID.");

            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
                return NotFound("Menu item not found.");

            // Check facility claims
            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            if (!facilityClaims.Contains(item.FacilityId))
                return Forbid("You do not have permission to delete this menu item.");

            return View(item);
        }

        [HttpPost, ActionName("DeleteMenuItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItemConfirmed(string id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
                return NotFound("Menu item not found.");

            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            if (!facilityClaims.Contains(item.FacilityId))
                return Forbid("You do not have permission to delete this menu item.");

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Menü öğesi başarıyla silindi.";
            return RedirectToAction("GetMenu", new { facilityId = item.FacilityId });
        }


        [HttpGet]
        public async Task<IActionResult> CreatePurchase()
        {
            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            var menuItems = await _context.MenuItems
                .Where(m => facilityClaims.Contains(m.FacilityId))
                .Select(m => new { m.Id, m.Name, m.Price, m.FacilityId })
                .ToListAsync();

            ViewBag.MenuItems = menuItems;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePurchase(Purchase purchase)
        {
            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            if (!facilityClaims.Contains(purchase.FacilityId))
                return Forbid("You do not have permission to create a purchase for this facility.");

            purchase.Id = Guid.NewGuid().ToString();
            purchase.CreatedAt = DateTime.Now;
            purchase.PurchasedAt ??= DateTime.Now;

            // Lookup price if menu item selected
            if (!string.IsNullOrEmpty(purchase.MenuItemId))
            {
                var menuItem = await _context.MenuItems.FindAsync(purchase.MenuItemId);
                if (menuItem != null)
                    purchase.Price = menuItem.Price;
            }

            purchase.TotalAmount = (purchase.Price ?? 0) * purchase.Quantity;

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Satın alma başarıyla kaydedildi.";
            return RedirectToAction("GetMenu");
        }


        [HttpGet]
        public async Task<IActionResult> GetPurchases()
        {
            var today = DateTime.Now.Date;

            var purchases = await (from p in _context.Purchases
                                   join m in _context.MenuItems on p.MenuItemId equals m.Id into pm
                                   from m in pm.DefaultIfEmpty()
                                   where p.PurchasedAt.HasValue && p.PurchasedAt.Value.Date == today
                                   orderby p.PurchasedAt descending
                                   select new
                                   {
                                       p.Id,
                                       p.GuestId,
                                       MenuItemName = m.Name,
                                       p.Price,
                                       p.Quantity,
                                       p.TotalAmount,
                                       p.PurchasedAt
                                   }).ToListAsync();

            var model = purchases.Select(x => new Purchase
            {
                Id = x.Id,
                GuestId = x.GuestId,
                Price = x.Price,
                Quantity = x.Quantity,
                TotalAmount = x.TotalAmount,
                PurchasedAt = x.PurchasedAt,
                MenuItemId = x.MenuItemName // temporarily reuse for display
            });

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchasesByDateRange(DateTime startDate, DateTime endDate)
        {
            var purchases = await _context.Purchases
                .Where(p => p.PurchasedAt >= startDate && p.PurchasedAt <= endDate)
                .OrderByDescending(p => p.PurchasedAt)
                .ToListAsync();

            return Ok(purchases);
        }

        // GET: Purchase/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> EditPurchase(string id)
        {
            if (id == null)
                return NotFound();

            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                return NotFound();

            // Optional: Only allow editing within allowed facilities
            var facilityClaims = User.Claims
                .Where(c => c.Type == "Facility")
                .Select(c => c.Value)
                .ToList();

            if (!facilityClaims.Contains(purchase.FacilityId))
                return Forbid("Bu tesise ait kayıtları düzenleme yetkiniz yok.");

            return View(purchase);
        }


        // POST: Purchase/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPurchase(string id, Purchase purchase)
        {
            if (id != purchase.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(purchase);

            var existing = await _context.Purchases.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.GuestId = purchase.GuestId;
            existing.MenuItemId = purchase.MenuItemId;
            existing.Quantity = purchase.Quantity;
            existing.Price = purchase.Price;
            existing.TotalAmount = (purchase.Price ?? 0) * purchase.Quantity;
            existing.PurchasedAt = purchase.PurchasedAt;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Satın alma başarıyla güncellendi.";
            return RedirectToAction(nameof(GetPurchases));
        }


        // GET: Purchase/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> DeletePurchase(string id)
        {
            if (id == null)
                return NotFound();

            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                return NotFound();

            return View(purchase);
        }


        // POST: Purchase/Delete/{id}
        [HttpPost, ActionName("DeletePurchase")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                return NotFound();

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Satın alma kaydı silindi.";
            return RedirectToAction(nameof(GetPurchases));
        }

    }
}
