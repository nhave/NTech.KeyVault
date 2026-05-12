using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using NTech.KeyVault.Frontend.Models;
using System.Security.Claims;

namespace NTech.KeyVault.Frontend.Services
{
    public class TokenAuthorizationHandler(IHttpContextAccessor contextAccessor, ITokenStore tokenStore, IHttpClientFactory clientFactory, IConfiguration configuration) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var ctx = contextAccessor.HttpContext;
            if (ctx?.User?.Identity?.IsAuthenticated != true) return await base.SendAsync(request, ct);

            var cookieId = ctx.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (cookieId == null) return await base.SendAsync(request, ct);
            var model = await tokenStore.GetAsync<AuthTokenModel>(cookieId);

            if (model == null || string.IsNullOrWhiteSpace(model.JwtToken)) return await base.SendAsync(request, ct);

            // If token is expired, try to refresh.
            if (model.ExpiresAt <= DateTime.UtcNow && !string.IsNullOrWhiteSpace(model.RefreshToken))
            {
                try
                {
                    var refreshClient = clientFactory.CreateClient();

                    var resp = await refreshClient.PostAsJsonAsync("/auth/refresh", new RefreshTokenRequest(model.RefreshToken), ct);
                    if (resp.IsSuccessStatusCode)
                    {
                        var payload = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
                        if (payload == null) return await base.SendAsync(request, ct);

                        model.JwtToken = payload.JwtToken;
                        var newExpires = DateTime.UtcNow.AddSeconds(payload.ExpiresIn);
                        var newRefresh = payload.RefreshToken;

                        await tokenStore.SetAsync(cookieId, new AuthTokenModel
                        {
                            JwtToken = model.JwtToken,
                            ExpiresAt = newExpires,
                            RefreshToken = newRefresh
                        });
                    }
                }
                catch
                {
                    return await base.SendAsync(request, ct);
                }
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", model.JwtToken);

            return await base.SendAsync(request, ct);
        }
    }
}
