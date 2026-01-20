using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.Localization;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;
using System.Globalization;

namespace RezerVanaUmv.Controllers
{

    public class ExtraFunctionsController : Controller
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly RoleManager<AppUserRoles> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ExtraFunctionsController(RzvnUmvUmvKrmnBlzrContext context, UserManager<ApplicationUser>
            userManager, IWebHostEnvironment webHostEnvironment, RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
        }

       

		public static int LevenshteinDistance(string source, string target)
		{
			if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
			if (string.IsNullOrEmpty(target)) return source.Length;

			int sourceLength = source.Length;
			int targetLength = target.Length;
			var distance = new int[sourceLength + 1, targetLength + 1];

			for (int i = 0; i <= sourceLength; distance[i, 0] = i++) { }
			for (int j = 0; j <= targetLength; distance[0, j] = j++) { }

			for (int i = 1; i <= sourceLength; i++)
			{
				for (int j = 1; j <= targetLength; j++)
				{
					int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
					distance[i, j] = Math.Min(
						Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
						distance[i - 1, j - 1] + cost);
				}
			}
			return distance[sourceLength, targetLength];
		}

		

		[AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Kvkk()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> HowToUse()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> KullanimSartlari()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Hakkimizda()
        {
            return View();
        }



        [AllowAnonymous]
        public IActionResult ChangeLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang))
                lang = "tr"; // varsayılan

            // URL'den gelen kısa kodu gerçek culture adına çevir
            var cultureName = lang switch
            {
                "tr" or "tr-TR" => "tr-TR",
                "en" or "en-US" => "en-US",
                "ru" or "ru-RU" => "ru-RU",
                "de" or "de-DE" => "de-DE",

                // Kore
                "kr" or "ko" or "ko-KR" => "ko-KR",

                // Çince
                "zh" or "zh-CN" => "zh-CN",

                _ => "tr-TR"
            };

            var culture = new CultureInfo(cultureName);

            // ASP.NET Core localization cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTime.Now.AddYears(1), HttpOnly = false }
            );

            // 🔹 Seçilen dili ayrıca kendi cookie'mize yazalım (bayrak için)
            Response.Cookies.Append(
                "SelectedLang",
                lang,
                new CookieOptions { Expires = DateTime.Now.AddYears(1), HttpOnly = false }
            );

            // Resource class kültürü
            RezerVanaUmv.Resources.Resource.Culture = culture;

            return Redirect(Request.Headers["Referer"].ToString());
        }




    }
}
