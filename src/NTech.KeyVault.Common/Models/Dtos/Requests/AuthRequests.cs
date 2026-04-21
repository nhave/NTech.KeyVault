namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public record LoginRequest(string Username, string Password);
    public record RegisterRequest(string Name, string Username, string Password);
    public record RefreshTokenRequest(string RefreshToken);
    public record PasswordChangeRequest(string CurrentPassword, string NewPassword);
    public record EmailChangeRequest(string CurrentPassword, string NewEmail);
}
