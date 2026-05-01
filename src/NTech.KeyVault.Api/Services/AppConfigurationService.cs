using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Text;
using System.Text.Json;

namespace NTech.KeyVault.Api.Services
{
    public interface IAppConfigurationService
    {
        public Task AddOrUpdateAsync(Guid appId, Dictionary<string, object> configData);
        public Task<ApplicationConfigurationResponse> GetByAppIdWithPermissionsAsync(Guid appId);
        public Task<ApplicationConfigurationResponse> GetByAppIdAsync(Guid appId);
        public Task<ApplicationConfigurationResponse> GetByAppIdAndVersionAsync(Guid appId, int version);
        public Task<List<int>> GetAllVersionsByAppIdAsync(Guid appId);
        public Task DeleteByAppIdAsync(Guid appId);
        public Task CleanupOldConfigurations(Guid appId);
    }

    public class AppConfigurationService(IAppConfigurationRepository configurationRepository, IApplicationRepository applicationRepository, IEncryptionService encryptionService, IUserService userService, IPermissionService permissionService) : IAppConfigurationService
    {
        public async Task AddOrUpdateAsync(Guid appId, Dictionary<string, object> configData)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(appId)
                ?? throw new KeyNotFoundException($"Application with ID {appId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, appId, Permission.Application_Config_Write))
                throw new UnauthorizedAccessException("User does not have permission to modify this application's configuration.");

            var serializedData = JsonSerializer.Serialize(configData);
            var encryptionResult = encryptionService.Encrypt(Encoding.UTF8.GetBytes(serializedData));

            var newConfig = new AppConfiguration
            {
                ApplicationId = appId,
                EncryptedData = encryptionResult.EncryptedValue,
                EncryptedDataKey = encryptionResult.EncryptedDataKey,
                DataNonce = encryptionResult.ValueNonce,
                DataKeyNonce = encryptionResult.DataKeyNonce,
                CreatedById = currentUser.Id,
                CreatedBy = currentUser
            };

            await configurationRepository.AddOrUpdate(newConfig);
        }

        public async Task<ApplicationConfigurationResponse> GetByAppIdWithPermissionsAsync(Guid appId)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(appId)
                ?? throw new KeyNotFoundException($"Application with ID {appId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, appId, Permission.Application_Config_Read))
                throw new UnauthorizedAccessException("User does not have permission to view this application's configuration.");

            return await GetByAppIdAsync(appId);
        }

        public async Task<ApplicationConfigurationResponse> GetByAppIdAsync(Guid appId)
        {
            var config = await configurationRepository.GetLatestByAppIdAsync(appId);
            if (config == null)
                return new ApplicationConfigurationResponse(appId);

            var decryptedData = DecryptConfig(config);

            return new ApplicationConfigurationResponse(
                config.ApplicationId,
                config.Version,
                config.CreatedById,
                config.CreatedBy?.Username,
                decryptedData
            );
        }

        public async Task<ApplicationConfigurationResponse> GetByAppIdAndVersionAsync(Guid appId, int version)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(appId)
                ?? throw new KeyNotFoundException($"Application with ID {appId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, appId, Permission.Application_Config_Read))
                throw new UnauthorizedAccessException("User does not have permission to view this application's configuration.");

            var config = await configurationRepository.GetByAppIdAndVersionAsync(appId, version)
                ?? throw new KeyNotFoundException($"No configuration found for application ID {appId} and version {version}");
            
            var decryptedData = DecryptConfig(config);

            return new ApplicationConfigurationResponse(
                config.ApplicationId,
                config.Version,
                config.CreatedById,
                config.CreatedBy?.Username,
                decryptedData
            );
        }

        public async Task<List<int>> GetAllVersionsByAppIdAsync(Guid appId)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(appId)
                ?? throw new KeyNotFoundException($"Application with ID {appId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, appId, Permission.Application_Config_Read))
                throw new UnauthorizedAccessException("User does not have permission to view this application's configuration.");

            var configs = await configurationRepository.GetAllVersionsByAppIdAsync(appId);
            if (configs == null || !configs.Any())
            {
                throw new KeyNotFoundException($"No configurations found for application ID {appId}");
            }
            
            return configs;
        }

        public async Task DeleteByAppIdAsync(Guid appId)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(appId)
                ?? throw new KeyNotFoundException($"Application with ID {appId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, appId, Permission.Application_Config_Write))
                throw new UnauthorizedAccessException("User does not have permission to modify this application's configuration.");

            await configurationRepository.DeleteByAppIdAsync(appId);
        }

        public async Task CleanupOldConfigurations(Guid appId)
        {
            var currentUser = await userService.GetCurrentUserAsync()
                ?? throw new Exception("Current user not found.");

            var application = await applicationRepository.GetApplicationByIdAsync(appId)
                ?? throw new KeyNotFoundException($"Application with ID {appId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, appId, Permission.Application_Config_Write))
                throw new UnauthorizedAccessException("User does not have permission to modify this application's configuration.");

            await configurationRepository.CleanupOldConfigurations(appId);
        }

        // Helper method to decrypt configuration data and deserialize it into a dictionary
        private Dictionary<string, object> DecryptConfig(AppConfiguration config)
        {
            var decryptionResult = encryptionService.Decrypt(config.EncryptedData, config.EncryptedDataKey, config.DataNonce, config.DataKeyNonce);
            var deserializedData = JsonSerializer.Deserialize<Dictionary<string, object>>(decryptionResult.Value)
                ?? throw new Exception("Failed to deserialize configuration data.");

            return deserializedData;
        }
    }
}
