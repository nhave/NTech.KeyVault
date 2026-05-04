namespace NTech.KeyVault.Common.Models.Database
{
    public class AppSecret : Common
    {
        public string Name { get; set; } = default!;

        // Encryption
        public byte[] EncryptedData { get; set; } = default!;
        public byte[] DataNonce { get; set; } = default!;
        public byte[] EncryptedDataKey { get; set; } = default!;
        public byte[] DataKeyNonce { get; set; } = default!;

        // Relationships
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; } = default!;
    }
}
