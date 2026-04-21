using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public record EnableMfaRequest(MfaMethodType MethodType);
    public record DisableMfaRequest(MfaMethodType MethodType, string Code);
    public record VerifyTotpRequest(string Code);
}
