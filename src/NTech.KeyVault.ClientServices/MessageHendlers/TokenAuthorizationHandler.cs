using Microsoft.Extensions.Configuration;
using NTech.KeyVault.ClientServices.Abstractions;
using NTech.KeyVault.ClientServices.Models;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NTech.KeyVault.ClientServices.MessageHendlers
{
    public sealed class TokenAuthorizationHandler : DelegatingHandler
    {
        private readonly ITokenContext _context;
        private readonly ITokenStore _tokenStore;
        private readonly IHttpClientFactory _clientFactory;

        public TokenAuthorizationHandler(
            ITokenContext context,
            ITokenStore tokenStore,
            IHttpClientFactory clientFactory)
        {
            _context = context;
            _tokenStore = tokenStore;
            _clientFactory = clientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            if (!_context.IsAuthenticated())
                return await base.SendAsync(request, ct);

            var entityId = _context.GetEntityId();
            if (entityId is null)
                return await base.SendAsync(request, ct);

            var model = await _tokenStore.GetAsync<AuthTokenModel>(entityId);
            if (model is null || string.IsNullOrWhiteSpace(model.JwtToken))
                return await base.SendAsync(request, ct);

            // Refresh expired token
            if (model.ExpiresAt <= DateTime.UtcNow &&
                !string.IsNullOrWhiteSpace(model.RefreshToken))
            {
                try
                {
                    var refreshClient = _clientFactory.CreateClient("Auth");

                    var resp = await refreshClient.PostAsJsonAsync(
                        "/auth/refresh",
                        new RefreshTokenRequest(model.RefreshToken),
                        ct);

                    if (resp.IsSuccessStatusCode)
                    {
                        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
                        if (payload != null)
                        {
                            model.JwtToken = payload.JwtToken;
                            model.ExpiresAt = DateTime.UtcNow.AddSeconds(payload.ExpiresIn);
                            model.RefreshToken = payload.RefreshToken;

                            await _tokenStore.SetAsync(entityId, model);
                        }
                    }
                }
                catch
                {
                    // ignore refresh errors
                }
            }

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", model.JwtToken);

            return await base.SendAsync(request, ct);
        }
    }

}
