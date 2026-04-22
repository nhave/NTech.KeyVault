using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;
using System.Text;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IApplicationRepository
    {
        public Task<DecryptedApplication> CreateApplicationAsync(Guid? userId, string applicationName, string? description = null);
        public Task<Application?> GetApplicationById(Guid applicationId);
        public Task<List<Application>> GetApplicationsByUserId(Guid userId);
        public Task<List<Application>> GetAllApplicationsAsync();
        public Task UpdateApplicationAsync(Application application);
        public Task DeleteApplicationAsync(Application application);
    }

    public class ApplicationRepository(AppDbContext dbContext, IEncryptionService encryptionService) : IApplicationRepository
    {
        public async Task<DecryptedApplication> CreateApplicationAsync(Guid? userId, string applicationName, string? description = null)
        {
            var appSecret = Guid.NewGuid().ToString("N");
            var secretBytes = Encoding.UTF8.GetBytes(appSecret);
            var enc = encryptionService.Encrypt(secretBytes);

            var appSecretHash = encryptionService.CreateLookupHash(appSecret);

            var application = new Application
            {
                OwnerUserId = userId,
                Name = applicationName,
                Description = description,
                AppSecretHash = appSecretHash,
                EncryptedSecret = enc.EncryptedValue,
                SecretNonce = enc.ValueNonce,
                EncryptedDataKey = enc.EncryptedDataKey,
                DataKeyNonce = enc.DataKeyNonce
            };

            await dbContext.Applications.AddAsync(application);
            await dbContext.SaveChangesAsync();

            return new DecryptedApplication
            {
                Id = application.Id,
                AppSecret = appSecret,
                Name = application.Name,
                Description = application.Description,
                OwnerUserId = application.OwnerUserId
            };
        }

        public async Task<Application?> GetApplicationById(Guid applicationId)
        {
            return await dbContext.Applications
                .Include(a => a.OwnerUser)
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task<List<Application>> GetApplicationsByUserId(Guid userId)
        {
            return await dbContext.Applications
                .Include(a => a.OwnerUser)
                .Where(a => a.OwnerUserId == userId)
                .ToListAsync();
        }

        public async Task<List<Application>> GetAllApplicationsAsync()
        {
            return await dbContext.Applications
                .Include(a => a.OwnerUser)
                .ToListAsync();
        }

        public async Task UpdateApplicationAsync(Application application)
        {
            dbContext.Applications.Update(application);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteApplicationAsync(Application application)
        {
            dbContext.Applications.Remove(application);
            await dbContext.SaveChangesAsync();
        }
    }
}
