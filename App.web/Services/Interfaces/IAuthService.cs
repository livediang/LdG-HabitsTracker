using System.Threading.Tasks;
using App.web.Models;

namespace App.web.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthResult> RegisterAsync(string fullName, string email, string password);
        public Task<AuthResult> LoginAsync(string email, string password);
        public Task UpdatePasswordAsync(User user, string newPassword);
        public string HashPassword(string password);
    }
}
