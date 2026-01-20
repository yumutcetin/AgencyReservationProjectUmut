using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using RezerVanaUmv.ViewModels;
using System.Linq;
using System.Security.Claims;
using RabbitMQ.Client;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace RezerVanaUmv.Controllers
{
    [Authorize(Policy = "AgencyCrwAuthPolicy")]
    public class AgencyReservationsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<AppUserRoles> _roleManager;
        private readonly ILogger<AgencyReservationsController> _logger;

        public AgencyReservationsController(
             RzvnUmvUmvKrmnBlzrContext context,
             UserManager<ApplicationUser> userManager,
              RoleManager<AppUserRoles> roleManager,
        ILogger<AgencyReservationsController> logger)   // <-- burada alıyoruz
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }



        [HttpGet]
        public async Task<IActionResult> RewardAndRedempt()
        {
            var userId = _userManager.GetUserId(User);

            // 1️⃣ Kullanıcının AgencyId'lerini al
            var agencyIDs = User.Claims
                .Where(c => c.Type == "AgencyId")
                .Select(c => c.Value)
                .ToList();

            if (!agencyIDs.Any())
            {
                return View(new UserTotalPointsViewModel
                {
                    RewardCatalogs = new(),
                    RedemptionRecords = new()
                });
            }

            // 2️⃣ Bağlı olduğu aktif otellerin (TenantId) listesini al
            var tenantIds = await _context.DavetKodlari
                .Where(d => agencyIDs.Contains(d.AgencyId) && d.IsActive && d.TenantId.HasValue)
                .Select(d => d.TenantId.Value)
                .Distinct()
                .ToListAsync();

            // 3️⃣ Bu tenantlara ait ödül kataloglarını getir (kazanılabilecek)
            var rewardCatalogs = await _context.RewardCatalogs
                .Where(r => r.TenantId.HasValue && tenantIds.Contains(r.TenantId.Value))
                .Include(r => r.Tenant)
                .Select(r => new RewardCatalogRecord
                {
                    HotelName = r.Tenant.Name,
                    RoomType = r.RoomType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RequiredPoints = r.RequiredPoints
                })
                .ToListAsync();

            // 4️⃣ Kullanıcının yaptığı harcamaları getir (redemptions)
            var redemptions = await _context.Redemptions
                .Where(r => r.TenantId.HasValue && tenantIds.Contains(r.TenantId.Value))
                .Include(r => r.Tenant)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RedemptionRecord
                {
                    HotelName = r.Tenant.Name,
                    RoomType = r.RoomType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RequiredPoints = r.RequiredPoints
                })
                .ToListAsync();

            // 5️⃣ ViewModel'i hazırla
            var model = new UserTotalPointsViewModel
            {
                RewardCatalogs = rewardCatalogs,
                RedemptionRecords = redemptions
            };

            return View(model);
        }




        [HttpGet]
        public async Task<IActionResult> GetBonusSettingsByTenant(int tenantId)
        {
            var settings = await _context.ReservationBonusSettings
                .Where(s => s.TenantId == tenantId)
                .Select(s => new
                {
                    minReservationDay = s.MinReservationDay,
                    maxReservationDay = s.MaxReservationDay
                })
                .FirstOrDefaultAsync();

            if (settings == null)
                return NotFound();

            return Json(settings);
        }


        // GET: Reservations
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1️⃣ Kullanıcının tüm AgencyId claim'lerini al
            var agencyIds = User.Claims
                .Where(c => c.Type == "AgencyId")
                .Select(c => c.Value)
                .ToList();

            if (!agencyIds.Any())
            {
                // Claim yoksa boş liste dön
                return View(new List<Reservation>());
            }

            // 2️⃣ agencyIds listesindeki herhangi biriyle eşleşen rezervasyonları getir
            var reservations = await _context.Reservations
                .Include(r => r.Agency)
                .Include(r => r.Tenant)
                .Include(r => r.Passengers)
                .Where(r => agencyIds.Contains(r.AgencyId) && r.Type == 1)
                .ToListAsync();

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> HarcamaListesi()
        {
            // 1️⃣ Kullanıcının tüm AgencyId claim'lerini al
            var agencyIds = User.Claims
                .Where(c => c.Type == "AgencyId")
                .Select(c => c.Value)
                .ToList();

            if (!agencyIds.Any())
            {
                // Claim yoksa boş liste dön
                return View(new List<Reservation>());
            }

            // 2️⃣ agencyIds listesindeki herhangi biriyle eşleşen rezervasyonları getir
            var reservations = await _context.Reservations
                .Include(r => r.Agency)
                .Include(r => r.Tenant)
                .Include(r => r.Passengers)
                .Where(r => agencyIds.Contains(r.AgencyId) && r.Type == 2)
                .ToListAsync();

            return View(reservations);
        }


        public async Task<IActionResult> TotalPoints()
        {
            var userId = _userManager.GetUserId(User);

            var agencyIDs = User.Claims
                .Where(c => c.Type == "AgencyId")
                .Select(c => c.Value)
                .ToList();

            var tenantPoints = new List<TenantPointsSummary>();

            if (agencyIDs.Any())
            {
                var tenantIds = await _context.DavetKodlari
                    .Where(d => agencyIDs.Contains(d.AgencyId) && d.IsActive && d.TenantId.HasValue)
                    .Select(d => d.TenantId.Value)
                    .Distinct()
                    .ToListAsync();

                foreach (var tenantId in tenantIds)
                {
                    var tenant = await _context.Tenants.FindAsync(tenantId);
                    if (tenant == null) continue;

                    var earnedPoints = await _context.Reservations
                        .Where(r => r.UserId == userId && r.Type == 1 && r.Status == "Acquired" && r.TenantId == tenantId)
                        .SumAsync(r => r.TotalAmount ?? 0);

                    var pendingPoints = await _context.Reservations
                        .Where(r => r.UserId == userId && r.Type == 1 && r.Status == "Pending"  && r.TenantId == tenantId)
                        .SumAsync(r => r.TotalAmount ?? 0);

                    var spentPoints = await _context.Reservations
                        .Where(r => r.UserId == userId && r.Type == 2 && r.Status == "Acquired" && r.TenantId == tenantId)
                        .SumAsync(r => r.TotalAmount ?? 0);

                    var spentPending = await _context.Reservations
                        .Where(r => r.UserId == userId && r.Type == 2 && r.Status == "Pending" && r.TenantId == tenantId)
                        .SumAsync(r => r.TotalAmount ?? 0);

                    var bonus = await _context.BalancePoints
                        .Where(bp => bp.UserId == userId && bp.TenantId == tenantId)
                        .SumAsync(bp => bp.Points ?? 0);

                    tenantPoints.Add(new TenantPointsSummary
                    {
                        TenantId = tenantId,
                        HotelName = tenant.Name,
                        PointsEarned = earnedPoints,
                        PointsPending = pendingPoints,
                        PointsSpent = spentPoints,
                        PointsSpentPending = spentPending,
                        BonusPoints = bonus
                    });
                }
            }

            var rewardCatalogs = await _context.RewardCatalogs
                .Where(r => r.TenantId.HasValue && tenantPoints.Select(t => t.TenantId).Contains(r.TenantId.Value))
                .Include(r => r.Tenant)
                .Select(r => new RewardCatalogRecord
                {
                    HotelName = r.Tenant.Name,
                    RoomType = r.RoomType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RequiredPoints = r.RequiredPoints
                })
                .ToListAsync();

            var redemptions = await _context.Redemptions
                .Where(r => r.TenantId.HasValue && tenantPoints.Select(t => t.TenantId).Contains(r.TenantId.Value))
                .Include(r => r.Tenant)
                .Select(r => new RedemptionRecord
                {
                    HotelName = r.Tenant.Name,
                    RoomType = r.RoomType,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    RequiredPoints = r.RequiredPoints,
                })
                .ToListAsync();

            return View(new UserTotalPointsViewModel
            {
                TenantPoints = tenantPoints,
                RewardCatalogs = rewardCatalogs,
                RedemptionRecords = redemptions
            });
        }





        // GET: Reservations/Details/5
        [HttpGet]
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
                .Where(r => r.TenantId == tenantId)
                .Select(r => r.Name)
                .ToList();

            // Başa boş seçenek ekle
            roomTypes.Insert(0, "Oda tipi seçiniz");

            // string listesi dön
            return Json(roomTypes);
        }

        [HttpGet]
        public IActionResult GetOperatorsByTenant(int tenantId)
        {
            var operators = _context.Operators
                .Where(o => o.TenantId == tenantId)
                .OrderBy(o => o.Name)
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Name
                })
                .ToList();

            return Json(operators);
        }

        private async Task<decimal> CalculateTotalPointsForUser(string userId)
        {
            var reservationPoints = await _context.Reservations
                .Where(r => r.UserId == userId && r.Status == "Acquired" && r.Type == 1)
                .GroupBy(r => r.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Total = g.Sum(r => r.TotalAmount ?? 0)
                })
                .ToListAsync();

            var usedPoints = await _context.Reservations
                .Where(r => r.UserId == userId &&  r.Status == "Pending" && r.Type == 2)
                .SumAsync(r => r.TotalAmount ?? 0);

            var bonusPoints = await _context.BalancePoints
                .Where(bp => bp.UserId == userId)
                .SumAsync(bp => bp.Points ?? 0);

            var gained = reservationPoints.FirstOrDefault(r => r.Status == "Acquired")?.Total ?? 0;

            return gained + bonusPoints - usedPoints;
        }


        // GET: Reservations/Create
        [Authorize(Policy = "AgencyCrwAuthPolicy")]
        [HttpGet]
        public IActionResult Create()
        {
            var agencyId = User.Claims.FirstOrDefault(c => c.Type == "AgencyId")?.Value;

            ViewBag.AgencyId = agencyId;
            ViewBag.OperatorId = new SelectList(Enumerable.Empty<SelectListItem>());


            // İlgili agency'e ait tenantId'leri al
            var tenantIds = _context.DavetKodlari
                .Where(d => d.AgencyId == agencyId && d.IsActive == true)
                .Select(d => d.TenantId)
                .Distinct()
                .ToList();

            // Sadece bu tenantId'leri içeren tenant kayıtlarını al
            var tenantList = _context.Tenants
                .Where(t => tenantIds.Contains(t.Id))
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

            // Başta boş bir seçenek ekle
            tenantList.Insert(0, new SelectListItem { Value = "", Text = "Bir otel seçiniz" });

            ViewBag.TenantId = tenantList;

            // Oda tipleri dinamik yükleneceği için boş bırakılıyor
            ViewBag.RoomTypeId = new SelectList(Enumerable.Empty<SelectListItem>());

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
                reservation.UserId = _userManager.GetUserId(User);
                reservation.Type = 1;
                reservation.Status = "Pending";

                // 1. Calculate total points from RewardCatalog
                var totalPoints = await CalculateRewardPoints(reservation);
                reservation.TotalAmount = totalPoints;

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                // 2. Add passengers
                foreach (var p in model.Passengers)
                {
                    var passenger = new Passenger
                    {
                        TenantId = reservation.TenantId,
                        CreatedAt = DateTime.Now,
                        BirthDate = p.BirthDate,
                        FirstName = p.FirstName.ToUpper(),
                        LastName = p.LastName.ToUpper(),
                        Gender = p.Gender.ToUpper(),
                        ReservationId = reservation.Id
                    };

                    _context.Passengers.Add(passenger);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "AgencyReservations");
            }

            ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", model.Reservation.TenantId);
            ViewBag.AgencyId = new SelectList(_context.Agencies, "Id", "Name", model.Reservation.AgencyId);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetAvailablePoints(int tenantId)
        {
            var userId = _userManager.GetUserId(User);

            // Calculate total available points for the user for this tenant
            var earnedPoints = await _context.Reservations
                .Where(r => r.UserId == userId && r.Type == 1 && r.Status == "Acquired" && r.TenantId == tenantId)
                .SumAsync(r => r.TotalAmount ?? 0);

            var pendingPoints = await _context.Reservations
                .Where(r => r.UserId == userId && r.Type == 1 && r.Status == "Pending" && r.TenantId == tenantId)
                .SumAsync(r => r.TotalAmount ?? 0);

            var spentPoints = await _context.Reservations
                .Where(r => r.UserId == userId && r.Type == 2 && r.Status == "Acquired" && r.TenantId == tenantId)
                .SumAsync(r => r.TotalAmount ?? 0);

            var pendingSpent = await _context.Reservations
                .Where(r => r.UserId == userId && r.Type == 2 && r.Status == "Pending" && r.TenantId == tenantId)
                .SumAsync(r => r.TotalAmount ?? 0);

            var bonus = await _context.BalancePoints
                .Where(bp => bp.UserId == userId && bp.TenantId == tenantId)
                .SumAsync(bp => bp.Points ?? 0);

            var totalAvailable = earnedPoints + bonus - spentPoints - pendingSpent;

            return Json(totalAvailable);
        }


        [Authorize(Policy = "AgencyCrwAuthPolicy")]
        [HttpGet]
        public async Task<IActionResult> PointUsage()
        {
            var agencyId = User.Claims.FirstOrDefault(c => c.Type == "AgencyId")?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            ViewBag.AgencyId = agencyId;
            ViewBag.OperatorId = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.TotalPoints = "--"; // Default value for initial load


            // İlgili agency'e ait tenantId'leri al
            var tenantIds = _context.DavetKodlari
                .Where(d => d.AgencyId == agencyId)
                .Select(d => d.TenantId)
                .Distinct()
                .ToList();

            // Sadece bu tenantId'leri içeren tenant kayıtlarını al
            var tenantList = _context.Tenants
                .Where(t => tenantIds.Contains(t.Id))
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

            // Başta boş bir seçenek ekle
            tenantList.Insert(0, new SelectListItem { Value = "", Text = "Bir otel seçiniz" });

            ViewBag.TenantId = tenantList;

            // Oda tipleri dinamik yükleneceği için boş bırakılıyor
            ViewBag.RoomTypeId = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PointUsage(ReservationWithPassengersViewModel model, decimal AvailablePoints)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", model.Reservation.TenantId);
                ViewBag.AgencyId = model.Reservation.AgencyId;
                ViewBag.TotalPoints = AvailablePoints;
                return View(model);
            }

            var reservation = model.Reservation;
            reservation.ReservationDate = DateTime.Now;
            reservation.UserId = _userManager.GetUserId(User);
            reservation.Type = 2;
            reservation.Status = "Pending";

            // 1) Giriş / çıkış tarihleri kontrolü
            if (reservation.CheckinDate == null || reservation.CheckoutDate == null)
            {
                ModelState.AddModelError("", "Giriş ve çıkış tarihleri boş olamaz.");

                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", reservation.TenantId);
                ViewBag.AgencyId = reservation.AgencyId;
                ViewBag.TotalPoints = AvailablePoints;
                return View(model);
            }

            var checkin = reservation.CheckinDate.Value.ToDateTime(TimeOnly.MinValue);
            var checkout = reservation.CheckoutDate.Value.ToDateTime(TimeOnly.MinValue);
            var stayDays = (checkout - checkin).Days;   // örn: 01-05 arası = 4 gece

            if (stayDays <= 0)
            {
                ModelState.AddModelError("", "Çıkış tarihi, giriş tarihinden sonra olmalıdır.");

                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", reservation.TenantId);
                ViewBag.AgencyId = reservation.AgencyId;
                ViewBag.TotalPoints = AvailablePoints;
                return View(model);
            }

            // 2) İlgili otel (Tenant) için Bonus Settings’i bul
            var bonusSettings = await _context.ReservationBonusSettings
                .FirstOrDefaultAsync(x => x.TenantId == reservation.TenantId);

            if (bonusSettings == null)
            {
                ModelState.AddModelError("", "Bu otel için puan kullanımı ayarı bulunamadı. Lütfen yönetici ile iletişime geçiniz.");

                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", reservation.TenantId);
                ViewBag.AgencyId = reservation.AgencyId;
                ViewBag.TotalPoints = AvailablePoints;
                return View(model);
            }

            // 3) MinStayDay / MaxStayDay kontrolü
            var minStay = bonusSettings.MinStayDay;
            var maxStay = bonusSettings.MaxStayDay;

            bool minViolation = minStay.HasValue && stayDays < minStay.Value;
            bool maxViolation = maxStay.HasValue && stayDays > maxStay.Value;

            if (minViolation || maxViolation)
            {
                string msg;

                if (minStay.HasValue && maxStay.HasValue)
                {
                    msg = $"Bu otel için puan kullanımı, {minStay.Value} - {maxStay.Value} gece arası rezervasyonlarda geçerlidir. " +
                          $"Seçtiğiniz rezervasyon süresi: {stayDays} gece.";
                }
                else if (minStay.HasValue)
                {
                    msg = $"Bu otel için puan kullanımı en az {minStay.Value} gece konaklamalarda geçerlidir. " +
                          $"Seçtiğiniz rezervasyon süresi: {stayDays} gece.";
                }
                else // sadece maxStay var
                {
                    msg = $"Bu otel için puan kullanımı en fazla {maxStay!.Value} gece konaklamalarda geçerlidir. " +
                          $"Seçtiğiniz rezervasyon süresi: {stayDays} gece.";
                }

                ModelState.AddModelError("", msg);

                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", reservation.TenantId);
                ViewBag.AgencyId = reservation.AgencyId;
                ViewBag.TotalPoints = AvailablePoints;
                return View(model);
            }

            // 🔴 4) MinBalance (minimum puan bakiyesi) kontrolü
            // Bu otelde puan kullanabilmek için hesabında en az MinBalance kadar puan olmalı
            if (bonusSettings.MinBalance > 0 && AvailablePoints < bonusSettings.MinBalance)
            {
                ModelState.AddModelError("",
                    $"Bu otelde puan kullanımı için hesabınızda en az {bonusSettings.MinBalance:N0} puan bulunmalıdır. " +
                    $"Mevcut puan bakiyeniz: {AvailablePoints:N0}.");

                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", reservation.TenantId);
                ViewBag.AgencyId = reservation.AgencyId;
                ViewBag.TotalPoints = AvailablePoints;
                return View(model);
            }

            // 5) Puan hesapla (rezervasyon için gereken puan)
            var totalPointsRequired = await CalculateRedempitonPoints(reservation);

            // 5.1) Yeterli puanı var mı? (bu rezervasyon için)
            if (AvailablePoints < totalPointsRequired)
            {
                ModelState.AddModelError("", "Yeterli puanınız bulunmamaktadır.");

                var tenantId = reservation.TenantId;
                var agencyId = reservation.AgencyId;

                ViewBag.TenantId = new SelectList(_context.Tenants, "Id", "Name", tenantId);
                ViewBag.AgencyId = agencyId;
                ViewBag.TotalPoints = AvailablePoints;

                return View(model);
            }

            // 6) Rezervasyonu kaydet
            reservation.TotalAmount = (int?)totalPointsRequired;
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // 7) Yolcuları kaydet
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
            }

            await _context.SaveChangesAsync();

            // 8) BONUS INFO e-postasını MQ'ya at
            await EnqueueBonusInfoEmailAsync(reservation.Id);

            return RedirectToAction("Index", "AgencyReservations");
        }



        private async Task EnqueueBonusInfoEmailAsync(int reservationId)
        {
            // Rezervasyon + Tenant + Agency bilgileri
            var res = await _context.Reservations
                .Include(r => r.Tenant)
                .Include(r => r.Agency)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (res == null) return;

            // ReservationBonusSettings üzerinden bonus info mailini al
            var bonusInfo = await _context.ReservationBonusSettings
                .Include(s => s.Tenant)
                .Where(s => s.TenantId == res.TenantId)
                .Select(s => new
                {
                    TenantName = s.Tenant.Name,
                    BonusInfoEmail = s.BonusInfoEmail // <-- sende alan adı farklıysa burada düzelt
                })
                .FirstOrDefaultAsync();

            // Mail adresi yoksa güvenli çık
            if (bonusInfo == null || string.IsNullOrWhiteSpace(bonusInfo.BonusInfoEmail))
                return;

            // Konu ve Body
            string subject = $"[Points Redemption] Tenant: {bonusInfo.TenantName} | Reservation #{res.Id}";
            string detailsUrl = Url.Action(
                action: "Details",
                controller: "AgencyReservations",
                values: new { id = res.Id },
                protocol: Request.Scheme);

            var bodyHtml = $@"
                    Dear Bonus Team,<br><br>
                    A new points redemption request has been created.<br><br>
                    <b>Reservation ID:</b> {res.Id}<br>
                    <b>Tenant:</b> {bonusInfo.TenantName}<br>
                    <b>Agency:</b> {res.Agency?.Name}<br>
                    <b>Date:</b> {res.ReservationDate:yyyy-MM-dd HH:mm}<br>
                    <b>Total Points:</b> {res.TotalAmount}<br>
                    <b>Status:</b> {res.Status}<br><br>
                    You can view details from the link below:<br>
                    <a href="">Open Reservation Details</a><br><br>
                    Kind regards.
                ";

            // RabbitMQ bağlantısı
            var factory = new ConnectionFactory
            {
                HostName = "157.90.161.220",
                Port = 5672,
                UserName = "guest",
                Password = "adminumv"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Worker'ının dinlediği kuyruk
            channel.QueueDeclare(
                queue: "email_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // İstersen Agency veya Tenant genel e-postasını CC yapabilirsin
            string ccAddress = res.Tenant?.ContactEmail; // alan yoksa null kalsın

            var emailMQ = new EmailMQ
            {
                email = bonusInfo.BonusInfoEmail,
                mailsubject = subject,
                mailbody = bodyHtml,
                cc = ccAddress
            };

            var json = System.Text.Json.JsonSerializer.Serialize(emailMQ);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: "",
                routingKey: "email_queue",
                basicProperties: null,
                body: body);
        }


        [Authorize(Policy = "AgencyCrwAuthPolicy")]
        [HttpGet]
        public IActionResult PointUsageList()
        {
            /*
            var agencyId = User.Claims.FirstOrDefault(c => c.Type == "AgencyId")?.Value;

            ViewBag.AgencyId = agencyId;

            // İlgili agency'e ait tenantId'leri al
            var tenantIds = _context.DavetKodlari
                .Where(d => d.AgencyId.ToString() == agencyId)
                .Select(d => d.TenantId)
                .Distinct()
                .ToList();

            // Sadece bu tenantId'leri içeren tenant kayıtlarını al
            var tenantList = _context.Tenants
                .Where(t => tenantIds.Contains(t.Id))
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

            // Başta boş bir seçenek ekle
            tenantList.Insert(0, new SelectListItem { Value = "", Text = "Bir otel seçiniz" });

            ViewBag.TenantId = tenantList;

            // Oda tipleri dinamik yükleneceği için boş bırakılıyor
            ViewBag.RoomTypeId = new SelectList(Enumerable.Empty<SelectListItem>());
            */
            return View();
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


        private async Task<decimal> CalculateRedempitonPoints(Reservation reservation)
        {
            if (reservation.CheckinDate == null || reservation.CheckoutDate == null || reservation.TenantId == null)
                return 0;

            // Convert DateOnly to DateTime for comparisons
            var checkin = reservation.CheckinDate.Value.ToDateTime(TimeOnly.MinValue);
            var checkout = reservation.CheckoutDate.Value.ToDateTime(TimeOnly.MinValue);

            // Exclude the checkout day from calculation
            var actualCheckout = checkout.AddDays(-1);

            var catalogs = await _context.Redemptions
                .Where(c => c.TenantId == reservation.TenantId
                            && c.RoomType == reservation.RoomType
                            && c.StartDate <= actualCheckout
                            && c.EndDate >= checkin)
                .ToListAsync();

            if (!catalogs.Any()) return 0;

            decimal totalPoints = 0;

            foreach (var catalog in catalogs)
            {
                // Catalog's valid date range
                var catalogStart = catalog.StartDate!.Value.Date;
                var catalogEnd = catalog.EndDate!.Value.Date;

                // Calculate overlapping date range, still excluding the checkout date
                var overlapStart = (catalogStart > checkin ? catalogStart : checkin).Date;
                var overlapEnd = (catalogEnd < actualCheckout ? catalogEnd : actualCheckout).Date;

                if (overlapEnd < overlapStart)
                    continue;

                int overlapNights = (int)(overlapEnd - overlapStart).TotalDays + 1;

                totalPoints += overlapNights * catalog.RequiredPoints;
            }

            return totalPoints;
        }



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
        public async Task<IActionResult> Edit(int id, ReservationWithPassengersViewModel model)
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
                return RedirectToAction("Index", "AgencyReservations");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(model.Reservation.Id))
                    return NotFound();
                else
                    throw;
            }
        }



        [HttpGet]
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

            return View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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

            return RedirectToAction("Index", "AgencyReservations");
        }


        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }



        
    }
}
