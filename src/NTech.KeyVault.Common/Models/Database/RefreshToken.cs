namespace NTech.KeyVault.Common.Models.Database
{
    public class RefreshToken : Common
    {
        // Relation
        public required Guid UserId { get; set; }
        public required User User { get; set; }

        // Hashed token (SHA-256 + pepper)
        public required string TokenHash { get; set; }

        // Token lifetime
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;

        // Audit metadata
        public required string CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }

        // Token rotation
        public string? ReplacedByTokenHash { get; set; }
        public RefreshToken? ReplacedByToken { get; set; }

        // Convenience property
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;
    }
}
