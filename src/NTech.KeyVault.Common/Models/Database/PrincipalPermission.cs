using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Database
{
    public class PrincipalPermission : Common
    {
        public PrincipalType PrincipalType { get; set; }
        public Guid PrincipalId { get; set; }

        public ResourceType ResourceType { get; set; }
        public Guid ResourceId { get; set; }

        public List<Permission> Permissions { get; set; } = new();
    }
}
