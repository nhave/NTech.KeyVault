namespace NTech.KeyVault.ClientServices.Models
{
    public class AuthTokenModel
    {
        public string JwtToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
