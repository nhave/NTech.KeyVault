using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Blazor.Shared.Services;
using System.Text.Json;

namespace NTech.KeyVault.Blazor.Maui.Services
{
    public class AuthService(MauiAuthStateProvider authState) : IAuthService
    {
        private const string ServerListKey = "serverList";

        public List<ServerInfo> LoadServers()
        {
            var json = Preferences.Get(ServerListKey, "[]");
            return JsonSerializer.Deserialize<List<ServerInfo>>(json) ?? new();
        }

        public ServerInfo AddServer(string name, string host)
        {
            var list = LoadServers();

            var server = new ServerInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Host = host
            };

            list.Add(server);
            SaveServers(list);

            return server;
        }

        private void SaveServers(List<ServerInfo> list)
        {
            var json = JsonSerializer.Serialize(list);
            Preferences.Set(ServerListKey, json);
        }

        public async Task RemoveServer(string id)
        {
            var list = LoadServers();
            var item = list.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                list.Remove(item);
                SaveServers(list);

                SecureStorage.Remove($"jwt_{id}");
                SecureStorage.Remove($"refresh_{id}");
                SecureStorage.Remove($"expiry_{id}");

                await authState.SetToken(null);
            }
        }

        public async Task SaveTokensAsync(string serverId, string jwt, string refresh, DateTime expiry)
        {
            await SecureStorage.SetAsync($"jwt_{serverId}", jwt);
            await SecureStorage.SetAsync($"refresh_{serverId}", refresh);
            await SecureStorage.SetAsync($"expiry_{serverId}", expiry.ToString("O"));

            await authState.SetToken(jwt);
        }

        public async Task<(string Jwt, string Refresh, DateTime Expiry)> GetTokensAsync(string serverId)
        {
            var jwt = await SecureStorage.GetAsync($"jwt_{serverId}");
            var refresh = await SecureStorage.GetAsync($"refresh_{serverId}");
            var expiryString = await SecureStorage.GetAsync($"expiry_{serverId}");

            DateTime.TryParse(expiryString, out var expiry);

            return (jwt!, refresh!, expiry);
        }

        public async Task<bool> HasTokensAsync(string serverId)
        {
            var jwt = await SecureStorage.GetAsync($"jwt_{serverId}");
            var refresh = await SecureStorage.GetAsync($"refresh_{serverId}");
            var expiryString = await SecureStorage.GetAsync($"expiry_{serverId}");

            if (string.IsNullOrWhiteSpace(jwt) ||
                string.IsNullOrWhiteSpace(refresh) ||
                string.IsNullOrWhiteSpace(expiryString))
            {
                return false;
            }

            if (!DateTime.TryParse(expiryString, out var expiry))
                return false;

            return expiry > DateTime.UtcNow;
        }
    }
}
