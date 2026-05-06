using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IVaultSecretRepository
    {
        public Task CreateVaultSecretAsync(VaultSecret vaultSecret);
        public Task<VaultSecret?> GetVaultSecretByIdAsync(Guid id);
        public Task<VaultSecret?> GetVaultSecretByEntityIdAsync(Guid applicationId, string entityId);
        public Task UpdateVaultSecretAsync(VaultSecret vaultSecret);
        public Task DeleteVaultSecretAsync(Guid id);
        public Task DeleteVaultSecretsByApplicationIdAsync(Guid applicationId);
    }

    public class VaultSecretRepository(AppDbContext dbContext) : IVaultSecretRepository
    {
        public async Task CreateVaultSecretAsync(VaultSecret vaultSecret)
        {
            dbContext.VaultSecrets.Add(vaultSecret);
            await dbContext.SaveChangesAsync();
        }

        public async Task<VaultSecret?> GetVaultSecretByIdAsync(Guid id)
        {
            return await dbContext.VaultSecrets.FindAsync(id);
        }

        public async Task<VaultSecret?> GetVaultSecretByEntityIdAsync(Guid applicationId, string entityId)
        {
            return await dbContext.VaultSecrets
                .FirstOrDefaultAsync(vs => vs.ApplicationId == applicationId && vs.EntityId == entityId);
        }

        public async Task UpdateVaultSecretAsync(VaultSecret vaultSecret)
        {
            dbContext.VaultSecrets.Update(vaultSecret);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteVaultSecretAsync(Guid id)
        {
            var vaultSecret = await dbContext.VaultSecrets.FindAsync(id);
            if (vaultSecret != null)
            {
                dbContext.VaultSecrets.Remove(vaultSecret);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteVaultSecretsByApplicationIdAsync(Guid applicationId)
        {
            var vaultSecrets = await dbContext.VaultSecrets
                .Where(vs => vs.ApplicationId == applicationId)
                .ToListAsync();

            if (vaultSecrets.Count != 0)
            {
                dbContext.VaultSecrets.RemoveRange(vaultSecrets);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
