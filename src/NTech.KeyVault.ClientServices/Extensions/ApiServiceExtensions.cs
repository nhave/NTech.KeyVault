using NTech.KeyVault.ClientServices.MessageHendlers;
using NTech.KeyVault.ClientServices.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        string baseUrl)
        {
            // Register the TokenAuthorizationHandler as a transient service
            services.AddTransient<TokenAuthorizationHandler>();

            // Register the HttpClient for token refresh without the handler to avoid circular dependencies
            services.AddHttpClient("Auth", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });

            // Register the LoginService
            services.AddScoped<LoginService>();

            // Register the ApiService with the TokenAuthorizationHandler
            services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler<TokenAuthorizationHandler>();

            return services;
        }
    }
}
