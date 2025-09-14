using System.Threading.Tasks;

namespace App.web.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmEmailAsync(string email, string fullName, string confirmUrl);
        Task SendPasswordResetAsync(string email, string resetUrl);
    }
}
