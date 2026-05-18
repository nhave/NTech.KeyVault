using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Frontend
{
    public record LoginUiResponse(bool Success, bool IsMfaRequired, MfaInfo? MfaInfo = null, string ErrorMessage = "");
}
