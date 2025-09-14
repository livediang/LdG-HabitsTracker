using Microsoft.AspNetCore.DataProtection;
using App.web.Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace App.web.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateEmailConfirmationToken(Guid userId, string email)
        {
            var payload = new { userId, email, type = "confirm", exp = DateTime.UtcNow.AddHours(1) };
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        }

        public string GeneratePasswordResetToken(Guid userId, string email)
        {
            var payload = new { userId, email, type = "reset", exp = DateTime.UtcNow.AddHours(1) };
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        }

        public TokenResult ValidateEmailConfirmationToken(string token)
        {
            try
            {
                var decoded = JsonSerializer.Deserialize<TokenPayload>(Decode(token));
                if (decoded == null || decoded.type != "confirm" || decoded.exp < DateTime.UtcNow)
                    return new TokenResult(false, Guid.Empty, "");
                return new TokenResult(true, decoded.userId, decoded.email);
            }
            catch
            {
                return new TokenResult(false, Guid.Empty, "");
            }
        }

        public TokenResult ValidatePasswordResetToken(string token)
        {
            try
            {
                var decoded = JsonSerializer.Deserialize<TokenPayload>(Decode(token));
                if (decoded == null || decoded.type != "reset" || decoded.exp < DateTime.UtcNow)
                    return new TokenResult(false, Guid.Empty, "");
                return new TokenResult(true, decoded.userId, decoded.email);
            }
            catch
            {
                return new TokenResult(false, Guid.Empty, "");
            }
        }

        private string Decode(string token)
        {
            var bytes = Convert.FromBase64String(token);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private class TokenPayload
        {
            public Guid userId { get; set; }
            public string email { get; set; }
            public string type { get; set; }
            public DateTime exp { get; set; }
        }
    }
}
