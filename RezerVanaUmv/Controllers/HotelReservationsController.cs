using FuzzySharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using RezerVanaUmv.ViewModels;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;




namespace RezerVanaUmv.Controllers
{
    [Authorize(Policy = "OtelCrwAuthPolicy")]
    public class HotelReservationsController : Controller
    {

        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public HotelReservationsController(
            RzvnUmvUmvKrmnBlzrContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<AppUserRoles> roleManager,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || !userRoles.Any())
                return Forbid();

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

            // ✅ SON 24 AY FİLTRESİ
            var limitDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-24));

            var reservations = await _context.Reservations
                .AsNoTracking() // ⚡ Bonus hız
                .Include(r => r.Agency)
                .Include(r => r.Tenant)
                .Include(r => r.Passengers)
                 .Include(r => r.Operator)
                .Where(r =>
                    r.TenantId == parsedTenantId &&
                    r.Status == "Pending" &&
                    r.Type == 1 &&
                    r.CheckinDate >= limitDate
                )
                .OrderByDescending(r => r.ReservationDate) // Liste daha düzenli
                .ToListAsync();

            return View(reservations);
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmedReservations()
        {
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

            var tenantId = await _context.RoleClaims
                .Where(rc => rc.ClaimType == "TenantId" && roleIds.Contains(rc.RoleId))
                .Select(rc => rc.ClaimValue)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(tenantId))
                return Forbid(); // TenantId yoksa erişim engellenir

            int parsedTenantId = int.Parse(tenantId);

            var limitDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-24));

            // 4️⃣ Sadece bu TenantId'ye ait rezervasyonları getir
            var reservations = await _context.Reservations
            .Where(r => r.TenantId == parsedTenantId
             && r.Status == "Acquired"
             && r.CheckinDate >= limitDate)
            .Include(r => r.Agency)
            .Include(r => r.Tenant)
            .Include(r => r.Passengers)
            .Include(r => r.Operator)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> RefusedReservations()
        {
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

            var tenantId = await _context.RoleClaims
                .Where(rc => rc.ClaimType == "TenantId" && roleIds.Contains(rc.RoleId))
                .Select(rc => rc.ClaimValue)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(tenantId))
                return Forbid(); // TenantId yoksa erişim engellenir

            int parsedTenantId = int.Parse(tenantId);

            var limitDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-24));

            var reservations = await _context.Reservations
                 .Where(r =>
                     r.TenantId == parsedTenantId &&
                     r.Status == "Cancel" &&          // veya "Acquired", "Pending" vs.
                     r.CheckinDate >= limitDate
                 )
                 .Include(r => r.Agency)
                 .Include(r => r.Tenant)
                 .Include(r => r.Passengers)
                 .Include(r => r.Operator)
                 .OrderByDescending(r => r.ReservationDate)
                 .ToListAsync();

            return View(reservations);
        }



        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Agency)
                .Include(r => r.Tenant)
                .Include(r=> r.Passengers)
                .Include(r => r.Operator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        [HttpGet]
        public IActionResult GetRoomTypesByTenant(int tenantId)
        {
            var roomTypes = _context.RoomTypes
                .Where(rt => rt.TenantId == tenantId)
                .Select(rt => new { rt.Id, rt.Name }) // veya başka kolon
                .ToList();

            return Json(roomTypes);
        }

        // GET: Reservations/Create
        //[Authorize(Policy = "AgencyCrwAuthPolicy")]
        public IActionResult Create()
        {
            // Giriş yapan kullanıcının DavetKodu claim'ini al
            var davetKodu = User.Claims.FirstOrDefault(c => c.Type == "DavetKodu")?.Value;

            string? agencyId = null;

            if (!string.IsNullOrEmpty(davetKodu))
            {
                var davet = _context.DavetKodlari.FirstOrDefault(x => x.DavetKodu == davetKodu);
                if (davet != null)
                {
                    agencyId = davet.AgencyId;
                }
            }

            ViewBag.AgencyId = agencyId;

            // Tenant selectbox için yine gerekebilir
            ViewData["TenantId"] = new SelectList(_context.Tenants, "Id", "Id");

            // RoomType boş geliyor, Tenant seçince dolacak
            ViewData["RoomTypeId"] = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationWithPassengersViewModel model)
        {
            if (ModelState.IsValid)
            {
                var reservation = model.Reservation;
                reservation.ReservationDate = DateTime.Now;
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                foreach (var p in model.Passengers)
                {
                    var passenger = new Passenger
                    {
                        TenantId = reservation.TenantId,
                        CreatedAt = DateTime.Now,
                        BirthDate = p.BirthDate,
                        FirstName = p.FirstName.ToUpper(),
                        LastName = p.LastName.ToUpper(),
                        ReservationId = reservation.Id
                    };

                    _context.Passengers.Add(passenger);
                    await _context.SaveChangesAsync();
    
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "HotelReservations");
            }
            ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", model.Reservation.TenantId);
            ViewBag.AgencyId = new SelectList(_context.Agencies, "Id", "Name", model.Reservation.AgencyId);
            return View(model);
        }


        public sealed class HotspotResponse
        {
            public bool process { get; set; }
            public string? error { get; set; }
            public List<MemberRecord> listismember { get; set; } = new();
        }

        public sealed class MemberRecord
        {
            public bool process { get; set; }
            public int reservationid { get; set; }

            // "7402" gibi oda NO dönebiliyor
            public string? room { get; set; }

            public DateTime checkindate { get; set; }
            public DateTime checkOutdate { get; set; }
            public string? agency { get; set; }
            public string? board { get; set; }
            public string? voucherno { get; set; }
            public string? firstname { get; set; }
            public string? lastname { get; set; }

            // 🔴 Sedna'nın alanı "dateofbirth"
            [JsonPropertyName("dateofbirth")]
            public DateTime? dateofbirth { get; set; }
        }

        private static string Norm(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Trim();
            s = s.Replace('İ', 'I').Replace('ı', 'i')
                 .Replace('Ş', 'S').Replace('ş', 's')
                 .Replace('Ğ', 'G').Replace('ğ', 'g')
                 .Replace('Ü', 'U').Replace('ü', 'u')
                 .Replace('Ö', 'O').Replace('ö', 'o')
                 .Replace('Ç', 'C').Replace('ç', 'c');
            s = Regex.Replace(s, @"\s+", " ");
            return s.ToUpperInvariant();
        }

        private static string NormVoucher(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return Regex.Replace(s.Trim().ToUpperInvariant(), @"\s+", "");
        }

        private static bool SameDate(DateOnly? d, DateTime dt)
        {
            if (!d.HasValue) return false;
            return d.Value == DateOnly.FromDateTime(dt);
        }

        private static int NameSimilarity(string? f1, string? l1, string? f2, string? l2)
        {
            var a = $"{Norm(f1)} {Norm(l1)}";
            var b = $"{Norm(f2)} {Norm(l2)}";
            return FuzzySharp.Fuzz.Ratio(a, b); // 0..100
        }

        public class ApproveResultViewModel
        {
            public int ReservationId { get; set; }
            public string? Message { get; set; }
            public List<MemberRecord> SimilarRecords { get; set; } = new();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (reservation == null)
                return NotFound();

            // API çağrısı (timeoutlu)
            var client = _httpClientFactory.CreateClient("Sedna");
            client.Timeout = TimeSpan.FromSeconds(12);

            HotspotResponse? api;

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/v1/hotspot/GetHotSpotIsMemberInhouse")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(new { HotspotKey = "8q6a3ttgjelr97j8ko03" }),
                        Encoding.UTF8, "application/json")
                };

                using var res = await client.SendAsync(req, ct);

                if (!res.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Sedna API hata: {(int)res.StatusCode}";
                    return RedirectToAction("Index", "HotelReservations");
                }

                var json = await res.Content.ReadAsStringAsync(ct);

                // 🔴 BREAKPOINT KOYULACAK EN TATLI YERLERDEN BİRİ
                // burada 'json' içinde ham string var
                api = JsonSerializer.Deserialize<HotspotResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 🔴 İKİNCİ BREAKPOINT BURAYA
                // api ve api.listismember'i burada detaylı inceleyebilirsin
                var guests = api?.listismember ?? new();

                // Şimdilik sadece kaç kayıt geldiğini TempData ile görelim
                TempData["Success"] = $"Sedna API OK. Gelen kayıt sayısı: {guests.Count}";
                // İstersen json'ı da loglayabilirsin
                //_logger.LogInformation("Sedna JSON: {Json}", json);

                return RedirectToAction("Index", "HotelReservations");
            }
            catch (OperationCanceledException)
            {
                TempData["Error"] = "Sedna API zaman aşımı.";
                return RedirectToAction("Index", "HotelReservations");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Sedna API istisna: {ex.Message}";
                return RedirectToAction("Index", "HotelReservations");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAction(int id, string decision, CancellationToken ct)
        {
            var reservation = await _context.Reservations.FindAsync(new object[] { id }, ct);
            if (reservation == null) return NotFound();

            switch (decision)
            {
                case "approve":
                    reservation.Status = "Acquired";
                    TempData["Success"] = "Reservation approved manually.";
                    break;
                case "cancel":
                    reservation.Status = "Cancelled";
                    TempData["Warning"] = "Reservation cancelled by user.";
                    break;
                case "pending":
                    TempData["Info"] = "Reservation kept pending.";
                    break;
                default:
                    TempData["Error"] = "Unknown action.";
                    break;
            }

            await _context.SaveChangesAsync(ct);
            return RedirectToAction("Index", "HotelReservations");
        }



            [HttpGet]
    public async Task<IActionResult> ApproveManually(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Passengers)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
            return NotFound();

        var vm = new ReservationWithPassengersViewModel
        {
            Reservation = reservation,

            // Entity Passenger -> PassengerInputModel map
            Passengers = reservation.Passengers?
                .Select(p => new PassengerInputModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Gender = p.Gender,
                    BirthDate = p.BirthDate
                })
                .ToList()
                ?? new List<PassengerInputModel>(),

            // İstersen burayı da doldur:
            TotalAmount = (int)reservation.TotalAmount
            // veya tipin decimal ise:
            // TotalAmount = (int)Math.Round(reservation.TotalAmount ?? 0)
        };

        // Eğer bu view Edit.cshtml ise:
        // return View("Edit", vm);

        // Eğer ApproveManually.cshtml diye ayrı view varsa:
        return View(vm);
    }



         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveManually(int id, CancellationToken ct)
        {
            // 1) Rezervasyonu çek
            var reservation = await _context.Reservations
                .Include(r => r.Passengers)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (reservation == null)
                return NotFound();

            // 2) Status'ü ACQUIRED yap
            var oldStatus = reservation.Status;          // İstersen loglamak için sakladım
            reservation.Status = "Acquired";

            // 3) DB'ye kaydet
            await _context.SaveChangesAsync(ct);

            // 4) Kullanıcıya mesaj
            var passengerCount = reservation.Passengers?.Count ?? 0;

            TempData["Success"] =
                $"Rezervasyon manuel olarak onaylandı. " +
                $"Eski durum: {oldStatus}, yeni durum: Acquired. " +
                $"Yolcu sayısı: {passengerCount}.";

            return RedirectToAction("Index", "HotelReservations");
        }


        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPoint(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Acquired";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "HotelReservations");
        }
        */

        // GET: Reservations/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit(ReservationWithPassengersViewModel model)
        {

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
                    .FirstOrDefaultAsync(r => r.Id == model.Reservation.Id);

                if (reservation == null)
                    return NotFound();

                // 📝 Rezervasyon bilgilerini güncelle
                reservation.CheckinDate = model.Reservation.CheckinDate;
                reservation.CheckoutDate = model.Reservation.CheckoutDate;
                reservation.RoomCount = model.Reservation.RoomCount;
                reservation.RoomType = model.Reservation.RoomType;
                reservation.TotalAmount = model.Reservation.TotalAmount;
                reservation.BookingReference = model.Reservation.BookingReference;
                reservation.ReservationDate = model.Reservation.ReservationDate;
                reservation.NightCount = model.Reservation.NightCount;

                // 👥 Mevcut yolcuların listesini al
                var existingPassengers = reservation.Passengers.ToList();

                // 🔁 Yeni ve mevcut yolcuları güncelle veya ekle
                foreach (var updatedPassenger in model.Passengers)
                {
                    if (updatedPassenger.Id == 0)
                    {
                        reservation.Passengers.Add(new Passenger
                        {
                            FirstName = updatedPassenger.FirstName?.ToUpper(),
                            LastName = updatedPassenger.LastName?.ToUpper(),
                            Gender = updatedPassenger.Gender,
                            BirthDate = updatedPassenger.BirthDate,
                            ReservationId = reservation.Id
                        });
                    }
                    else
                    {
                        var existing = existingPassengers.FirstOrDefault(p => p.Id == updatedPassenger.Id);
                        if (existing != null)
                        {
                            existing.FirstName = updatedPassenger.FirstName?.ToUpper();
                            existing.LastName = updatedPassenger.LastName?.ToUpper();
                            existing.Gender = updatedPassenger.Gender;
                            existing.BirthDate = updatedPassenger.BirthDate;
                        }
                    }
                }

                // ❌ Silinen yolcuları işaretle ve sil
                if (!string.IsNullOrEmpty(Request.Form["DeletedPassengerIds"]))
                {
                    var deletedIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(Request.Form["DeletedPassengerIds"]);

                    foreach (var passengerId in deletedIds)
                    {
                        var toDelete = reservation.Passengers.FirstOrDefault(p => p.Id == passengerId);
                        if (toDelete != null)
                        {
                            _context.Passengers.Remove(toDelete);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                if (model.Reservation.Status == "Acquired")
                {
                    return RedirectToAction("ConfirmedReservations", "HotelReservations");
                }
                else
                {
                    return RedirectToAction("Index", "HotelReservations");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(model.Reservation.Id))
                    return NotFound();
                else
                    throw;
            }
        }


        public async Task<IActionResult> Delete(int? id)
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

            return View( reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = "Cancel";
            await _context.SaveChangesAsync();

            return RedirectToAction("ConfirmedReservations", "HotelReservations");
            /*
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

            return RedirectToAction("Index", "HotelReservations");
            */
        }


        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
