using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using RezerVanaUmv.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace RezerVanaUmv.Controllers
{
    //[Authorize]
    public class UserManagementController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;


        public UserManagementController(
        RzvnUmvUmvKrmnBlzrContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Policy = "OtelCrwAuthPolicy")]





        // GET: UserManagement
        //[HttpGet]
        //public async Task<IActionResult> AktifKullanicilar()
        //{
        //    var users = await GetUserListByStatus(true); // Get active users

        //    var userIds = users.Select(u => u.UserId).ToList();

        //    // Reservation points per user grouped by status
        //    var reservationPoints = await _context.Reservations
        //        .Where(r => userIds.Contains(r.UserId) &&
        //                    (r.Status == "Confirmed" || r.Status == "Acquired"))
        //        .GroupBy(r => new { r.UserId, r.Status })
        //        .Select(g => new
        //        {
        //            g.Key.UserId,
        //            g.Key.Status,
        //            TotalPoints = g.Sum(r => r.TotalAmount ?? 0)
        //        })
        //        .ToListAsync();

        //    // Bonus points per user
        //    var bonusPoints = await _context.BalancePoints
        //        .Where(bp => userIds.Contains(bp.UserId))
        //        .GroupBy(bp => bp.UserId)
        //        .Select(g => new
        //        {
        //            UserId = g.Key,
        //            Bonus = g.Sum(bp => bp.Points ?? 0)
        //        })
        //        .ToListAsync();

        //    // Combine all into final model
        //    var result = users.Select(u =>
        //    {
        //        var userId = u.UserId;

        //        var pending = reservationPoints
        //            .FirstOrDefault(r => r.UserId == userId && r.Status == "Confirmed")?.TotalPoints ?? 0;

        //        var gained = reservationPoints
        //            .FirstOrDefault(r => r.UserId == userId && r.Status == "Acquired")?.TotalPoints ?? 0;

        //        var bonus = bonusPoints
        //            .FirstOrDefault(b => b.UserId == userId)?.Bonus ?? 0;

        //        return new UserPointsViewModel
        //        {
        //            User = u,
        //            PendingPoints = pending,
        //            GainedPoints = gained,
        //            BonusPoints = bonus
        //        };
        //    }).ToList();

        //    return View("AktifKullanicilar", result);
        //}

        /*
        [HttpGet]
        public async Task<IActionResult> PasifKullanicilar()
        {
            var model = await GetUserListByStatus(false);
            return View("PasifKullanicilar", model);
        }
        */
        private async Task<List<EditUserViewModel>> GetUserListByStatus(bool aktifMi)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return new();

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            int? tenantId = null;

            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                var tenantClaim = roleClaims.FirstOrDefault(c => c.Type == "TenantId");

                if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int parsedId))
                {
                    tenantId = parsedId;
                    break;
                }
            }

            if (tenantId == null)
                return new();

            var davetKodlari = await _context.DavetKodlari
                .Where(d => d.TenantId == tenantId && d.AgencyId != null /* && d.IsActive == aktifMi */)
                .ToListAsync();

            var davetKoduList = davetKodlari.Select(d => d.DavetKodu).ToList();
            var agencyIdList = davetKodlari.Select(d => d.AgencyId).Distinct().ToList();

            var agencies = await _context.Agencies
                .Where(a => agencyIdList.Contains(a.Id))
                .ToListAsync();

            var allUsers = await _userManager.Users.ToListAsync();
            var model = new List<EditUserViewModel>();

            foreach (var user in allUsers)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var davetKoduClaim = claims.FirstOrDefault(c =>
                    c.Type == "DavetKodu" && davetKoduList.Contains(c.Value));

                if (davetKoduClaim != null)
                {
                    var davetRow = davetKodlari.FirstOrDefault(d => d.DavetKodu == davetKoduClaim.Value);
                    var agency = agencies.FirstOrDefault(a => a.Id == davetRow?.AgencyId);

                    // ✅ Kullanıcının rezervasyonlarını çek
                    var kullaniciRezervasyonlari = await _context.Reservations
                        .Where(r => r.UserId == user.Id && r.TenantId == tenantId)
                        .ToListAsync();

                    int toplamPuan = 0;

                    foreach (var rezervasyon in kullaniciRezervasyonlari)
                    {
                        var puan = await _context.RewardCatalogs
                            .Where(rc => rc.TenantId == rezervasyon.TenantId 
                            && rc.RoomType == rezervasyon.RoomType)
                            .Select(rc => rc.RequiredPoints)
                            .FirstOrDefaultAsync();

                        toplamPuan += puan;
                    }

                    model.Add(new EditUserViewModel
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        AgencyName = agency?.Name,
                        AgencyCountry = agency?.Ulke,
                        IsActive = davetRow?.IsActive ?? false,
                        DavetKodu = davetRow?.DavetKodu,
                        DavetKoduId = davetRow?.Id,
                        ToplamKazanilanPuan = toplamPuan
                    });
                }
            }


            return model;
        }

        // GET: UserManagement/Edit/id
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName
            };

            return View(model);
        }

        // POST: UserManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            user.Email = model.Email;
            user.UserName = model.UserName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            // Şifre değişikliği istenmişse uygula
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(model);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AcenteKullanicilar()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            // Claim'den AgencyId'yi al
            var agencyId = User.Claims.FirstOrDefault(c => c.Type == "AgencyId")?.Value;
            if (string.IsNullOrEmpty(agencyId))
                return Forbid(); // AgencyId yoksa işlem yapılmaz

            
            var agency = await _context.Agencies
                .FirstOrDefaultAsync(a => a.Id.ToString() == agencyId);

            var model = new List<EditUserViewModel>();

            // Tüm kullanıcıları tara
            var allUsers = await _userManager.Users.ToListAsync();

            foreach (var user in allUsers)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var userAgencyClaim = claims.FirstOrDefault(c => c.Type == "AgencyId" && c.Value == agencyId);

                if (userAgencyClaim != null)
                {
                    model.Add(new EditUserViewModel
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        AgencyName = agency?.Name,
                        AgencyCountry = agency?.Ulke,
                        //IsActive = davetRow.IsActive,
                       // DavetKodu = davetRow.Kod, // <-- Bu önemli: davetKodu değişkeni yoktu
                        //DavetKoduId = davetRow.Id
                    });
                }
            }

            ViewData["Title"] = "Davet Koduna Sahip Kullanıcılar";
            return View(model);
        }

    }
}
