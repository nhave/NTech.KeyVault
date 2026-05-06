using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Text;

namespace NTech.KeyVault.Api.Services
{
    public interface IAppIOService
    {
        public Task<SimpleApplicationResponse> ValidateAsync(Guid applicationId, string applicationSecret);
        public Task<ApplicationConfigurationResponse> GetConfigurationByAppIdAsync(Guid applicationId);
        public Task<string> GetAppSecretValueAsync(Guid applicationId, string secretName);
        public Task SetAppSecretAsync(Guid applicationId, string name, string value);
        public Task DeleteAppSecretAsync(Guid applicationId, Guid secretId);
    }

    public class AppIOService(IApplicationRepository applicationRepository, IAppConfigurationService appConfigurationService, IAppSecretRepository secretRepository, IEncryptionService encryptionService) : IAppIOService
    {
        public async Task<SimpleApplicationResponse> ValidateAsync(Guid applicationId, string applicationSecret)
        {
            var application = await applicationRepository.GetApplicationByIdAsync(applicationId)
                ?? throw new KeyNotFoundException($"Application with ID {applicationId} not found.");

            var providedSecretHash = encryptionService.CreateLookupHash(applicationSecret);
            if (application.AppSecretHash != providedSecretHash)
                throw new UnauthorizedAccessException("Invalid application secret.");

            return new SimpleApplicationResponse
            (
                application.Id,
                application.Name,
                application.Description,
                application.OwnerUserId
            );
        }

        public async Task<ApplicationConfigurationResponse> GetConfigurationByAppIdAsync(Guid applicationId)
        {
            return await appConfigurationService.GetByAppIdAsync(applicationId);
        }

        public async Task<string> GetAppSecretValueAsync(Guid applicationId, string secretName)
        {
            var secret = await secretRepository.GetVaultSecretByNameAsync(applicationId, secretName)
                ?? throw new KeyNotFoundException($"Secret with name {secretName} not found for application with ID {applicationId}.");

            var decryptionResult = encryptionService.Decrypt(secret.EncryptedData, secret.EncryptedDataKey, secret.DataNonce, secret.DataKeyNonce);
            var value = Encoding.UTF8.GetString(decryptionResult.Value);

            return value;
        }

        public async Task SetAppSecretAsync(Guid applicationId, string name, string value)
        {
            name = name.Trim().ToLower(); // Ensure consistent naming
    
            var encryptionResult = encryptionService.Encrypt(Encoding.UTF8.GetBytes(value));
    
            var secret = await secretRepository.GetVaultSecretByNameAsync(applicationId, name);
            if (secret == null)
            {
                secret = new AppSecret
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    Name = name,
                    EncryptedData = encryptionResult.EncryptedValue,
                    DataNonce = encryptionResult.ValueNonce,
                    EncryptedDataKey = encryptionResult.EncryptedDataKey,
                    DataKeyNonce = encryptionResult.DataKeyNonce
                };
    
                await secretRepository.CreateVaultSecretAsync(secret);
            }
            else
            {
                secret.EncryptedData = encryptionResult.EncryptedValue;
                secret.DataNonce = encryptionResult.ValueNonce;
                secret.EncryptedDataKey = encryptionResult.EncryptedDataKey;
                secret.DataKeyNonce = encryptionResult.DataKeyNonce;
    
                await secretRepository.UpdateVaultSecretAsync(secret);
            }
        }

        public async Task DeleteAppSecretAsync(Guid applicationId, Guid secretId)
        {
            var secret = await secretRepository.GetVaultSecretByIdAsync(secretId)
                ?? throw new KeyNotFoundException($"Secret with ID {secretId} not found.");

            if (secret.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("Secret does not belong to the specified application.");

            await secretRepository.DeleteVaultSecretAsync(secretId);
        }
    }
}
