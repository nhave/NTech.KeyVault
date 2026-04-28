using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IAppConfigurationRepository
    {
        public Task AddOrUpdate(AppConfiguration appConfig);
        public Task<AppConfiguration?> GetLatestByAppIdAsync(Guid appId);
        public Task<List<int>> GetAllVersionsByAppIdAsync(Guid appId);
        public Task<AppConfiguration?> GetByAppIdAndVersionAsync(Guid appId, int version);
        public Task DeleteAsync(AppConfiguration appConfig);
        public Task DeleteByAppIdAsync(Guid appId);
        public Task CleanupOldConfigurations(Guid appId);
    }

    public class AppConfigurationRepository(AppDbContext dbContext) : IAppConfigurationRepository
    {
        public async Task AddOrUpdate(AppConfiguration appConfig)
        {
            var existingConfig = await GetLatestByAppIdAsync(appConfig.ApplicationId);
            if (existingConfig != null)
                appConfig.Version = existingConfig.Version + 1;

            await dbContext.AppConfigurations.AddAsync(appConfig);
            await dbContext.SaveChangesAsync();
        }

        public async Task<AppConfiguration?> GetLatestByAppIdAsync(Guid appId)
        {
            return await dbContext.AppConfigurations.Include(ac => ac.CreatedBy).Where(ac => ac.ApplicationId == appId).OrderByDescending(ac => ac.Version).FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetAllVersionsByAppIdAsync(Guid appId)
        {
            return await dbContext.AppConfigurations.Include(ac => ac.CreatedBy).Where(ac => ac.ApplicationId == appId).Select(ac => ac.Version).ToListAsync();
        }

        public async Task<AppConfiguration?> GetByAppIdAndVersionAsync(Guid appId, int version)
        {
            return await dbContext.AppConfigurations.FirstOrDefaultAsync(ac => ac.ApplicationId == appId && ac.Version == version);
        }

        public async Task DeleteAsync(AppConfiguration appConfig)
        {
            dbContext.AppConfigurations.Remove(appConfig);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteByAppIdAsync(Guid appId)
        {
            var configs = await dbContext.AppConfigurations.Where(ac => ac.ApplicationId == appId).ToListAsync();
            dbContext.AppConfigurations.RemoveRange(configs);
            await dbContext.SaveChangesAsync();
        }

        public async Task CleanupOldConfigurations(Guid appId)
        {
            var configs = await dbContext.AppConfigurations.Where(ac => ac.ApplicationId == appId).OrderByDescending(ac => ac.Version).ToListAsync();
            if (configs.Any())
            {
                var latestVersion = configs.First().Version;
                var oldConfigs = configs.Where(c => c.Version < latestVersion).ToList();
                if (oldConfigs.Any())
                {
                    dbContext.AppConfigurations.RemoveRange(oldConfigs);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
