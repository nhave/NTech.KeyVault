using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IVaultSecretRepository
    {
        public Task CreateVaultSecretAsync(VaultSecret vaultSecret);
        public Task<VaultSecret?> GetVaultSecretByIdAsync(Guid id);
        public Task<VaultSecret?> GetVaultSecretByNameAsync(Guid applicationId, string name);
        public Task<List<VaultSecret>> GetVaultSecretsByApplicationIdAsync(Guid applicationId);
        public Task UpdateVaultSecretAsync(VaultSecret vaultSecret);
        public Task DeleteVaultSecretAsync(Guid id);
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
            return await dbContext.VaultSecrets
                .Include(vs => vs.Application)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }

        public async Task<VaultSecret?> GetVaultSecretByNameAsync(Guid applicationId, string name)
        {
            return await dbContext.VaultSecrets
                .Include(vs => vs.Application)
                .Where(vs => vs.ApplicationId == applicationId && vs.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<VaultSecret>> GetVaultSecretsByApplicationIdAsync(Guid applicationId)
        {
            return await dbContext.VaultSecrets
                .Include(vs => vs.Application)
                .Where(vs => vs.ApplicationId == applicationId)
                .ToListAsync();
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
    }
}
