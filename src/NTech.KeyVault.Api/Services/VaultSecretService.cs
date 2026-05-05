using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Database;
using System.Text;

namespace NTech.KeyVault.Api.Services
{
    public interface IVaultSecretService
    {
        public Task SetVaultSecretAsync(Guid applicationId, string entityId, string secretValue, DateTime ExpirationDate);
        public Task<string?> GetVaultSecretAsync(Guid applicationId, string entityId);
        public Task DeleteVaultSecretAsync(Guid applicationId, string entityId);
    }

    public class VaultSecretService(IVaultSecretRepository vaultSecretRepository, IEncryptionService encryptionService) : IVaultSecretService
    {
        public async Task SetVaultSecretAsync(Guid applicationId, string entityId, string secretValue, DateTime ExpirationDate)
        {
            if (string.IsNullOrWhiteSpace(secretValue))
                throw new ArgumentNullException(nameof(secretValue), "Secret value cannot be null or empty.");

            if (ExpirationDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future.", nameof(ExpirationDate));

            if (ExpirationDate > DateTime.UtcNow.AddYears(1))
                throw new ArgumentException("Expiration date cannot be more than 1 year in the future.", nameof(ExpirationDate));

            var existingVaultSecret = await vaultSecretRepository.GetVaultSecretByEntityIdAsync(applicationId, entityId);

            var encodedSecretValue = Encoding.UTF8.GetBytes(secretValue);
            var encryptionResult = encryptionService.Encrypt(encodedSecretValue);

            if (existingVaultSecret != null)
            {
                existingVaultSecret.EncryptedValue = encryptionResult.EncryptedValue;
                existingVaultSecret.ValueNonce = encryptionResult.ValueNonce;
                existingVaultSecret.EncryptedDataKey = encryptionResult.EncryptedDataKey;
                existingVaultSecret.DataKeyNonce = encryptionResult.DataKeyNonce;
                existingVaultSecret.ExpirationDate = ExpirationDate;

                await vaultSecretRepository.UpdateVaultSecretAsync(existingVaultSecret);
            }
            else
            {
                var newVaultSecret = new VaultSecret
                {
                    ApplicationId = applicationId,
                    EntityId = entityId,
                    EncryptedValue = encryptionResult.EncryptedValue,
                    ValueNonce = encryptionResult.ValueNonce,
                    EncryptedDataKey = encryptionResult.EncryptedDataKey,
                    DataKeyNonce = encryptionResult.DataKeyNonce,
                    ExpirationDate = ExpirationDate
                };
                await vaultSecretRepository.CreateVaultSecretAsync(newVaultSecret);
            }
        }

        public async Task<string?> GetVaultSecretAsync(Guid applicationId, string entityId)
        {
            var vaultSecret = await vaultSecretRepository.GetVaultSecretByEntityIdAsync(applicationId, entityId)
                ?? throw new KeyNotFoundException($"Vault secret with entity ID {entityId} not found for application with ID {applicationId}.");
            
            var decryptionResult = encryptionService.Decrypt(
                vaultSecret.EncryptedValue,
                vaultSecret.EncryptedDataKey,
                vaultSecret.ValueNonce,
                vaultSecret.DataKeyNonce
            );
            return Encoding.UTF8.GetString(decryptionResult.Value);
        }

        public async Task DeleteVaultSecretAsync(Guid applicationId, string entityId)
        {
            var vaultSecret = await vaultSecretRepository.GetVaultSecretByEntityIdAsync(applicationId, entityId)
                ?? throw new KeyNotFoundException($"Vault secret with entity ID {entityId} not found for application with ID {applicationId}.");
            
            await vaultSecretRepository.DeleteVaultSecretAsync(vaultSecret.Id);
        }
    }
}
