using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Controllers
{
    [Authorize(Policy = "OtelCrwAuthPolicy")]
    public class BalanceController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;

        public BalanceController(RzvnUmvUmvKrmnBlzrContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // RoleClaim'den TenantId al
            var roles = await _userManager.GetRolesAsync(currentUser);
            var tenantId = await (
                from userRole in _context.UserRoles
                join role in _context.Roles on userRole.RoleId equals role.Id
                join roleClaim in _context.RoleClaims on role.Id equals roleClaim.RoleId
                where userRole.UserId == currentUser.Id && roleClaim.ClaimType == "TenantId"
                select roleClaim.ClaimValue
            ).FirstOrDefaultAsync();

            if (tenantId == null || !int.TryParse(tenantId, out var parsedTenantId))
                return View(new List<dynamic>()); // veya boş liste döndür

            // Sadece bu tenantId'ye ait BalancePoints verileri
            var points = await _context.BalancePoints
                .Where(bp => bp.TenantId == parsedTenantId)
                .OrderByDescending(bp => bp.CreatedAt)
                .Select(bp => new
                {
                    bp.Id,
                    FullName = _userManager.Users
                        .Where(u => u.Id == bp.UserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault(),
                    bp.Points,
                    bp.Description,
                    bp.CreatedAt,
                    Email = _userManager.Users
                        .Where(u => u.Id == bp.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return View(points);
        }


        public class UserWithAgencyViewModel
        {
            public string UserId { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? AgencyId { get; set; }
            public string? AgencyName { get; set; }
        }

        public class BalancePointCreatePageModel
        {
            public List<UserWithAgencyViewModel> Users { get; set; }
        }
        private async Task<int?> GetCurrentTenantId()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return null;

            var myRoles = await _userManager.GetRolesAsync(me);

            foreach (var rn in myRoles)
            {
                var role = await _roleManager.FindByNameAsync(rn);
                if (role == null) continue;

                var rclaims = await _roleManager.GetClaimsAsync(role);
                var t = rclaims.FirstOrDefault(c => c.Type == "TenantId");

                if (t != null && int.TryParse(t.Value, out var parsed))
                    return parsed;
            }

            return null;
        }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 1) Kullanıcıları tek seferde al
            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync();

            int? tenantId = await GetCurrentTenantId();

            if (!users.Any())
            {
                return View(new BalancePointCreatePageModel { Users = new List<UserWithAgencyViewModel>() });
            }

            // 2) Bu user'ların ID listesini çıkar
            var userIds = users.Select(u => u.Id).ToList();

            // 3) UserClaims içinden sadece AgencyId claim'lerini, sadece bu user'lar için çek
            var userClaimsQuery = _context.Set<IdentityUserClaim<string>>();

            var agencyClaims = await userClaimsQuery
                .AsNoTracking()
                .Where(c => c.ClaimType == "AgencyId" && userIds.Contains(c.UserId))
                .OrderBy(c => c.Id)  // aynı user'da birden fazla varsa en eskiyi alalım
                .ToListAsync();

            // 4) UserId -> AgencyId (ilk claim) map'i oluştur
            var userAgencyIdMap = agencyClaims
                .GroupBy(c => c.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().ClaimValue,
                    StringComparer.Ordinal
                );

            // 5) Geçerli AgencyId'leri topla
            var agencyIds = userAgencyIdMap.Values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .ToList();

            var agencyIdsbyTenant = await _context.DavetKodlari
                .AsNoTracking()
                .Where(a => a.TenantId == tenantId && agencyIds.Contains(a.AgencyId))
                .Distinct()
                .Select(a => a.AgencyId)
                .ToListAsync();

            if (!agencyIdsbyTenant.Any())
            {
                return View(new BalancePointCreatePageModel { Users = new List<UserWithAgencyViewModel>() });
            }

            // 6) Ajansları tek sorguda çek ve dict'e al
            var agenciesDict = await _context.Agencies
                .AsNoTracking()
                .Where(a => agencyIdsbyTenant.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a, StringComparer.Ordinal);

            // 7) Sonuç listesi: hem AgencyId claim'i olan hem de Agencies tablosunda karşılığı olan user'lar
            var result = new List<UserWithAgencyViewModel>();

            foreach (var user in users)
            {
                if (!userAgencyIdMap.TryGetValue(user.Id, out var agencyId))
                    continue;

                if (string.IsNullOrWhiteSpace(agencyId))
                    continue;

                if (!agenciesDict.TryGetValue(agencyId, out var agency))
                    continue;

                result.Add(new UserWithAgencyViewModel
                {
                    UserId = user.Id,
                    FullName = user.UserName,
                    Email = user.Email,
                    AgencyId = agency.Id,
                    AgencyName = agency.Name
                });
            }

            var model = new BalancePointCreatePageModel
            {
                Users = result
            };

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> Create(string SelectedUserId, int Point, string Explanation)
        {
            // 1) Kullanıcı kontrolü
            var user = await _userManager.FindByIdAsync(SelectedUserId);
            if (user == null)
                return NotFound();

            // 2) Hedef kullanıcının AgencyId claim'i (STRING!)
            var userClaims = await _userManager.GetClaimsAsync(user);
            var agencyIdClaim = userClaims.FirstOrDefault(c => c.Type == "AgencyId");
            var agencyId = agencyIdClaim?.Value; // string olabilir, yoksa null

            if (string.IsNullOrWhiteSpace(agencyId))
            {
                ModelState.AddModelError("", "Kullanıcının AgencyId claim'i bulunamadı.");
                TempData["Error"] = "Kullanıcının bir acentası yok veya AgencyId claim'i eksik.";
                return RedirectToAction("Index");
            }

            // (Opsiyonel) Agency gerçekten var mı?
            var agencyExists = await _context.Agencies.AsNoTracking().AnyAsync(a => a.Id == agencyId);
            if (!agencyExists)
            {
                ModelState.AddModelError("", "Geçersiz AgencyId.");
                TempData["Error"] = "Geçersiz acenta bilgisi.";
                return RedirectToAction("Index");
            }

            // 3) İşlemi yapan kullanıcının TenantId claim'i (INT?)
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserClaims = await _userManager.GetClaimsAsync(currentUser);
            var tenantIdClaim = currentUserClaims.FirstOrDefault(c => c.Type == "TenantId");

            int? tenantId = null;
            if (!string.IsNullOrWhiteSpace(tenantIdClaim?.Value) && int.TryParse(tenantIdClaim.Value, out var parsedTenantId))
                tenantId = parsedTenantId;

            // 4) Puan validasyonu (istenirse alt/üst sınır koy)
            if (Point == 0)
            {
                TempData["Error"] = "Puan 0 olamaz.";
                return RedirectToAction("Index");
            }

            // 5) Kayıt oluştur
            var balancePoint = new BalancePoint
            {
                UserId = SelectedUserId,
                AgencyId = agencyId,     // <-- STRING
                TenantId = tenantId,     // <-- INT?
                Points = Point,
                Description = Explanation,
                CreatedAt = DateTime.Now
            };

            _context.BalancePoints.Add(balancePoint);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Puan başarıyla eklendi.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var point = await _context.BalancePoints.FindAsync(id);
            if (point == null) return NotFound();

            return View(point);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BalancePoint model)
        {
            if (id != model.Id) return NotFound();

            var point = await _context.BalancePoints.FindAsync(id);
            if (point == null) return NotFound();

            point.Points = model.Points;
            point.Description = model.Description;
            // point.CreatedAt = DateTime.Now; // güncellenmesini istemiyorsan yorumda bırak

            _context.Update(point);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var point = await _context.BalancePoints.FindAsync(id);
            if (point == null) return NotFound();

            _context.BalancePoints.Remove(point);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
