using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Controllers
{
    public class ReservationBonusSettingsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;

        public ReservationBonusSettingsController(RzvnUmvUmvKrmnBlzrContext context)
        {
            _context = context;
        }


        [Authorize(Policy = "OtelCrwAuthPolicy")]

        [HttpGet]
        // GET: ReservationBonusSettings
        public async Task<IActionResult> Index()
        {
            var rzvnUmvUmvKrmnBlzrContext = _context.ReservationBonusSettings.Include(r => r.Tenant);
            return View(await rzvnUmvUmvKrmnBlzrContext.ToListAsync());
        }

        // GET: ReservationBonusSettings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationBonusSetting = await _context.ReservationBonusSettings
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservationBonusSetting == null)
            {
                return NotFound();
            }

            return View(reservationBonusSetting);
        }

        // GET: ReservationBonusSettings/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Eğer TempData'da mesaj varsa ViewBag'e ata (gerekirse)
            if (TempData["ErrorMessage"] != null)
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View();
        }

        // POST: ReservationBonusSettings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationBonusSetting reservationBonusSetting)
        {
            // Kullanıcı claim'den TenantId al
            var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId");
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                ModelState.AddModelError("", "TenantId bulunamadı.");
                return View(reservationBonusSetting);
            }

            // TenantId'yi modele ata
            reservationBonusSetting.TenantId = tenantId;

            // Aynı tenant için daha önce kayıt yapılmış mı kontrol et
            var recordExists = await _context.ReservationBonusSettings
                .AnyAsync(x => x.TenantId == tenantId);

            if (recordExists)
            {
                TempData["ErrorMessage"] = "⚠️ Bu tenant için zaten bir kayıt mevcut. Lütfen liste sayfasından düzenleyin.";
                return RedirectToAction(nameof(Create)); // Liste değil Create sayfasına döndürülüyor, orada mesaj görünsün diye
            }

            // Geçerli modelse kaydet
            if (ModelState.IsValid)
            {
                _context.Add(reservationBonusSetting);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Ayarlar başarıyla kaydedildi.";
                return RedirectToAction(nameof(Create));
            }

            return View(reservationBonusSetting);
        }


        // GET: ReservationBonusSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationBonusSetting = await _context.ReservationBonusSettings.FindAsync(id);
            if (reservationBonusSetting == null)
            {
                return NotFound();
            }
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", reservationBonusSetting.TenantId);
            return View(reservationBonusSetting);
        }

        // POST: ReservationBonusSettings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BonusProcedureUrl,CreatedAt,BonusInfoEmail,SearchType,MinStayDay,MaxStayDay,MinReservationDay,MaxReservationDay,MinBalance,YearlyUsePoint,IsBonusProcEnabled,IsExcheckinDateControl,TenantId")] ReservationBonusSetting reservationBonusSetting)
        {
            if (id != reservationBonusSetting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservationBonusSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationBonusSettingExists(reservationBonusSetting.Id))
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
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", reservationBonusSetting.TenantId);
            return View(reservationBonusSetting);
        }

        // GET: ReservationBonusSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationBonusSetting = await _context.ReservationBonusSettings
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservationBonusSetting == null)
            {
                return NotFound();
            }

            return View(reservationBonusSetting);
        }

        // POST: ReservationBonusSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservationBonusSetting = await _context.ReservationBonusSettings.FindAsync(id);
            if (reservationBonusSetting != null)
            {
                _context.ReservationBonusSettings.Remove(reservationBonusSetting);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationBonusSettingExists(int id)
        {
            return _context.ReservationBonusSettings.Any(e => e.Id == id);
        }
    }
}
