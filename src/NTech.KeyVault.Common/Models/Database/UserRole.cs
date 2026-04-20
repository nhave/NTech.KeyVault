using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Database
{
    public class UserRole : Common
    {
        /// <summary>
        /// Foreign key to the User table, indicating which user this role is associated with.
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the role assigned to the user.
        /// </summary>
        public Roles Role { get; set; }

        // Relations

        /// <summary>
        /// Navigation property to the User entity, allowing access to the user's details from this role assignment.
        /// </summary>
        public required User User { get; set; }
    }
}
