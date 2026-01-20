using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using System.Security.Claims;
using RezerVanaUmv.ViewModels;
using Newtonsoft.Json;



[Authorize(Policy = "OtelCrwAuthPolicy")]
public class AgenciesHotelsController : Controller
{
    private readonly RzvnUmvUmvKrmnBlzrContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppUserRoles> _roleManager;

    public AgenciesHotelsController(
        RzvnUmvUmvKrmnBlzrContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<AppUserRoles> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
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
    public async Task<IActionResult> Index()
    {
        // 1) TenantId
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Unauthorized();

        var myRoles = await _userManager.GetRolesAsync(me);
        int? tenantId = null;
        foreach (var rn in myRoles)
        {
            var role = await _roleManager.FindByNameAsync(rn);
            if (role == null) continue;
            var rclaims = await _roleManager.GetClaimsAsync(role);
            var t = rclaims.FirstOrDefault(c => c.Type == "TenantId");
            if (t != null && int.TryParse(t.Value, out var parsed)) { tenantId = parsed; break; }
        }
        if (tenantId == null) return Forbid();
        int tenantIdInt = tenantId.Value;

        // 2) Tenant'a bağlı davetler (AgencyId STRING) -> sadece lazım olan alanlar
        var invites = await _context.DavetKodlari
            .AsNoTracking()
            .Where(d => d.TenantId == tenantIdInt && d.AgencyId != null)
            .Select(d => new { d.DavetKodu, d.AgencyId, d.IsActive, d.Email })
            .ToListAsync();

        // Hızlı lookup için hazırla
        var inviteCodes = invites.Select(i => i.DavetKodu!).Distinct().ToHashSet(StringComparer.Ordinal);
        var agencyIds = invites.Select(i => i.AgencyId!).Distinct().ToHashSet(StringComparer.Ordinal);
        var inviteByAgency = invites.GroupBy(i => i.AgencyId!).ToDictionary(g => g.Key, g => g.First());
        var agencyIdByInvite = invites.ToDictionary(i => i.DavetKodu!, i => i.AgencyId!, StringComparer.Ordinal);

        // 3) Ajans listesini çek (no-tracking)
        var agencies = await _context.Agencies
            .AsNoTracking()
            .Where(a => agencyIds.Contains(a.Id))
            .ToListAsync();

        var agencyIDs = agencies.Select(r => r.Id);

        // 4) Rezervasyon puanları (ajans bazında) -> tek sorgu
        var resAgg = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.TenantId == tenantIdInt && r.AgencyId != null && agencyIds.Contains(r.AgencyId) && r.Type == 1)
            .GroupBy(r => new { r.AgencyId, r.Status })
            .Select(g => new { g.Key.AgencyId, g.Key.Status, Sum = g.Sum(x => x.TotalAmount ?? 0) })
            .ToListAsync();

        var resAggSpent = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.TenantId == tenantIdInt && r.AgencyId != null && agencyIds.Contains(r.AgencyId) && r.Type == 2)
            .GroupBy(r => new { r.AgencyId, r.Status })
            .Select(g => new { g.Key.AgencyId, g.Key.Status, Sum = g.Sum(x => x.TotalAmount ?? 0) })
            .ToListAsync();

