using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Blazor.Shared.Services;
using System.Text.Json;

namespace NTech.KeyVault.Blazor.Web.Services
{
    public class AuthService(ITokenStore tokenStore, IUserContext userContext, IConfiguration configuration) : IAuthService
    {
        private readonly ServerInfo _server = configuration.GetSection("Api").Get<ServerInfo>() ?? new ServerInfo();

        public List<ServerInfo> LoadServers()
        => new() { _server };

        public ServerInfo AddServer(string name, string host)
            => _server; // Ignored in the Blazor Server

        public void RemoveServer(string id)
        {
            // Ignored – Blazor Server has only one server
        }

        // --- Token-håndtering ---
        public async Task<bool> HasTokensAsync(string serverId)
        {
            var json = await tokenStore.GetAsync(userContext.EntityId);
            if (json == null)
                return false;

            var wrapper = JsonSerializer.Deserialize<TokenWrapper>(json);
            if (wrapper?.Tokens == null)
                return false;

            return wrapper.Tokens.Expiry > DateTime.UtcNow;
        }

        public async Task SaveTokensAsync(string serverId, string jwt, string refresh, DateTime expiry)
        {
            var wrapper = new TokenWrapper
            {
                EntityId = userContext.EntityId,
                Tokens = new TokenModel
                {
                    Jwt = jwt,
                    Refresh = refresh,
                    Expiry = expiry
                }
            };

            var json = JsonSerializer.Serialize(wrapper);
            await tokenStore.SaveAsync(userContext.EntityId, json);
        }

        public async Task<(string Jwt, string Refresh, DateTime Expiry)> GetTokensAsync(string serverId)
        {
            var json = await tokenStore.GetAsync(userContext.EntityId);
            if (json == null)
                return (string.Empty, string.Empty, DateTime.MinValue);

            var wrapper = JsonSerializer.Deserialize<TokenWrapper>(json);

            // Safety check: Ensure the wrapper and tokens are not null before accessing properties
            if (wrapper == null || wrapper.Tokens == null)
                return (string.Empty, string.Empty, DateTime.MinValue);

            var t = wrapper.Tokens;

            return (t.Jwt, t.Refresh, t.Expiry);
        }

        private class TokenWrapper
        {
            public string EntityId { get; set; } = string.Empty;
            public TokenModel Tokens { get; set; } = new TokenModel();
        }
    }
}
