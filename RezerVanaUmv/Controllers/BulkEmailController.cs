using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using RezerVanaUmv.ViewModels;

[Authorize]
public class BulkEmailController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppUserRoles> _roleManager;
    private readonly RzvnUmvUmvKrmnBlzrContext _context;

    public BulkEmailController(
        UserManager<ApplicationUser> userManager,
        RoleManager<AppUserRoles> roleManager,
        RzvnUmvUmvKrmnBlzrContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [Authorize(Policy = "OtelCrwAuthPolicy")]

    // Ortak: aktif kullanıcının TenantId'sini role claim'lerinden bul
    private async Task<int?> GetCurrentTenantIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var claims = await _roleManager.GetClaimsAsync(role);
            var tenantClaim = claims.FirstOrDefault(c => c.Type == "TenantId");
            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var tid))
                return tid;
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var tenantId = await GetCurrentTenantIdAsync();
        if (tenantId == null) return Forbid();

        // 1) Bu tenant'a bağlı ajanslar
        var davetAgencyIds = await _context.DavetKodlari
            .Where(c => c.TenantId == tenantId && c.AgencyId != null)
            .Select(c => c.AgencyId!)
            .Distinct()
            .ToListAsync();

        // 2) Ajansların kendisi
        var agencies = await _context.Agencies
            .Where(a => davetAgencyIds.Contains(a.Id))
            .OrderBy(a => a.Name)
            .ToListAsync();

        // 3) Ajans başına kullanıcı sayısı
        var counts = await _context.UserClaims
            .Where(uc => uc.ClaimType == "AgencyId" && davetAgencyIds.Contains(uc.ClaimValue!))
            .GroupBy(uc => uc.ClaimValue!)
            .Select(g => new
            {
                AgencyId = g.Key,
                Count = g.Select(x => x.UserId).Distinct().Count()
            })
            .ToListAsync();

        // 4) ViewModel doldur
        var model = new BulkEmailViewModel();

        model.Agencies = agencies.Select(a => new AgencyWithCountVM
        {
            Id = a.Id,
            Name = $"{a.Name} ({counts.FirstOrDefault(c => c.AgencyId == a.Id)?.Count ?? 0})",
            UserCount = counts.FirstOrDefault(c => c.AgencyId == a.Id)?.Count ?? 0,
            IsSelected = false      // şu an create ekranı ∴ default false
        }).ToList();

        // 5) MultiSelectList için seçili ID’leri buraya dolduruyoruz
        model.SelectedAgencyIds = model.Agencies
            .Where(a => a.IsSelected)
            .Select(a => a.Id)
            .ToList();

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BulkEmailViewModel model)
    {
        var tenantId = await GetCurrentTenantIdAsync();
        if (tenantId == null) return Forbid();

        if (string.IsNullOrWhiteSpace(model.Subject))
            ModelState.AddModelError(nameof(model.Subject), "Konu zorunludur.");
        if (string.IsNullOrWhiteSpace(model.BodyHtml))
            ModelState.AddModelError(nameof(model.BodyHtml), "E-posta gövdesi zorunludur.");

        if (!model.SendToAllInTenant &&
            (model.SelectedAgencyIds == null || model.SelectedAgencyIds.Count == 0) &&
            string.IsNullOrWhiteSpace(model.AdditionalEmails))
        {
            ModelState.AddModelError(string.Empty, "Alıcı seçin: Oteldeki tüm kullanıcılar, ajans seçimi veya ekstra e-postalar.");
        }

        async Task RefillAgenciesAsync()
        {
            var davetAgencyIds = await _context.DavetKodlari
                .Where(c => c.TenantId == tenantId && c.AgencyId != null)
                .Select(c => c.AgencyId!)
                .Distinct()
                .ToListAsync();

            var agencies = await _context.Agencies
                .Where(a => davetAgencyIds.Contains(a.Id))
                .ToListAsync();

            var counts = await _context.UserClaims
                .Where(uc => uc.ClaimType == "AgencyId" && davetAgencyIds.Contains(uc.ClaimValue!))
                .GroupBy(uc => uc.ClaimValue!)
                .Select(g => new { AgencyId = g.Key, Count = g.Select(x => x.UserId).Distinct().Count() })
                .ToListAsync();

            model.Agencies = agencies.Select(a => new AgencyWithCountVM
            {
                Id = a.Id,
                Name = a.Name,
                UserCount = counts.FirstOrDefault(c => c.AgencyId == a.Id)?.Count ?? 0,
                IsSelected = model.SelectedAgencyIds?.Contains(a.Id) ?? false
            }).ToList();
        }

        if (!ModelState.IsValid)
        {
            await RefillAgenciesAsync();
            return View(model);
        }

        async Task<List<string>> GetUserEmailsByClaimAsync(string claimType, IEnumerable<string> claimValues)
        {
            var vals = claimValues.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToList();
            if (vals.Count == 0) return new();

            var query =
                from u in _context.Users
                join uc in _context.UserClaims on u.Id equals uc.UserId
                where u.Email != null
                   && uc.ClaimType == claimType
                   && vals.Contains(uc.ClaimValue!)
                select u.Email!;

            return await query.Distinct().ToListAsync();
        }

        // ======= Alıcılar =======
        var recipientSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1) Oteldeki (tenant) TÜM kullanıcılar — onaylı/onaysız AYIRT ETME!
        if (model.SendToAllInTenant)
        {
            var tenantEmails = await GetUserEmailsByClaimAsync("TenantId", new[] { tenantId.Value.ToString() });
            foreach (var mail in tenantEmails) recipientSet.Add(mail);
        }

        // 2) Seçili ajanslar (opsiyonel)
        if (model.SelectedAgencyIds != null && model.SelectedAgencyIds.Count > 0)
        {
            var agencyEmails = await GetUserEmailsByClaimAsync("AgencyId", model.SelectedAgencyIds);
            foreach (var mail in agencyEmails) recipientSet.Add(mail);
        }

        // 3) Ekstra e-postalar (opsiyonel)
        if (!string.IsNullOrWhiteSpace(model.AdditionalEmails))
        {
            foreach (var mail in model.AdditionalEmails.Replace(";", ",")
                         .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                recipientSet.Add(mail);
        }

        if (recipientSet.Count == 0)
        {
            TempData["Error"] = "Uygun alıcı bulunamadı.";
            await RefillAgenciesAsync();
            return View(model);
        }

        // ======= Kuyruğa gönder =======
        var factory = new ConnectionFactory
        {
            HostName = "157.90.161.220",
            Port = 5672,
            UserName = "guest",
            Password = "adminumv"
        };

        int queued = 0;
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare("rezervana_queue", 
            durable: false, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null);

        foreach (var mail in recipientSet)
        {
            var emailMQ = new EmailMQ
            {
                email = mail,
                mailsubject = model.Subject!,
                mailbody = model.BodyHtml!,
                cc = null
            };

            var json = System.Text.Json.JsonSerializer.Serialize(emailMQ);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", 
                routingKey: "rezervana_queue", 
                basicProperties: null,
                body: body);
            queued++;
        }

        TempData["Success"] = $"{queued} e-posta kuyruğa eklendi.";
        return RedirectToAction(nameof(Create));
    }


}
