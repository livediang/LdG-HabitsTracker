namespace App.web.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateEmailConfirmationToken(Guid userId, string email);
        string GeneratePasswordResetToken(Guid userId, string email);

        TokenResult ValidateEmailConfirmationToken(string token);
        TokenResult ValidatePasswordResetToken(string token);
    }

    public record TokenResult(bool IsValid, Guid UserId, string Email);
}
