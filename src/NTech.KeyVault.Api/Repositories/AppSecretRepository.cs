using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IAppSecretRepository
    {
        public Task CreateVaultSecretAsync(AppSecret vaultSecret);
        public Task<AppSecret?> GetVaultSecretByIdAsync(Guid id);
        public Task<AppSecret?> GetVaultSecretByNameAsync(Guid applicationId, string name);
        public Task<List<AppSecret>> GetVaultSecretsByApplicationIdAsync(Guid applicationId);
        public Task UpdateVaultSecretAsync(AppSecret vaultSecret);
        public Task DeleteVaultSecretAsync(Guid id);
    }

    public class AppSecretRepository(AppDbContext dbContext) : IAppSecretRepository
    {
        public async Task CreateVaultSecretAsync(AppSecret vaultSecret)
        {
            dbContext.AppSecrets.Add(vaultSecret);
            await dbContext.SaveChangesAsync();
        }

        public async Task<AppSecret?> GetVaultSecretByIdAsync(Guid id)
        {
            return await dbContext.AppSecrets
                .Include(vs => vs.Application)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }

        public async Task<AppSecret?> GetVaultSecretByNameAsync(Guid applicationId, string name)
        {
            return await dbContext.AppSecrets
                .Include(vs => vs.Application)
                .Where(vs => vs.ApplicationId == applicationId && vs.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AppSecret>> GetVaultSecretsByApplicationIdAsync(Guid applicationId)
        {
            return await dbContext.AppSecrets
                .Include(vs => vs.Application)
                .Where(vs => vs.ApplicationId == applicationId)
                .ToListAsync();
        }

        public async Task UpdateVaultSecretAsync(AppSecret vaultSecret)
        {
            dbContext.AppSecrets.Update(vaultSecret);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteVaultSecretAsync(Guid id)
        {
            var vaultSecret = await dbContext.AppSecrets.FindAsync(id);
            if (vaultSecret != null)
            {
                dbContext.AppSecrets.Remove(vaultSecret);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
