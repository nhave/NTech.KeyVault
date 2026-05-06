using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Blazor.Shared.Models
{
    public class ClientUser
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<Roles> Roles { get; set; } = new List<Roles>();
    }
}
