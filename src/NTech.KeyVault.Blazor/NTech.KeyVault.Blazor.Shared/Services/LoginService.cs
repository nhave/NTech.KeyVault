using NTech.KeyVault.Blazor.Shared.Models;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Net.Http.Json;

namespace NTech.KeyVault.Blazor.Shared.Services
{
    public class LoginService(ApiClientFactory clientFactory, IAuthService authService)
    {
        public async Task<LoginResponse> LoginAsync(
        ServerInfo serverInfo,
        LoginRequest request)
        {
            var client = clientFactory.Create(serverInfo.Host);

            var response = await client.PostAsJsonAsync("/auth/login", request);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Login failed: {response.StatusCode}");

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (login is null)
                throw new InvalidOperationException("Invalid login response");

            // MFA required → return response to UI
            if (login.IsMfaNeeded)
                return login;

            // Normal login → store token
            await authService.SaveTokensAsync(serverInfo.Id, login.JwtToken, login.RefreshToken, DateTime.UtcNow.AddSeconds(login.ExpiresIn));

            return login;
        }
    }
}
