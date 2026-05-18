using NTech.KeyVault.ClientServices.Abstractions;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using System.Text.Json;

namespace NTech.KeyVault.Frontend.Services
{
    public class TokenStore(HttpClient httpClient) : ITokenStore
    {
        //private readonly HttpClient _client = clientFactory.CreateClient("VaultApplication");

        public async Task<bool> SetAsync<T>(string entityId, T data)
        {
            var request = new SetVaultSecretRequest
            (
                JsonSerializer.Serialize(data),
                DateTime.UtcNow.AddDays(7)
            );
            var response = await httpClient.PostAsJsonAsync($"appio/vaultsecret/{entityId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<T?> GetAsync<T>(string entityId)
        {
            var response = await httpClient.GetAsync($"appio/vaultsecret/{entityId}");
            if (!response.IsSuccessStatusCode) return default;

            var content = await response.Content.ReadFromJsonAsync<T>() ?? default;
            return content;
        }

        public async Task ClearAsync(string entityId)
        {
            await httpClient.DeleteAsync($"appio/vaultsecret/{entityId}");
        }
    }
}
