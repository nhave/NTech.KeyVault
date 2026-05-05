using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record EnableMfaResponse(string MfaSecretKey);
    public record VerifyTotpResponse(List<string> MfaBackupCodes);
    public record GetMfaStatusResponse(bool IsMfaEnabled, MfaInfo? MfaInfo = null);

    public record MfaInfo(List<MfaMethodType> EnabledMfaMethods, MfaMethodType DefaultMfaMethod);
}
