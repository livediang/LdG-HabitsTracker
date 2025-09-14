using App.web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using RazorLight;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace App.web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly RazorLightEngine _razor;

        public EmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _razor = new RazorLightEngineBuilder()
                .UseFileSystemProject(Path.Combine(env.ContentRootPath, "Views/Shared/Emails"))
                .UseMemoryCachingProvider()
                .Build();
        }

        private async Task SendAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["SMTP:Host"];
            var smtpPort = int.Parse("587");
            var smtpUser = _configuration["SMTP:Email"];
            var smtpPass = _configuration["SMTP:Password"];
            var fromName = "Habits Tracker";

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage()
            {
                From = new MailAddress(smtpUser, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            await smtp.SendMailAsync(mail);
        }

        public async Task SendConfirmEmailAsync(string email, string fullName, string confirmUrl)
        {
            var subject = _configuration["SMTP:Subjects:ConfirmEmail"] ?? "Confirm your account";

            var body = await _razor.CompileRenderAsync(
                "ConfirmEmailTemplate.cshtml",
                new { FullName = fullName, ConfirmUrl = confirmUrl }
            );

            await SendAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string resetUrl)
        {
            var subject = _configuration["SMTP:Subjects:ResetPassword"] ?? "Reset your password";

            var body = await _razor.CompileRenderAsync(
                "ResetPasswordTemplate.cshtml",
                new { ResetUrl = resetUrl }
            );

            await SendAsync(email, subject, body);
        }
    }
}
