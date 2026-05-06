using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Blazor.Shared.Services;

namespace NTech.KeyVault.Blazor.Maui.Services
{
    public class ActiveServerService(IAuthService authService) : IActiveServerService
    {
        private const string ActiveServerKey = "activeServerId";

        public ServerInfo? GetActiveServer()
        {
            var id = Preferences.Get(ActiveServerKey, null);
            if (id == null)
                return null;

            var servers = authService.LoadServers();
            return servers.FirstOrDefault(s => s.Id == id);
        }

        public void SetActiveServer(ServerInfo server)
        {
            Preferences.Set(ActiveServerKey, server.Id);
        }

        public void ClearActiveServer()
        {
            Preferences.Remove(ActiveServerKey);
        }
    }
}
