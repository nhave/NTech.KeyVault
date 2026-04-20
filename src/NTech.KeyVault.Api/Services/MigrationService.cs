using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;

namespace NTech.KeyVault.Api.Services
{
    public class MigrationService(IServiceProvider services, ILogger<MigrationService> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            const int maxRetries = 10;
            var delay = TimeSpan.FromSeconds(5);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    logger.LogInformation("Running migrations...");
                    await db.Database.MigrateAsync(cancellationToken);

                    logger.LogInformation("Running seed-data...");
                    await SeedData.InitializeAsync(db, scope, cancellationToken);

                    logger.LogInformation("Migrations og seed-data done.");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Migration/seed failed (attempt {Attempt}/{Max}).", attempt, maxRetries);

                    if (attempt == maxRetries)
                        throw;

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
