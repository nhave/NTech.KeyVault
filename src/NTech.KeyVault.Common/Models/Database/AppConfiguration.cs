namespace NTech.KeyVault.Common.Models.Database
{
    public class AppConfiguration : Common
    {
        // Relationships
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; } = default!;

        // Encryption
        public required byte[] EncryptedData { get; set; }
        public required byte[] DataNonce { get; set; }
        public required byte[] EncryptedDataKey { get; set; }
        public required byte[] DataKeyNonce { get; set; }
    }
}
