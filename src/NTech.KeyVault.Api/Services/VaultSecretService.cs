using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Text;
using System.Xml.Linq;

namespace NTech.KeyVault.Api.Services
{
    public interface IVaultSecretService
    {
        public Task<VaultSecretResponse> SetSecretAsync(Guid ApplicationId, string Name, string Value);
        public Task<VaultSecretResponse> GetVaultSecretAsync(Guid applicationId, string name);
        public Task<string> GetSecretValueAsync(Guid secretId);
        public Task<List<VaultSecretResponse>> GetVaultSecretsByApplicationIdAsync(Guid ApplicationId);
        public Task DeleteVaultSecretAsync(Guid secretId, string name);
    }

    public class VaultSecretService(IVaultSecretRepository secretRepository, IApplicationRepository applicationRepository, IEncryptionService encryptionService, IUserService userService, IPermissionService permissionService) : IVaultSecretService
    {
        public async Task<VaultSecretResponse> SetSecretAsync(Guid applicationId, string name, string value)
        {
            name = name.Trim().ToLower(); // Ensure consistent naming

            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId)
                ?? throw new KeyNotFoundException($"Application with ID {applicationId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, applicationId, Permission.Application_Secret_Write))
                throw new UnauthorizedAccessException("User does not have permission to modify this application's secrets.");

            var encryptionResult = encryptionService.Encrypt(Encoding.UTF8.GetBytes(value));

            var secret = await secretRepository.GetVaultSecretByNameAsync(applicationId, name);
            if (secret == null)
            {
                secret = new VaultSecret
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

            return new VaultSecretResponse(application.Id, secret.Id, secret.Name, secret.CreatedAt, secret.UpdatedAt);
        }

        public async Task<VaultSecretResponse> GetVaultSecretAsync(Guid applicationId, string name)
        {
            name = name.Trim().ToLower(); // Ensure consistent naming

            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId)
                ?? throw new KeyNotFoundException($"Application with ID {applicationId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, applicationId, Permission.Application_Secret_Read))
                throw new UnauthorizedAccessException("User does not have permission to view this application's secrets.");

            var secret = await secretRepository.GetVaultSecretByNameAsync(applicationId, name)
                ?? throw new KeyNotFoundException($"Secret with name {name} not found for application with ID {applicationId}.");

            return new VaultSecretResponse(application.Id, secret.Id, secret.Name, secret.CreatedAt, secret.UpdatedAt);
        }

        public async Task<string> GetSecretValueAsync(Guid secretId)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var secret = await secretRepository.GetVaultSecretByIdAsync(secretId)
                ?? throw new KeyNotFoundException($"Secret with id {secretId} not found.");

            var isOwner = secret.Application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, secret.ApplicationId, Permission.Application_Secret_Read))
                throw new UnauthorizedAccessException("User does not have permission to view this application's secrets.");

            var decryptionResult = encryptionService.Decrypt(secret.EncryptedData, secret.EncryptedDataKey, secret.DataNonce, secret.DataKeyNonce);
            var value = Encoding.UTF8.GetString(decryptionResult.Value);

            return value;
        }

        public async Task<List<VaultSecretResponse>> GetVaultSecretsByApplicationIdAsync(Guid ApplicationId)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(ApplicationId)
                ?? throw new KeyNotFoundException($"Application with ID {ApplicationId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, ApplicationId, Permission.Application_Secret_Read))
                throw new UnauthorizedAccessException("User does not have permission to view this application's secrets.");

            var secrets = await secretRepository.GetVaultSecretsByApplicationIdAsync(ApplicationId);
            return secrets.Select(s => new VaultSecretResponse(s.ApplicationId, s.Id, s.Name, s.CreatedAt, s.UpdatedAt)).ToList();
        }

        public async Task DeleteVaultSecretAsync(Guid secretId, string name)
        {
            var secret = await secretRepository.GetVaultSecretByIdAsync(secretId)
                ?? throw new KeyNotFoundException($"Secret with ID {secretId} not found.");

            if (secret.Name != name.Trim().ToLower())
                throw new ArgumentException("Secret name does not match the provided name.");

            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(secret.ApplicationId)
                ?? throw new KeyNotFoundException($"Application with ID {secret.ApplicationId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, secret.ApplicationId, Permission.Application_Secret_Write))
                throw new UnauthorizedAccessException("User does not have permission to delete this application's secrets.");

            await secretRepository.DeleteVaultSecretAsync(secretId);
        }
    }
}
