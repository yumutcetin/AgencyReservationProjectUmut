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
    [Authorize(Policy = "OtelCrwAuthPolicy")]
    public class LoyaltyPointsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;

        public LoyaltyPointsController(RzvnUmvUmvKrmnBlzrContext context)
        {
            _context = context;
        }

        // GET: LoyaltyPoints
        public async Task<IActionResult> Index()
        {
            var rzvnUmvUmvKrmnBlzrContext = _context.LoyaltyPoints.Include(l => l.Agency).Include(l => l.Reservation).Include(l => l.Tenant);
            return View(await rzvnUmvUmvKrmnBlzrContext.ToListAsync());
        }

        // GET: LoyaltyPoints/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyPoint = await _context.LoyaltyPoints
                .Include(l => l.Agency)
                .Include(l => l.Reservation)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loyaltyPoint == null)
            {
                return NotFound();
            }

            return View(loyaltyPoint);
        }

        // GET: LoyaltyPoints/Create
        public IActionResult Create()
        {
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "Id");
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Id");
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id");
            return View();
        }

        // POST: LoyaltyPoints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenantId,ReservationId,AgencyId,BasePoints,BonusPoints,TotalPoints,CalculatedAt")] LoyaltyPoint loyaltyPoint)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loyaltyPoint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "Id", loyaltyPoint.AgencyId);
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Id", loyaltyPoint.ReservationId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", loyaltyPoint.TenantId);
            return View(loyaltyPoint);
        }

        // GET: LoyaltyPoints/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyPoint = await _context.LoyaltyPoints.FindAsync(id);
            if (loyaltyPoint == null)
            {
                return NotFound();
            }
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "Id", loyaltyPoint.AgencyId);
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Id", loyaltyPoint.ReservationId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", loyaltyPoint.TenantId);
            return View(loyaltyPoint);
        }

        // POST: LoyaltyPoints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,ReservationId,AgencyId,BasePoints,BonusPoints,TotalPoints,CalculatedAt")] LoyaltyPoint loyaltyPoint)
        {
            if (id != loyaltyPoint.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loyaltyPoint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoyaltyPointExists(loyaltyPoint.Id))
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
            ViewData["AgencyId"] = new SelectList(_context.Agencies, "Id", "Id", loyaltyPoint.AgencyId);
            ViewData["ReservationId"] = new SelectList(_context.Reservations, "Id", "Id", loyaltyPoint.ReservationId);
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id", loyaltyPoint.TenantId);
            return View(loyaltyPoint);
        }

        // GET: LoyaltyPoints/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loyaltyPoint = await _context.LoyaltyPoints
                .Include(l => l.Agency)
                .Include(l => l.Reservation)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loyaltyPoint == null)
            {
                return NotFound();
            }

            return View(loyaltyPoint);
        }

        // POST: LoyaltyPoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loyaltyPoint = await _context.LoyaltyPoints.FindAsync(id);
            if (loyaltyPoint != null)
            {
                _context.LoyaltyPoints.Remove(loyaltyPoint);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoyaltyPointExists(int id)
        {
            return _context.LoyaltyPoints.Any(e => e.Id == id);
        }
    }
}
