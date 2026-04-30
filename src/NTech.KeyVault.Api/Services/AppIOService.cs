using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Services
{
    public interface IAppIOService
    {
        public Task<SimpleApplicationResponse> ValidateAsync(string applicationId, string applicationSecret);
    }

    public class AppIOService(IApplicationRepository applicationRepository, IEncryptionService encryptionService) : IAppIOService
    {
        public async Task<SimpleApplicationResponse> ValidateAsync(string applicationId, string applicationSecret)
        {
            var appIdGuid = Guid.TryParse(applicationId, out var parsedAppId)
                ? parsedAppId
                : throw new ArgumentException("Invalid application ID format.", nameof(applicationId));

            var application = await applicationRepository.GetApplicationByIdAsync(appIdGuid)
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
    }
}
