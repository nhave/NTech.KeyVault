namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record LoginResponse(string JwtToken = "", int ExpiresIn = 0, string RefreshToken = "", bool IsMfaNeeded = false, MfaInfo? MfaInfo = null);
    public record RefreshTokenResponse(string JwtToken, int ExpiresIn, string RefreshToken);
    public record PasswordChangeResponse(string Message);
    public record MeResponse(string Id, string FullName, string Email, List<String> Roles);
}
