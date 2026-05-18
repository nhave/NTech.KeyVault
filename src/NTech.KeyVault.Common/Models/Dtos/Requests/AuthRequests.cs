using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public class LoginRequest
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public MfaMethodType? MfaMethod { get; set; }
        public string? MfaCode { get; set; }
    }


    public record RegisterRequest(string Name, string Username, string Password);
    public record RefreshTokenRequest(string RefreshToken);
    public record PasswordChangeRequest(string CurrentPassword, string NewPassword);
    public record EmailChangeRequest(string CurrentPassword, string NewEmail);
}
