using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Database
{
    public class UserMfaMethod : Common
    {
        public string UserId { get; set; }

        public MfaMethodType Method { get; set; }
        public bool IsEnabled { get; set; } = false;

        // Encrypted TOTP-secret
        public byte[] EncryptedSecret { get; set; }
        public byte[] SecretNonce { get; set; }
        public byte[] EncryptedDataKey { get; set; }
        public byte[] DataKeyNonce { get; set; }

        // Backup codes (hashes)
        public List<string> BackupCodeHashes { get; set; } = new();

        // Audit
        public DateTime? LastUsedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
