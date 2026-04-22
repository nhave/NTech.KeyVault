using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Services
{
    public interface IApplicationService
    {
        public Task<DecryptedApplication> CreateApplicationAsync(CreateApplicationRequest dto);
        public Task<List<ApplicationResponse>> GetAccessibleApplicationsAsync();
    }

    public class ApplicationService(IApplicationRepository applicationRepository, IUserService userService) : IApplicationService
    {
        public async Task<DecryptedApplication> CreateApplicationAsync(CreateApplicationRequest dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentNullException(nameof(dto.Name));
            
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("Failed to get signed in user.");

            return await applicationRepository.CreateApplicationAsync(user.Id, dto.Name, dto.Description);
        }

        public async Task<List<ApplicationResponse>> GetAccessibleApplicationsAsync()
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("Failed to get signed in user.");

            var applications = await applicationRepository.GetAllApplicationsAsync();
            
            return applications.Select(app => new ApplicationResponse
            (
                app.Id,
                app.Name,
                app.Description,
                app.OwnerUserId,
                app.OwnerUser?.Username
            )).ToList();
        }
    }
}
