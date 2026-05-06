namespace NTech.KeyVault.Blazor.Shared.Models
{
    public class ServerInfo
    {
        public string Id { get; set; } = string.Empty;     // Unik ID
        public string Name { get; set; } = string.Empty;   // Visningsnavn
        public string Host { get; set; } = string.Empty;   // URL eller hostname
    }
}
