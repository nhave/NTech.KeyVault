using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Blazor.Shared.Services;

namespace NTech.KeyVault.Blazor.Maui.Services
{
    public class ActiveServerService(IAuthService authService, MauiAuthStateProvider authState) : IActiveServerService
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

        public async Task SetActiveServer(ServerInfo server)
        {
            Preferences.Set(ActiveServerKey, server.Id);

            // Hent token for denne server
            var (jwt, _, _) = await authService.GetTokensAsync(server.Id);

            // Opdater Blazor AuthenticationState
            await authState.SetToken(jwt);
        }

        public async Task ClearActiveServer()
        {
            Preferences.Remove(ActiveServerKey);

            // Log brugeren ud i Blazor
            await authState.SetToken(null);
        }
    }
}
