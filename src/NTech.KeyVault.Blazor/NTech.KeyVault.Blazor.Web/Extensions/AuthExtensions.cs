using Microsoft.AspNetCore.Authentication.Cookies;

namespace NTech.KeyVault.Blazor.Web.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            // In development, allow non-secure cookies for testing without HTTPS
            //CookieSecurePolicy securePolicy = configuration["ASPNETCORE_ENVIRONMENT"] == "Development" ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            CookieSecurePolicy securePolicy = CookieSecurePolicy.None;

            // Add authentication with cookie scheme
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
                    options.Cookie.Name = "NTech.Vault.Auth";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = securePolicy;
                    options.Cookie.HttpOnly = true;
                    options.LoginPath = "/login";
                    options.SlidingExpiration = true;
                });

            // Add authorization services
            services.AddAuthorization();

            return services;
        }
    }
}