        var gainedByAgency = resAgg
            .Where(x => x.Status == "Acquired")
            .GroupBy(x => x.AgencyId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Sum));

        var pendingByAgency = resAgg
            .Where(x => x.Status == "Pending")
            .GroupBy(x => x.AgencyId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Sum));

        var spentByAgency = resAggSpent
            .Where(x => x.Status == "Acquired")
            .GroupBy(x => x.AgencyId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Sum));

        var spentPendingByAgency = resAggSpent
            .Where(x => x.Status == "Pending")
            .GroupBy(x => x.AgencyId!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Sum));

        // 5) BONUS puanları:
        //    BalancePoints (UserId)  JOIN  AspNetUserClaims (ClaimType='DavetKodu' AND ClaimValue IN inviteCodes)
        //    -> önce DavetKodu bazında topla, sonra AgencyId’ye çevirip ajans bazında birleştir.
        var userClaims = _context.Set<IdentityUserClaim<string>>(); // Identity tablo erişimi

        var bonusByInvite = await (
            from bp in _context.BalancePoints.AsNoTracking()
            join uc in userClaims.AsNoTracking()
                 on bp.UserId equals uc.UserId
            where bp.TenantId == tenantIdInt
               && bp.UserId != null
               && uc.ClaimType == "DavetKodu"
               && inviteCodes.Contains(uc.ClaimValue!)
            group bp by uc.ClaimValue into g
            select new
            {
                DavetKodu = g.Key!,
                Bonus = g.Sum(x => x.Points ?? 0)
            }
        ).ToListAsync();

        // DavetKodu -> AgencyId -> ajans bazında bonus toplamı
        var bonusByAgency = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var row in bonusByInvite)
        {
            if (!agencyIdByInvite.TryGetValue(row.DavetKodu, out var aId)) continue;
            bonusByAgency[aId] = (bonusByAgency.TryGetValue(aId, out var cur) ? cur : 0) + row.Bonus;
        }

        // 6) Model (3 tür + toplam) -> sözlüklerden O(1) ile çek
        var model = agencies.Select(a =>
        {
            gainedByAgency.TryGetValue(a.Id, out var gained);
            pendingByAgency.TryGetValue(a.Id, out var pending);
            spentByAgency.TryGetValue(a.Id, out var spent);
            spentPendingByAgency.TryGetValue(a.Id, out var spentpending);
            bonusByAgency.TryGetValue(a.Id, out var bonus);
            var inv = inviteByAgency.TryGetValue(a.Id, out var info) ? info : null;

            return new AgencyWithStatusViewModel
            {
                Agency = a,
                IsActive = inv?.IsActive ?? false,
                Email = inv?.Email ?? "",
                GainedPoints = gained,
                PendingPoints = pending,
                SpentPoints = spent,
                SpentPendingPoints = spentpending,
                BonusPoints = bonus
            };
        }).ToList();

        return View(model);
    }



    // GET: Agencies/Details/5
    [HttpGet]
    public async Task<IActionResult> AgenciesDetails(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        // 1) TenantId al
        var tenantId = await GetCurrentTenantId();
        if (tenantId == null)
            return Forbid();

        int tenantIdInt = tenantId.Value;

        // 2) Bu tenant + bu ajansa ait davetler
        var invites = await _context.DavetKodlari
            .AsNoTracking()
            .Where(d => d.TenantId == tenantIdInt &&
                        d.AgencyId != null &&
                        d.AgencyId == id)
            .Select(d => new { d.DavetKodu, d.AgencyId })
            .ToListAsync();

        // Bu tenant'a bağlı değilse erişim verme
        if (!invites.Any())
            return Forbid();

        var inviteCodes = invites
            .Select(i => i.DavetKodu!)
            .Distinct()
            .ToList();

        // 3) Ajans bilgisi
        var agency = await _context.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agency == null)
            return NotFound();

        // 4) Bu ajansa ait tüm rezervasyonlar (hem kazanma hem harcama)
        var reservations = await _context.Reservations
            .Include(r => r.Passengers)
            .AsNoTracking()
            .Where(r =>
                r.TenantId == tenantIdInt &&
                r.AgencyId == id)
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        var earn = reservations.Where(r => r.Type == 1); // kazanılan
        var spend = reservations.Where(r => r.Type == 2); // harcanan

        int pendingPoints = earn
            .Where(r => r.Status == "Pending")
            .Sum(r => r.TotalAmount ?? 0);

        int gainedPoints = earn
            .Where(r => r.Status == "Acquired")
            .Sum(r => r.TotalAmount ?? 0);

        int spentPoints = spend
            .Where(r => r.Status == "Acquired")
            .Sum(r => r.TotalAmount ?? 0);

        int spentPendingPoints = spend
            .Where(r => r.Status == "Pending")
            .Sum(r => r.TotalAmount ?? 0);

        // 5) Bonus puanlar (sadece bu ajansın DavetKod'ları için)
        var userClaims = _context.Set<IdentityUserClaim<string>>();

        var bonusByInvite = await (
            from bp in _context.BalancePoints.AsNoTracking()
            join uc in userClaims.AsNoTracking()
                on bp.UserId equals uc.UserId
            where bp.TenantId == tenantIdInt
                  && bp.UserId != null
                  && uc.ClaimType == "DavetKodu"
                  && inviteCodes.Contains(uc.ClaimValue!)
            group bp by uc.ClaimValue into g
            select new
            {
                DavetKodu = g.Key!,
                Bonus = g.Sum(x => x.Points ?? 0)
            }
        ).ToListAsync();

        int bonusPoints = bonusByInvite.Sum(x => x.Bonus);

        // 6) ViewModel doldur
        var vm = new AgencyReservationsViewModel
        {
            agency = agency,
            PendingPoints = pendingPoints,
            GainedPoints = gainedPoints,
            SpentPoints = spentPoints,
            SpentPendingPoints = spentPendingPoints,
            BonusPoints = bonusPoints,

            // Burada ReservationWithUserViewModel'i kendi tanımına göre doldurursun
            Reservations = reservations
                .Select(r => new ReservationWithUserViewModel
                {
                    Reservation = r
                })
                .ToList()

        };

        return View("AgenciesDetails", vm);
    }


    // GET: Agencies/Create
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id");
        return View();
    }

    // POST: Agencies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Agency agency)
    {
        if (!ModelState.IsValid)
            return View(agency);

        // 🕒 1. Zaman damgası ekle ve kaydet
        agency.CreatedAt = DateTime.Now;
        _context.Agencies.Add(agency);
        await _context.SaveChangesAsync();

        // 👤 2. Giriş yapan kullanıcıyı al
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized();

        // 🎯 3. Kullanıcının rollerini al
        var userRoles = await _userManager.GetRolesAsync(currentUser);

        // 🔐 4. RoleClaim olarak AgencyId ekle (rol içinde yoksa)
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var exists = await _context.RoleClaims.AnyAsync(rc =>
                rc.RoleId == role.Id &&
                rc.ClaimType == "AgencyId" &&
                rc.ClaimValue == agency.Id.ToString());

            if (!exists)
            {
                _context.RoleClaims.Add(new IdentityRoleClaim<string>
                {
                    RoleId = role.Id,
                    ClaimType = "AgencyId",
                    ClaimValue = agency.Id.ToString()
                });
            }
        }

        // 👤 5. UserClaim olarak AgencyId ekle (kullanıcıda yoksa)
        var userClaims = await _userManager.GetClaimsAsync(currentUser);
        if (!userClaims.Any(c => c.Type == "AgencyId" && c.Value == agency.Id.ToString()))
        {
            await _userManager.AddClaimAsync(currentUser, new Claim("AgencyId", agency.Id.ToString()));
        }

        // 📩 6. Kullanıcıya özel davet kodu oluştur veya güncelle
        var existingInvite = await _context.DavetKodlari
            .FirstOrDefaultAsync(d => d.AgencyId == null && d.TenantId == null); // veya kullanıcıya göre filtrele

        if (existingInvite != null)
        {
            existingInvite.AgencyId = agency.Id;
            _context.DavetKodlari.Update(existingInvite);
        }
        else
        {
            _context.DavetKodlari.Add(new DavetKoduTablosu
            {
                DavetKodu = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                AgencyId = agency.Id,
                TenantId = null,
                RoleId = null
            });
        }

        // 💾 7. Değişiklikleri kaydet
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> SetConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Kullanıcının rollerini al
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles == null || !userRoles.Any())
            return Forbid();

        // Role bazlı tenant ID al
        var roleIds = await _context.Roles
            .Where(r => userRoles.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        var tenantIdStr = await _context.RoleClaims
            .Where(rc => rc.ClaimType == "TenantId" && roleIds.Contains(rc.RoleId))
            .Select(rc => rc.ClaimValue)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(tenantIdStr))
            return Forbid();

        int parsedTenantId = int.Parse(tenantIdStr);

        // Rezervasyonu getir
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == parsedTenantId);

        if (reservation == null)
            return NotFound();

        // Statüyü güncelle
        reservation.Status = "Acquired";

        /*
        // Puanı düş (eksi değer olarak)
        var balanceEntry = new BalancePoint
        {
            TenantId = reservation.TenantId,
            AgencyId = reservation.AgencyId,
            Points = -(reservation.TotalAmount ?? 0), // eksi değer
            Description = $"Rezervasyon #{reservation.Id} onaylandı, puan düşüldü.",
            CreatedAt = DateTime.Now,
            UserId = reservation.UserId
        };

        _context.BalancePoints.Add(balanceEntry);

        */

        await _context.SaveChangesAsync();

        return RedirectToAction("Harcamalar", "AgenciesHotels");
    }



    [HttpGet]
    public async Task<IActionResult> SetAcquired(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Kullanıcı rollerini al
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles == null || !userRoles.Any())
            return Forbid();

        // Rollerden TenantId'yi çek
        var roleIds = await _context.Roles
            .Where(r => userRoles.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        var tenantId = await _context.RoleClaims
            .Where(rc => rc.ClaimType == "TenantId" && roleIds.Contains(rc.RoleId))
            .Select(rc => rc.ClaimValue)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(tenantId))
            return Forbid();

        int parsedTenantId = int.Parse(tenantId);

        // İlgili rezervasyonu al ve kontrol et
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == parsedTenantId);

        if (reservation == null)
            return NotFound();

        // Statüsünü güncelle
        reservation.Status = "Acquired";
        await _context.SaveChangesAsync();

        return RedirectToAction("Harcamalar", "AgenciesHotels");
    }


    [HttpGet]
    public async Task<IActionResult> EditHarcamalar(int? id)
    {
        if (id == null)
            return NotFound();

        var reservation = await _context.Reservations
            .Include(r => r.Passengers)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
            return NotFound();

        // Yolcuları PassengerInputModel'a dönüştür
        var passengerInputs = reservation.Passengers.Select(p => new PassengerInputModel
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Gender = p.Gender,
            BirthDate = p.BirthDate
        }).ToList();

        var viewModel = new ReservationWithPassengersViewModel
        {
            
            Reservation = reservation,
            Passengers = passengerInputs
        };

        return View(viewModel); // ✅ DOĞRU MODEL GÖNDERİLİYOR
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHarcamalar(int id, ReservationWithPassengersViewModel model)
    {
        if (id != model.Reservation.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", model.Reservation.TenantId);
            ViewBag.AgencyId = new SelectList(_context.Agencies, "Id", "Name", model.Reservation.AgencyId);
            return View(model);
        }

        try
        {
            var reservation = await _context.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            // 📝 Temel rezervasyon bilgilerini güncelle
            reservation.CheckinDate = model.Reservation.CheckinDate;
            reservation.CheckoutDate = model.Reservation.CheckoutDate;
            reservation.RoomCount = model.Reservation.RoomCount;
            reservation.RoomType = model.Reservation.RoomType;
            reservation.BookingReference = model.Reservation.BookingReference;
            reservation.ReservationDate = model.Reservation.ReservationDate ?? DateTime.Now;

            // 🧮 NightCount (kaç gece kalacak) yeniden hesapla
            if (reservation.CheckinDate != null && reservation.CheckoutDate != null)
            {
                var checkin = reservation.CheckinDate.Value.ToDateTime(TimeOnly.MinValue);
                var checkout = reservation.CheckoutDate.Value.ToDateTime(TimeOnly.MinValue);
                reservation.NightCount = (int)(checkout - checkin).TotalDays;
            }

            // 🧮 Toplam puanı yeniden hesapla
            var totalPoints = await CalculateRewardPoints(reservation);
            reservation.TotalAmount = totalPoints;

            // 👥 Yolcu bilgilerini güncelle
            var existingPassengers = reservation.Passengers.ToList();

            foreach (var p in model.Passengers)
            {
                if (p.Id == 0)
                {
                    reservation.Passengers.Add(new Passenger
                    {
                        FirstName = p.FirstName?.ToUpper(),
                        LastName = p.LastName?.ToUpper(),
                        Gender = p.Gender,
                        BirthDate = p.BirthDate,
                        ReservationId = reservation.Id
                    });
                }
                else
                {
                    var existing = existingPassengers.FirstOrDefault(ep => ep.Id == p.Id);
                    if (existing != null)
                    {
                        existing.FirstName = p.FirstName?.ToUpper();
                        existing.LastName = p.LastName?.ToUpper();
                        existing.Gender = p.Gender;
                        existing.BirthDate = p.BirthDate;
                    }
                }
            }

            // ❌ Silinen yolcuları kaldır
            if (!string.IsNullOrEmpty(Request.Form["DeletedPassengerIds"]))
            {
                var deletedIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(Request.Form["DeletedPassengerIds"]);
                foreach (var pid in deletedIds)
                {
                    var toDelete = reservation.Passengers.FirstOrDefault(p => p.Id == pid);
                    if (toDelete != null)
                        _context.Passengers.Remove(toDelete);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Harcamalar", "AgenciesHotels");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReservationExists(model.Reservation.Id))
                return NotFound();
            else
                throw;
        }
    }

    private async Task<int> CalculateRewardPoints(Reservation reservation)
    {
        if (reservation.CheckinDate == null || reservation.CheckoutDate == null || reservation.TenantId == null)
            return 0;

        // Convert DateOnly to DateTime for comparisons
        var checkin = reservation.CheckinDate.Value.ToDateTime(TimeOnly.MinValue);
        var checkout = reservation.CheckoutDate.Value.ToDateTime(TimeOnly.MinValue);

        // Exclude the checkout day from calculation
        var actualCheckout = checkout.AddDays(-1);

        var catalogs = await _context.RewardCatalogs
            .Where(c => c.TenantId == reservation.TenantId
                        && c.RoomType == reservation.RoomType
                        && c.StartDate <= actualCheckout
                        && c.EndDate >= checkin)
            .ToListAsync();

        if (!catalogs.Any()) return 0;

        int totalPoints = 0;

        foreach (var catalog in catalogs)
        {
            var catalogStart = catalog.StartDate!.Value.Date;
            var catalogEnd = catalog.EndDate!.Value.Date;

            var overlapStart = (catalogStart > checkin ? catalogStart : checkin).Date;
            var overlapEnd = (catalogEnd < actualCheckout ? catalogEnd : actualCheckout).Date;

            if (overlapEnd < overlapStart)
                continue;

            int overlapNights = (int)(overlapEnd - overlapStart).TotalDays + 1;

            totalPoints += overlapNights * catalog.RequiredPoints;
        }

        return totalPoints;
    }

    private bool ReservationExists(int id)
    {
        return _context.Reservations.Any(e => e.Id == id);
    }


    [HttpGet]
    public async Task<IActionResult> DetailsHarcamalar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reservation = await _context.Reservations
            .Include(r => r.Agency)
            .Include(r => r.Tenant)
            .Include(r => r.Passengers)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }


    [HttpGet]
    public async Task<IActionResult> DeleteHarcamalar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reservation = await _context.Reservations
            .Include(r => r.Agency)
            .Include(r => r.Tenant)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    [HttpPost, ActionName("DeleteHarcamalar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHarcamalarConfirmed(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Passengers)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation != null)
        {
            // Önce yolcuları sil
            _context.Passengers.RemoveRange(reservation.Passengers);

            // Sonra rezervasyonu sil
            _context.Reservations.Remove(reservation);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Harcamalar", "AgenciesHotels");
    }


    // GET: Agencies/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null) return NotFound();

        var agency = await _context.Agencies.FindAsync(id);
        if (agency == null)
            return Unauthorized();

        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Unauthorized();

        var myRoles = await _userManager.GetRolesAsync(me);
        int? tenantId = null;

        foreach (var rn in myRoles)
        {
            var role = await _roleManager.FindByNameAsync(rn);
            if (role == null) continue;

            var rclaims = await _roleManager.GetClaimsAsync(role);
            var t = rclaims.FirstOrDefault(c => c.Type == "TenantId");
            if (t != null && int.TryParse(t.Value, out var parsed))
            {
                tenantId = parsed;
                break;
            }
        }

        if (tenantId == null) return Forbid();
        int tenantIdInt = tenantId.Value;

        var earnedPoints = await _context.Reservations
            .Where(r => r.UserId == agency.Id && r.Type == 1 && r.Status == "Acquired" && r.TenantId == tenantIdInt)
            .SumAsync(r => r.TotalAmount ?? 0);

        var pendingPoints = await _context.Reservations
            .Where(r => r.UserId == agency.Id && r.Type == 1 && (r.Status == "Pending") && r.TenantId == tenantIdInt)
            .SumAsync(r => r.TotalAmount ?? 0);

        var spentPoints = await _context.Reservations
            .Where(r => r.UserId == agency.Id && r.Type == 2 && r.Status == "Acquired" && r.TenantId == tenantIdInt)
            .SumAsync(r => r.TotalAmount ?? 0);

        var spentPending = await _context.Reservations
            .Where(r => r.UserId == agency.Id && r.Type == 2 && (r.Status == "Pending") && r.TenantId == tenantIdInt)
            .SumAsync(r => r.TotalAmount ?? 0);

        var bonus = await _context.BalancePoints
            .Where(bp => bp.UserId == agency.Id && bp.TenantId == tenantIdInt)
            .SumAsync(bp => bp.Points ?? 0);

        var tenantPoints = new TenantPointsSummary
        {
            TenantId = tenantIdInt,
            PointsEarned = earnedPoints,
            PointsPending = pendingPoints,
            PointsSpent = spentPoints,
            PointsSpentPending = spentPending,
            BonusPoints = bonus
        };

        var vm = new EditPointsViewModel
        {
            agency = agency,
            TenantPoints = tenantPoints,
            NewPassword = null,
            ConfirmPassword = null
        };

        return View(vm);
    }



    // POST: Agencies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditPointsViewModel vm)
    {
        if (id != vm.agency.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            // 1) Agency güncelle
            _context.Update(vm.agency);
            await _context.SaveChangesAsync();

            // 2) Kullanıcı şifresini güncelle (isterse)
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                var user = await _userManager.FindByIdAsync(vm.agency.Id);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                    return View(vm);
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, resetToken, vm.NewPassword);

                if (!passResult.Succeeded)
                {
                    foreach (var err in passResult.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);

                    return View(vm);
                }
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Agencies.Any(a => a.Id == vm.agency.Id))
                return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }



    [HttpGet]
    public async Task<IActionResult> Active(string id)
    {
        var agency = await _context.Agencies.FindAsync(id);
        if (agency == null) return NotFound();

        // Kullanıcıyı al
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Rollerini al
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var tenantClaim = roleClaims.FirstOrDefault(c => c.Type == "TenantId");

            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int tenantId))
            {
                // DavetKodlari içinde sadece bu tenant'a ait olan ve AgencyId eşleşen davetleri aktif et
                var davet = await _context.DavetKodlari
                    .FirstOrDefaultAsync(d => d.AgencyId == id && d.TenantId == tenantId);

                if (davet != null)
                {
                    davet.IsActive = true;
                    _context.Update(davet);
                    await _context.SaveChangesAsync();
                }

                break; // İlk eşleşen tenant üzerinden işlem tamamlandıktan sonra çık
            }
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Passive(string id)
    {
        var agency = await _context.Agencies.FindAsync(id);
        if (agency == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var tenantClaim = roleClaims.FirstOrDefault(c => c.Type == "TenantId");

            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out int tenantId))
            {
                var davet = await _context.DavetKodlari
                    .FirstOrDefaultAsync(d => d.AgencyId == id && d.TenantId == tenantId);

                if (davet != null)
                {
                    davet.IsActive = false;
                    _context.Update(davet);
                    await _context.SaveChangesAsync();
                }

                break;
            }
        }

        return RedirectToAction(nameof(Index));
    }



    // GET: Agencies/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null) return NotFound();
        //var agencyIds = await GetAuthorizedAgencyIdsAsync();

        var agency = await _context.Agencies
            .FirstOrDefaultAsync(m => m.Id == id );
        if (agency == null) return Unauthorized();

        return View(agency);
    }

    // POST: Agencies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        // Bu acentaya bağlı rezervasyon var mı?
        var hasReservations = await _context.Reservations
            .AnyAsync(r => r.AgencyId == id);

        if (hasReservations)
        {
            TempData["Error"] = "Bu acentaya ait rezervasyonlar olduğu için silemezsiniz.";
            return RedirectToAction(nameof(Index));
        }

        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Unauthorized();

        var myRoles = await _userManager.GetRolesAsync(me);
        int? tenantId = null;
        foreach (var rn in myRoles)
        {
            var role = await _roleManager.FindByNameAsync(rn);
            if (role == null) continue;
            var rclaims = await _roleManager.GetClaimsAsync(role);
            var t = rclaims.FirstOrDefault(c => c.Type == "TenantId");
            if (t != null && int.TryParse(t.Value, out var parsed)) { tenantId = parsed; break; }
        }

        int tenantIdInt = tenantId.Value;

        var invite = _context.DavetKodlari
            .AsNoTracking()
            .Where(d => d.TenantId == tenantIdInt && d.AgencyId == id)
            .FirstOrDefault();

        _context.DavetKodlari.Remove(invite);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    private bool AgencyExists(string id)
    {
        return _context.Agencies.Any(e => e.Id == id);
    }



    [HttpGet]
    public async Task<IActionResult> Harcamalar()
    {
        // 1) TenantId'yi role claim'den al
        var tenantId = await GetCurrentTenantId();
        if (tenantId == null)
            return Forbid(); // güvenlik

        int tenantIdInt = tenantId.Value;

        // 2) Sadece bu tenant'a ait "harcama" rezervasyonları (Type == 2)
        var reservations = await _context.Reservations
            .Include(r => r.Agency)
            .Include(r => r.Tenant)
            .Include(r => r.Passengers)
            .Where(r => r.Type == 2 && r.TenantId == tenantIdInt)
            .ToListAsync();

        // 3) Kullanıcı bilgisi ekleme
        var model = new List<ReservationWithUserViewModel>();

        foreach (var reservation in reservations)
        {
            string userName = "";
            if (!string.IsNullOrEmpty(reservation.UserId))
            {
                var user = await _userManager.FindByIdAsync(reservation.UserId);
                userName = user?.UserName ?? "";
            }

            model.Add(new ReservationWithUserViewModel
            {
                Reservation = reservation,
                UserName = userName
            });
        }

        return View(model);
    }




}
