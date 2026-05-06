namespace NTech.KeyVault.Common.Models.Database
{
    public class Application : Common
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string AppSecretHash { get; set; }

        // The following properties are designed to securely store sensitive information related to the application,
        // such as secrets and encryption keys, ensuring that they are protected and can only be accessed by authorized entities.
        public required byte[] EncryptedSecret { get; set; }
        public required byte[] SecretNonce { get; set; }
        public required byte[] EncryptedDataKey { get; set; }
        public required byte[] DataKeyNonce { get; set; }

        // User association for ownership and management of the application.
        // This allows for tracking which user created or owns the application,
        // enabling features like user-specific application management and permissions.
        public Guid? OwnerUserId { get; set; }
        public User? OwnerUser { get; set; }
    }
}
