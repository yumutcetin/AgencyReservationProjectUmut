// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using RabbitMQ.Client;
using RezerVanaUmv.Controllers;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
       //private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            //_emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Güvenlik nedeniyle kullanıcı var mı yok mu belli etmiyoruz
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Şifre sıfırlama token'ı oluştur
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // RabbitMQ ile kuyruğa e-posta mesajı gönder
                var factory = new ConnectionFactory()
                {
                    HostName = "157.90.161.220",
                    Port = 5672,
                    UserName = "guest",
                    Password = "adminumv"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "rezervana_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var emailMQ = new EmailMQ
                {
                    email = Input.Email,
                    mailsubject = "Reset Password",
                    mailbody = $@"Hello,<br><br>
                    To reset your password, please click the link below:<br><br>
                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Reset Password</a><br><br>
                    If you did not request this, you can safely ignore this email.",
                                        cc = null
                };

                var json = System.Text.Json.JsonSerializer.Serialize(emailMQ);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: "",
                                     routingKey: "rezervana_queue",
                                     basicProperties: null,
                                     body: body);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
public class EmailMQ
{
    public string email { get; set; }
    public string mailsubject { get; set; }
    public string mailbody { get; set; }
    public string cc { get; set; }
}
