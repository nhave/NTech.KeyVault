using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Database
{
    public class UserMfaMethod : Common
    {
        public required Guid UserId { get; set; }

        public MfaMethodType Method { get; set; }
        public bool IsEnabled { get; set; } = false;

        // Encrypted TOTP-secret
        public required byte[] EncryptedSecret { get; set; }
        public required byte[] SecretNonce { get; set; }
        public required byte[] EncryptedDataKey { get; set; }
        public required byte[] DataKeyNonce { get; set; }

        // Backup codes (hashes)
        public List<string> BackupCodeHashes { get; set; } = new();

        // Audit
        public DateTime? LastUsedAt { get; set; }

        // Navigation property
        public required User User { get; set; }
    }
}
