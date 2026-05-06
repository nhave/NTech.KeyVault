using NTech.KeyVault.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTech.KeyVault.Common.Models.Database
{
    public class User : Common
    {
        // Login

        /// <summary>
        /// The username of the user. This is unique across the system and is used for login purposes. It should be treated as case-insensitive.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Gets or sets the hashed representation of the user's password.
        /// </summary>
        public required string PasswordHash { get; set; }

        // Lookup (deterministic hashes)

        /// <summary>
        /// Gets or sets the deterministic hash value used for email address lookups.
        /// </summary>
        /// <remarks>This property stores a hash derived from an email address to enable efficient and
        /// privacy-preserving searches. The hash must be generated using a consistent algorithm to ensure lookups are
        /// reliable across different instances.</remarks>
        public required string EmailLookupHash { get; set; }

        // EncryptedUserInfo

        /// <summary>
        /// Gets or sets the user information in encrypted form.
        /// </summary>
        public required byte[] EncryptedUserInfo { get; set; }

        /// <summary>
        /// Gets or sets the nonce value associated with the user information for this entity.
        /// </summary>
        /// <remarks>The nonce is intended to be unique per entity and may be used to prevent replay
        /// attacks or to correlate user information securely. The length and format of the nonce should be consistent
        /// with the requirements of the consuming system.</remarks>
        public required byte[] UserInfoNonce { get; set; }

        /// <summary>
        /// Gets or sets the encrypted form of the data encryption key.
        /// </summary>
        public required byte[] EncryptedDataKey { get; set; }

        /// <summary>
        /// Gets or sets the nonce value associated with the data encryption key for this entity.
        /// </summary>
        /// <remarks>The nonce must be unique per entity to ensure cryptographic security when encrypting
        /// or decrypting data. Changing this value may affect the ability to decrypt previously encrypted
        /// data.</remarks>
        public required byte[] DataKeyNonce { get; set; }

        // Audit

        /// <summary>
        /// Gets or sets the date and time when the user last logged in.
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// Gets or sets the last known IP address associated with the entity.
        /// </summary>
        public string LastIp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the HTTP User-Agent header sent with requests.
        /// </summary>
        /// <remarks>Set this property to identify the client application or library when making HTTP
        /// requests. Some servers may use the User-Agent value for analytics, compatibility, or access control
        /// purposes.</remarks>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the component is disabled.
        /// </summary>
        public bool IsDisabled { get; set; } = false;

        // Relations

        /// <summary>
        /// Gets or sets the collection of roles associated with the user.
        /// </summary>
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Gets or sets the collection of multi-factor authentication (MFA) methods associated with the user.
        /// </summary>
        public List<UserMfaMethod> MfaMethods { get; set; } = new List<UserMfaMethod>();

        /// <summary>
        /// Gets the list of roles assigned to the user, including any expanded or inherited roles.
        /// </summary>
        /// <remarks>The returned list includes all roles directly assigned to the user as well as any
        /// roles that are expanded through role inheritance or grouping. The list is read-only and reflects the current
        /// state of the user's role assignments.</remarks>
        public List<Roles> Roles => UserRoles.Select(ur => ur.Role).ExpandRoles();
    }
}
