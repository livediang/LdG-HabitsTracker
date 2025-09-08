using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace App.web.Services
{
    public class TokenService
    {
        private readonly IDataProtector _protector;

        public TokenService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("EmailConfirmation");
        }

        public string GenerateEmailConfirmationToken(Guid userId, string email)
        {
            var data = $"{userId}|{email}|{DateTime.UtcNow}";
            return _protector.Protect(data);
        }

        public (Guid userId, string email, DateTime created) ValidateToken(string token)
        {
            var unprotected = _protector.Unprotect(token);
            var parts = unprotected.Split('|');

            return (Guid.Parse(parts[0]), parts[1], DateTime.Parse(parts[2]));
        }
    }
}
