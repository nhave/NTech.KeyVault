namespace NTech.KeyVault.Common.Models.Database
{
    public class VaultSecret : Common
    {
        public string Name { get; set; }

        // Encryption
        public byte[] EncryptedData { get; set; }
        public byte[] DataNonce { get; set; }
        public byte[] EncryptedDataKey { get; set; }
        public byte[] DataKeyNonce { get; set; }

        // Relationships
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; } = default!;
    }
}
