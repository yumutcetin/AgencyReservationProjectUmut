using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using RezerVanaUmv.ViewModels;

namespace RezerVanaUmv.Controllers
{
    [Authorize(Policy = "OtelCrwAuthPolicy")]
    public class DavetKoduTablosusController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;

        public DavetKoduTablosusController(
             RzvnUmvUmvKrmnBlzrContext context,
             UserManager<ApplicationUser> userManager,
             RoleManager<AppUserRoles> roleManager)
                {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: DavetKoduTablosus
        [HttpGet]
        public async Task<IActionResult> Index(bool isActive)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            string? tenantIdStr = null;

            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var tenantClaim = claims.FirstOrDefault(c => c.Type == "TenantId");

                    if (tenantClaim != null)
                    {
                        tenantIdStr = tenantClaim.Value;
                        break;
                    }
                }
            }

            if (!int.TryParse(tenantIdStr, out int tenantId))
            {
                return Forbid();
            }

            // Join ile tenant ve agency adlarını çek
            var davetKodlari = await _context.DavetKodlari
                .Where(d => d.TenantId == tenantId && d.IsActive == isActive)
                .Select(d => new
                {
                    d.Id,
                    d.DavetKodu,
                    d.TenantId,
                    d.AgencyId,
                    d.Email,
                    TenantName = _context.Tenants.FirstOrDefault(t => t.Id == d.TenantId).Name,
                    AgencyName = _context.Agencies.FirstOrDefault(a => a.Id == d.AgencyId).Name
                })
                .ToListAsync();

            // ViewModel'e dönüştür
            var model = davetKodlari.Select(d => new DavetKoduViewModel
            {
                Id = (int)d.Id,
                DavetKodu = d.DavetKodu,
                TenantName = d.TenantName,
                AgencyName = d.AgencyName,
                Email = d.Email
                
            }).ToList();

            return View(model);
        }



        // GET: DavetKoduTablosus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var davetKoduTablosu = await _context.DavetKodlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (davetKoduTablosu == null)
            {
                return NotFound();
            }

            return View(davetKoduTablosu);
        }

        private static string GenerateUniqueCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // GET: DavetKoduTablosus/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {

            //var model = new DavetKoduTablosu
            //{
            //    Email = davetKodu,
            //    TenantId = tenantId,
            //    RoleId = roleId,
            //    AgencyId = null 
            //};

            return View();
        }


        // POST: DavetKoduTablosus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,TenantId,AgencyId")]
                DavetKoduTablosu davetKoduTablosu)
        {
            if (!ModelState.IsValid)
                return View(davetKoduTablosu);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            if (userRoles == null || userRoles.Count == 0)
                return Forbid();

            var roleName = userRoles.First();
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return Forbid();

            var claims = await _roleManager.GetClaimsAsync(role);
            var tenantIdClaim = claims.FirstOrDefault(c => c.Type == "TenantId");
            var agencyRoleClaim = claims.FirstOrDefault(c => c.Type == "AgencyRole");

            if (tenantIdClaim == null || agencyRoleClaim == null)
                return Forbid();

            int tenantId = int.Parse(tenantIdClaim.Value);
            string roleId = agencyRoleClaim.Value;

            // 🚨 BURADA VARSA DURDUR
            bool alreadyExists = _context.DavetKodlari.Any(d =>
                d.Email == davetKoduTablosu.Email && d.TenantId == tenantId);

            if (alreadyExists)
            {
                ModelState.AddModelError(string.Empty, "Bu kullanıcı zaten bu otele'a bağlı." +
                    " Tekrar davet kodu oluşturulamaz.");
                return View(davetKoduTablosu);
            }

            // ✅ Devam et
            string uniquePart = GenerateUniqueCode(4);
            string tenantName = _context.Tenants
                                  .Where(t => t.Id == tenantId)
                                  .Select(t => t.Name)
                                  .FirstOrDefault() ?? "CODE";

            string davetKodu = tenantName.Substring(0, Math.Min(4, tenantName.Length)).ToUpper() + uniquePart;

            var model = new DavetKoduTablosu
            {
                Email = davetKoduTablosu.Email,
                DavetKodu = davetKodu,
                TenantId = tenantId,
                RoleId = roleId,
                AgencyId = null
            };

            var message = new MailMessage
            {
                Subject = "Invitation For RezerVana Loyalty Program",
                IsBodyHtml = true,
                From = new MailAddress("info@bagsfollow.com")
            };

            message.Body = davetKodu;
            message.To.Add(davetKoduTablosu.Email);

            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new NetworkCredential("info@bagsfollow.com", "itawepffkryuxkrw");
                client.EnableSsl = true;
                client.Send(message);
            }

            _context.DavetKodlari.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // GET: DavetKoduTablosus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var davetKoduTablosu = await _context.DavetKodlari.FindAsync(id);
            if (davetKoduTablosu == null)
            {
                return NotFound();
            }
            return View(davetKoduTablosu);
        }

        // POST: DavetKoduTablosus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,DavetKodu,TenantId,AgencyId")] DavetKoduTablosu davetKoduTablosu)
        {
            if (id != davetKoduTablosu.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(davetKoduTablosu);

            // 🔐 1. Giriş yapan kullanıcıyı al
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            // 🧾 2. Kullanıcının rollerini al
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            if (userRoles == null || userRoles.Count == 0)
            {
                ModelState.AddModelError("", "Kullanıcının atanmış bir rolü yok.");
                return View(davetKoduTablosu);
            }

            // 🎯 3. İlk rol adına göre IdentityRole nesnesini al
            var roleName = userRoles.First();
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                ModelState.AddModelError("", "Kullanıcının rol bilgisi sistemde bulunamadı.");
                return View(davetKoduTablosu);
            }

            // 🧩 4. Role'a ait 'AgencyRole' claim'ini al
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var agencyRoleClaim = roleClaims.FirstOrDefault(c => c.Type == "AgencyRole");
            if (agencyRoleClaim == null)
            {
                ModelState.AddModelError("", "'AgencyRole' claim'i rol üzerinde tanımlı değil.");
                return View(davetKoduTablosu);
            }

            // 🏷️ 5. RoleClaim'den alınan değeri RoleId alanına ata
            davetKoduTablosu.RoleId = agencyRoleClaim.Value;

            // 💾 6. Güncelle
            try
            {
                _context.Update(davetKoduTablosu);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DavetKoduTablosuExists(davetKoduTablosu.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: DavetKoduTablosus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var davetKoduTablosu = await _context.DavetKodlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (davetKoduTablosu == null)
            {
                return NotFound();
            }

            return View(davetKoduTablosu);
        }

        // POST: DavetKoduTablosus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var davetKoduTablosu = await _context.DavetKodlari.FindAsync(id);
            if (davetKoduTablosu != null)
            {
                davetKoduTablosu.IsActive = false;
                // 1. DavetKodu verisini sil
                _context.DavetKodlari.Update(davetKoduTablosu);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool DavetKoduTablosuExists(int? id)
        {
            return _context.DavetKodlari.Any(e => e.Id == id);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int? id)
        {
            var davetKodu = await _context.DavetKodlari
                .FirstOrDefaultAsync(m => m.Id == id);

            davetKodu.IsActive = true;

            _context.Update(davetKodu);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { isActive = true});
        }
    }
}
