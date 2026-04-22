using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Api.Services;

namespace NTech.KeyVault.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApiRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IMfaRepository, MfaRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
        }

        public static void AddApiServices(this IServiceCollection services)
        {
            services.AddHostedService<MigrationService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMfaService, MfaService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IApplicationService, ApplicationService>();
        }
    }
}
