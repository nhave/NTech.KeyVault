using NTech.KeyVault.ClientServices.Abstractions;
using System.Security.Claims;

namespace NTech.KeyVault.Frontend.Services
{
    public class BlazorTokenContext : ITokenContext
    {
        private readonly IHttpContextAccessor _http;

        public BlazorTokenContext(IHttpContextAccessor http)
        {
            _http = http;
        }

        public bool IsAuthenticated()
            => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public string? GetEntityId()
            => _http.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
    }
}
