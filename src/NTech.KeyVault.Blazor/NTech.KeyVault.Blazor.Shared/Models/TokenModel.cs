namespace NTech.KeyVault.Blazor.Shared.Models
{
    public class TokenModel
    {
        public string Jwt { get; set; } = string.Empty;
        public string Refresh { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}
