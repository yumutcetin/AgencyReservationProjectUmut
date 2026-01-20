#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Data;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly RzvnUmvUmvKrmnBlzrContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<AppUserRoles> _roleManager;
        private readonly HttpClient _httpClient;

        public RegisterModel(
            RzvnUmvUmvKrmnBlzrContext context,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<AppUserRoles> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _httpClient = new HttpClient();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az {2}, en fazla {1} karakter olmalıdır.")]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
                ErrorMessage = "Şifre en az 1 büyük harf, 1 küçük harf, 1 rakam ve 1 özel karakter içermelidir.")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
            public string ConfirmPassword { get; set; }

            public string InviteCode { get; set; }

            [Required]
            [Display(Name = "Hesap Türü")]
            public string? AccountType { get; set; }

            public string? CompanyName { get; set; }

        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");


            var recaptchaToken = Request.Form["g-recaptcha-response"];
            if (!await ValidateReCaptcha(recaptchaToken))
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA doğrulaması başarısız oldu.");
                return Page();
            }


            if (!ModelState.IsValid)
                return Page();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    ModelState.AddModelError(string.Empty, "Bu e-posta adresi zaten kayıtlı.");
                    return Page();
                }
    
                    user = new ApplicationUser();
                    await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                    var createResult = await _userManager.CreateAsync(user, Input.Password);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return Page();
                    }

                    user.EmailConfirmed = true;
                    user.LastPasswordChangeDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Email}", Input.Email);

                    if (Input.AccountType == "KURUMSAL")
                    {
                        var claims = await _userManager.GetClaimsAsync(user);
                        await _userManager.AddClaimAsync(user, new Claim("UserType", "AGENCY"));
                        await _userManager.AddToRoleAsync(user, "AGENCY");
                        await transaction.CommitAsync();
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction("Create","Agencies");
                    }
                    else
                    {
                        await _userManager.AddClaimAsync(user, new Claim("UserType", "PASSENGER"));
                        await _userManager.AddToRoleAsync(user, "PASSENGER");
                    }

                    await transaction.CommitAsync();
                    await _signInManager.SignInAsync(user, false);
                    return LocalRedirect(ReturnUrl);
   

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Kayıt sırasında hata.");
                ModelState.AddModelError(string.Empty, "Kayıt işlemi sırasında bir hata oluştu.");
                return Page();
            }
        }


        private async Task<bool> ValidateReCaptcha(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var secret = "6Lc88nUrAAAAAKqZufkInMfGz79rbK3ac04g5KeZ";
                var response = await _httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                    null);

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReCaptchaResponse>(json);
                return result.success;
            }
            catch
            {
                return false;
            }
        }

        private class ReCaptchaResponse
        {
            public bool success { get; set; }
            public DateTime challenge_ts { get; set; }
            public string hostname { get; set; }
            public List<string> error_codes { get; set; }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("Email desteklenmiyor.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
