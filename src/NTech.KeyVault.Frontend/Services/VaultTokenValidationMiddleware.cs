using Microsoft.AspNetCore.Authentication;
using NTech.KeyVault.Frontend.Models;
using System.Security.Claims;

namespace NTech.KeyVault.Frontend.Services
{
    public sealed class VaultTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public VaultTokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenStore tokenStore)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var entityId = context.User.FindFirst(ClaimTypes.Sid)?.Value;

            if (entityId is null)
            {
                await SignOut(context);
                return;
            }

            var model = await tokenStore.GetAsync<AuthTokenModel>(entityId);

            if (model == null || string.IsNullOrWhiteSpace(model.JwtToken))
            {
                await SignOut(context);
                return;
            }

            await _next(context);
        }

        private static async Task SignOut(HttpContext context)
        {
            await context.SignOutAsync();
            context.Response.Redirect("/login");
        }
    }
}
