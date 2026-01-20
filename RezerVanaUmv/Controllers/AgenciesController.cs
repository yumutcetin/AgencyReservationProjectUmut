using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using System.Security.Claims;


[Authorize]
public class AgenciesController : Controller
{
    private readonly RzvnUmvUmvKrmnBlzrContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppUserRoles> _roleManager;

    private readonly SignInManager<ApplicationUser> _signInManager;

    public AgenciesController(
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


    // GET: Agencies
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var agencyIds = User.Claims.FirstOrDefault(c => c.Type == "AgencyId");
        if (agencyIds == null)
        {
            // Claim yoksa boş liste döndür
            return View(new List<Agency>());
        }

        var agencies = await _context.Agencies
            .Where(a => a.Id == agencyIds.Value)
            .ToListAsync();

        return View(agencies);
    }


    // GET: Agencies/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null) return NotFound();

        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agency == null) return NotFound();

        return View(agency); // ✅ Doğru model türü gönderildi
    }


    [HttpPost]
    public async Task<IActionResult> SetActive(int SelectedAgencyId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var claims = await _userManager.GetClaimsAsync(user);

        await _userManager.AddClaimAsync(user, new Claim("AgencyId", SelectedAgencyId.ToString()));

        await _context.SaveChangesAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }


    // GET: Agencies/Create
    // GET: Agencies/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // 6. Hiçbiri yoksa yeni kayıt sayfasına geç
        //var agencies = _context.Agencies.ToList();
        //ViewBag.Agencies = new SelectList(agencies, "Id", "Name");
        ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id");
        return View();
    }



    // POST: Agencies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Agency agency)
    {

        // 1. Zaman damgası ekle
        agency.CreatedAt = DateTime.Now;
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        // 2. Yeni eklenen acenteyi bul
        var newAgency = await _context.Agencies
            .FirstOrDefaultAsync(x => x.Email == agency.Email);

        // 3. Kullanıcıya AgencyId claim'i ekle (eski varsa önce kaldır)
        var currentUser = await _userManager.GetUserAsync(User);
        var existingClaims = await _userManager.GetClaimsAsync(currentUser);
        var oldClaim = existingClaims.FirstOrDefault(c => c.Type == "AgencyId");
        if (oldClaim != null)
            await _userManager.RemoveClaimAsync(currentUser, oldClaim);

        await _userManager.AddClaimAsync(currentUser, new Claim("AgencyId", newAgency.Id.ToString()));
        await _signInManager.SignInAsync(currentUser, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }



    // GET: Agencies/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null) return NotFound();

        var agency = await _context.Agencies.FindAsync(id);
        if (agency == null)
            return Unauthorized();

        return View(agency);
    }

    // POST: Agencies/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(string id, Agency agency)
    {
        if (id.ToString() != agency.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(agency);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AgencyExists(agency.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(agency);
    }

    // GET: Agencies/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null) return NotFound();

        var agency = await _context.Agencies
            .FirstOrDefaultAsync(m => m.Id == id);
        if (agency == null) return Unauthorized();

        return View(agency);
    }

    // POST: Agencies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        // Kullanıcının erişebildiği agency ID'lerini al

        var agency = await _context.Agencies.FindAsync(id);
        if (agency != null)
        {
            _context.Agencies.Remove(agency);
            await _context.SaveChangesAsync();

            // Ek olarak: RoleClaims içinden de bu AgencyId claim'lerini sil
            var agencyClaims = await _context.RoleClaims
                .Where(rc => rc.ClaimType == "AgencyId" && rc.ClaimValue == id)
                .ToListAsync();

            if (agencyClaims.Any())
            {
                _context.RoleClaims.RemoveRange(agencyClaims);
                await _context.SaveChangesAsync();
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private bool AgencyExists(string id)
    {
        return _context.Agencies.Any(e => e.Id == id);
    }

}