using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Authorize(Policy = "AgencyCrwAuthPolicy")]
    public class ConnectToHotelsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ConnectToHotelsController(
             RzvnUmvUmvKrmnBlzrContext context,
             UserManager<ApplicationUser> userManager,
             RoleManager<AppUserRoles> roleManager,
             SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var agencyID = User.Claims
                .Where(c => c.Type == "AgencyId")
                .Select(c => c.Value)
                .ToList();

            if (!agencyID.Any())
                return View(new List<TenantWithStatusViewModel>());

            var davetKodlari = _context.DavetKodlari
                .Where(d => agencyID.Contains(d.AgencyId))
                .ToList();

            var tenantViewModels = davetKodlari
                .Where(d => d.TenantId.HasValue)
                .Select(d => new TenantWithStatusViewModel
                {
                    Id = d.TenantId.Value,
                    Name = _context.Tenants.FirstOrDefault(t => t.Id == d.TenantId)!.Name,
                    ContactEmail = _context.Tenants.FirstOrDefault(t => t.Id == d.TenantId)!.ContactEmail,
                    CreatedAt = _context.Tenants.FirstOrDefault(t => t.Id == d.TenantId)!.CreatedAt,
                    IsActive = d.IsActive
                })
                .ToList();

            return View(tenantViewModels);
        }




        // GET: DavetKoduTablosus/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var agencyId = User.Claims.FirstOrDefault(c => c.Type == "AgencyId");

            if (currentUser == null)
                return Unauthorized();

            // tenants already connected with this agency
            var tenantsAlreadyConnected = _context.DavetKodlari
                .Where(t => t.AgencyId == agencyId.Value)
                .Select(t => t.TenantId) // assuming DavetKodlari has TenantId
                .ToList();

            // tenants not already connected
            var tenants = _context.Tenants
                .Where(t => !tenantsAlreadyConnected.Contains(t.Id))
                .ToList();

            ViewBag.Tenants = new SelectList(tenants, "Id", "Name");


            return View();
        }


        // POST: DavetKoduTablosus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int SelectedTenant)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            var agencyId = User.Claims.FirstOrDefault(c => c.Type == "AgencyId");
            var davetKodu = _context.DavetKodlari.Where(x=> x.AgencyId == agencyId.Value 
            && x.TenantId == SelectedTenant).FirstOrDefault();
            string newDavetKodu = Guid.NewGuid().ToString("N")[..8].ToUpper();
            if (davetKodu == null)
            {
                _context.DavetKodlari.Add(new DavetKoduTablosu
                {
                    Email = currentUser.Email,
                    DavetKodu = newDavetKodu,
                    AgencyId = agencyId.Value,
                    TenantId = SelectedTenant,
                    IsActive = true
                });
                await _userManager.AddClaimAsync(currentUser, new Claim("DavetKodu", newDavetKodu));
            }
            else
            {
                await _userManager.AddClaimAsync(currentUser, new Claim("DavetKodu", davetKodu.DavetKodu));
            }

            

            await _context.SaveChangesAsync();
            await _signInManager.SignInAsync(currentUser, false);

            return RedirectToAction("Index", "ConnectToHotels");
        }

    }
}
