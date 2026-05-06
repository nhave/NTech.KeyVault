using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Blazor.Shared.Services;

namespace NTech.KeyVault.Blazor.Web.Services
{
    public class ActiveServerService(IAuthService authService) : IActiveServerService
    {
        public ServerInfo? GetActiveServer()
        {
            var servers = authService.LoadServers();
            return servers[0];
        }

        public void SetActiveServer(ServerInfo server) { }

        public void ClearActiveServer() { }
    }
}
