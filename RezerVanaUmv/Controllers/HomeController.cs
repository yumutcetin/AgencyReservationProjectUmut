using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RezerVanaUmv.Models;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;


namespace RezerVanaUmv.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private const string RecaptchaSecretKey = "6Lc88nUrAAAAAKqZufkInMfGz79rbK3ac04g5KeZ";
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<AppUserRoles> _roleManager;
    private readonly RzvnUmvUmvKrmnBlzrContext _context;
    public HomeController(RzvnUmvUmvKrmnBlzrContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager,
        RoleManager<AppUserRoles> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
            var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var type = userClaims.FirstOrDefault(c => c.Type == "UserType");
            if (type !=null && type.Value == "AGENCY")
            {
                var agencyId = userClaims.FirstOrDefault(c => c.Type == "AgencyId");
                if(agencyId == null)
                {
                    return RedirectToAction("Create", "Agencies");
                }
                return View();
            }
        }
            
        

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    [HttpGet]
    public IActionResult Iletisim()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Send(ContactFormModel model)
    {
        if (string.IsNullOrEmpty(model.RecaptchaResponse))
        {
            ViewBag.Message = "Lütfen reCAPTCHA doðrulamasýný tamamlayýn.";
            return View("Iletisim");
        }

        if (!await ValidateRecaptcha(model.RecaptchaResponse))
        {
            ViewBag.Message = "reCAPTCHA doðrulamasý baþarýsýz oldu. Lütfen tekrar deneyin.";
            return View("Iletisim");
        }

        if (ModelState.IsValid)
        {
            try
            {
                SendEmailToQueue(model);
                ViewBag.Message = "Mesajýnýz gönderildi!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Mesaj gönderilirken bir hata oluþtu: " + ex.Message;
            }
        }
        else
        {
            ViewBag.Message = "Lütfen tüm alanlarý doldurun.";
        }

        return View("Iletisim");
    }

    private async Task<bool> ValidateRecaptcha(string recaptchaResponse)
    {
        using (var client = new HttpClient())
        {
            var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={RecaptchaSecretKey}&response={recaptchaResponse}", null);
            var jsonString = await response.Content.ReadAsStringAsync();
            dynamic jsonData = JsonSerializer.Deserialize<dynamic>(jsonString);

            return jsonData != null && jsonData.GetProperty("success").GetBoolean();
        }
    }


    private void SendEmailToQueue(ContactFormModel model)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "157.90.161.220",
            Port = 5672,
            UserName = "guest",
            Password = "adminumv"
        };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Kuyruk tanýmlama
            channel.QueueDeclare(queue: "email_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // E-posta bilgilerini JSON'a çevir
            var emailMQ = new EmailMQ
            {
                email = "info@bagsfollow.com",
                mailsubject = model.Subject,
                mailbody = $"{model.Email} - {model.Message}",
                cc = "umvkurumsal@bagsfollow.com"
            };

            var json = JsonSerializer.Serialize(emailMQ);
            var body = Encoding.UTF8.GetBytes(json);

            // Kuyruða mesajý gönder
            channel.BasicPublish(exchange: "",
                                 routingKey: "email_queue",
                                 basicProperties: null,
                                 body: body);
        }
    }
}

// ContactFormModel class for form validation
public class ContactFormModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public string RecaptchaResponse { get; set; }
}

// EmailMQ class for RabbitMQ message structure
public class EmailMQ
{
    public string email { get; set; }
    public string mailsubject { get; set; }
    public string mailbody { get; set; }
    public string cc { get; set; }
}



