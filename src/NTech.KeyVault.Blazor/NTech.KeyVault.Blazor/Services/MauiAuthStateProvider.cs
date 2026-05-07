using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NTech.KeyVault.Blazor.Maui.Services
{
    public sealed class MauiAuthStateProvider : AuthenticationStateProvider
    {
        private readonly JwtSecurityTokenHandler _handler = new();
        private ClaimsPrincipal _current = new(new ClaimsIdentity());

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var activeServerId = Preferences.Get("activeServerId", null);

            if (string.IsNullOrWhiteSpace(activeServerId))
                return new AuthenticationState(_current);

            var jwt = await SecureStorage.GetAsync($"jwt_{activeServerId}");

            if (string.IsNullOrWhiteSpace(jwt))
                return new AuthenticationState(_current);

            var identity = CreateIdentityFromJwt(jwt);
            _current = new ClaimsPrincipal(identity);

            return new AuthenticationState(_current);
        }

        public async Task SetToken(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
            {
                _current = new ClaimsPrincipal(new ClaimsIdentity());
            }
            else
            {
                var identity = CreateIdentityFromJwt(jwt);
                _current = new ClaimsPrincipal(identity);
            }

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private ClaimsIdentity CreateIdentityFromJwt(string jwt)
        {
            var token = _handler.ReadJwtToken(jwt);
            return new ClaimsIdentity(token.Claims, "jwt");
        }
    }
}
