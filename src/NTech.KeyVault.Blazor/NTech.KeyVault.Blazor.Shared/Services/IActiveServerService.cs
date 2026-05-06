using NTech.KeyVault.Blazor.Shared.Models;

namespace NTech.KeyVault.Blazor.Shared.Services
{
    public interface IActiveServerService
    {
        public ServerInfo? GetActiveServer();
        public void SetActiveServer(ServerInfo server);
        public void ClearActiveServer();
    }
}
