using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace App.web.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var smtpUser = _configuration["SMTP:Email"];
            var smtpPass = _configuration["SMTP:Password"];
            
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage(smtpUser, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
