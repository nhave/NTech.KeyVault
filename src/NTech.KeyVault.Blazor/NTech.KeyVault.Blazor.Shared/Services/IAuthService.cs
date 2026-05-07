using NTech.KeyVault.Blazor.Shared.Models;

namespace NTech.KeyVault.Blazor.Shared.Services
{
    public interface IAuthService
    {
        List<ServerInfo> LoadServers();
        ServerInfo AddServer(string name, string host);
        Task RemoveServer(string id);

        Task<bool> HasTokensAsync(string serverId);
        Task SaveTokensAsync(string serverId, string jwt, string refresh, DateTime expiry);
        Task<(string Jwt, string Refresh, DateTime Expiry)> GetTokensAsync(string serverId);
    }
}
