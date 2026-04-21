namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record EnableMfaResponse(string MfaSecretKey);
    public record VerifyTotpResponse(List<string> MfaBackupCodes);
}
