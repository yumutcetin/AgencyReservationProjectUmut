using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Encryption.Identity;
using RezerVanaUmv.Identity;
using RezerVanaUmv.Middleware;
using RezerVanaUmv.Models;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<RzvnUmvUmvKrmnBlzrContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity with Role support
builder.Services.AddIdentity<ApplicationUser, AppUserRoles>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;

    options.Password.RequireDigit = true;              // Rakam zorunlu
    options.Password.RequireLowercase = true;          // Küçük harf zorunlu
    options.Password.RequireUppercase = true;          // Büyük harf zorunlu
    options.Password.RequireNonAlphanumeric = true;    // Özel karakter zorunlu (*, !, ?, vb.)
    options.Password.RequiredLength = 8;               // Minimum 8 karakter
    options.Password.RequiredUniqueChars = 1;          // En az 1 farklı karakter
})
.AddEntityFrameworkStores<RzvnUmvUmvKrmnBlzrContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LogoutPath = new PathString("/Home/Logout");
    options.LoginPath = new PathString("/Home/Login");
    options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied"); // DÜZELTİLDİ ✅

    options.Cookie = new CookieBuilder
    {
        Name = "IdentityCookie",
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.SameAsRequest
    };

    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(60); // 60 gün süreyle aktif kalır
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AgencyCrwAuthPolicy", policy =>
    policy.Requirements.Add(new AgencyCrwAuthRequirement()));
    options.AddPolicy("OtelCrwAuthPolicy", policy =>
    policy.Requirements.Add(new OtelCrwAuthRequirement()));
    options.AddPolicy("AdminPolicy", policy =>
         policy.Requirements.Add(new AdminRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, OtelCrwAuthRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AgencyCrwAuthHandler>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddHttpClient("Sedna", c =>
{
    c.BaseAddress = new Uri("https://api.sedna360.com/");
    c.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
    c.DefaultRequestVersion = new Version(2, 0);
    c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
    PooledConnectionLifetime = TimeSpan.FromMinutes(6),   // DNS yenile
    MaxConnectionsPerServer = 20,                         // eşzamanlılık
    ConnectTimeout = TimeSpan.FromSeconds(5)
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");                // 500 için
    app.UseStatusCodePagesWithReExecute("/Error/{0}"); // 404, 403 gibi durumlar için
    app.UseHsts();
}

app.UseHttpsRedirection(); // En başta olsun
app.UseStaticFiles();



var supportedCultures = new[]
{
    new CultureInfo("tr-TR"),
    new CultureInfo("en-US"),
    new CultureInfo("ru-RU"),
    new CultureInfo("de-DE"),
    new CultureInfo("ko-KR"),
    new CultureInfo("zh-CN")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// Önce kullanıcı seçimi (cookie), yoksa tarayıcı dili (Accept-Language)
localizationOptions.RequestCultureProviders = new IRequestCultureProvider[]
{
    new CookieRequestCultureProvider(),
    new AcceptLanguageHeaderRequestCultureProvider()
};

app.UseRequestLocalization(localizationOptions);




app.UseRouting();


app.UseAuthentication(); // Ekledik
app.UseAuthorization();
app.UseMiddleware<ForcePasswordChangeMiddleware>();

/*
// Localization from cookie
app.Use(async (context, next) =>
{
    if (context.Request.Cookies.TryGetValue("Language", out var cookie))
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(cookie);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(cookie);
    }
    else
    {
        var defaultCulture = new CultureInfo("tr");
        Thread.CurrentThread.CurrentCulture = defaultCulture;
        Thread.CurrentThread.CurrentUICulture = defaultCulture;
    }
    await next.Invoke();
});
*/


// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
