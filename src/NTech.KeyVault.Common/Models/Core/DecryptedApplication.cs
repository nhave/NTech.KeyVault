using System.Text;
using System.Text.Json;

namespace NTech.KeyVault.Common.Models.Core
{
    public class DecryptedApplication
    {
        public Guid Id { get; set; }
        public required string AppSecret { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid? OwnerUserId { get; set; }
    }
}
