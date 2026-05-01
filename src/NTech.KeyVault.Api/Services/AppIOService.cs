using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Services
{
    public interface IAppIOService
    {
        public Task<SimpleApplicationResponse> ValidateAsync(Guid applicationId, string applicationSecret);
        public Task<ApplicationConfigurationResponse> GetByAppIdAsync(Guid applicationId, string applicationSecret);
    }

    public class AppIOService(IApplicationRepository applicationRepository, IAppConfigurationService appConfigurationService, IEncryptionService encryptionService) : IAppIOService
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

        public async Task<ApplicationConfigurationResponse> GetByAppIdAsync(Guid applicationId, string applicationSecret)
        {
            var application = await applicationRepository.GetApplicationByIdAsync(applicationId)
                ?? throw new KeyNotFoundException($"Application with ID {applicationId} not found.");

            var providedSecretHash = encryptionService.CreateLookupHash(applicationSecret);
            if (application.AppSecretHash != providedSecretHash)
                throw new UnauthorizedAccessException("Invalid application secret.");

            return await appConfigurationService.GetByAppIdAsync(applicationId);
        }
    }
}
