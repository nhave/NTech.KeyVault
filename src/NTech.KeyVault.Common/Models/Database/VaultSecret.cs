namespace NTech.KeyVault.Common.Models.Database
{
    public class VaultSecret : Common
    {
        // Encryption
        public byte[] EncryptedValue { get; set; } = default!;
        public byte[] ValueNonce { get; set; } = default!;
        public byte[] EncryptedDataKey { get; set; } = default!;
        public byte[] DataKeyNonce { get; set; } = default!;

        // Relationships
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; } = default!;

        // For secrets that are stored in the vault but not created by the vault, we can use these properties to track them.
        public string EntityId { get; set; } = default!;
        public DateTime ExpirationDate { get; set; }
    }
}
