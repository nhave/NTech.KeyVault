namespace NTech.KeyVault.Frontend
{
    public record LoginUiResponse(bool Success, bool IsMfaRequired, string ErrorMessage = "");
}
