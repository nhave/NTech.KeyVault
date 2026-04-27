using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Services
{
    public interface IApplicationService
    {
        public Task<DecryptedApplication> CreateApplicationAsync(CreateApplicationRequest dto);
        public Task<List<ApplicationResponse>> GetAccessibleApplicationsAsync();
        public Task SetUserPermissionsAsync(Guid applicationId, Guid userId, List<Permission> permissions);
        public Task<ApplicationUsersResponse> GetApplicationUsersAsync(Guid applicationId);
        public Task RemoveUserPermissionsAsync(Guid applicationId, Guid userId);
    }

    public class ApplicationService(IApplicationRepository applicationRepository, IUserService userService, IPermissionService permissionService) : IApplicationService
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

            List<Application> allApplications = new();

            var userApplications = await applicationRepository.GetApplicationsByUserId(user.Id);
            allApplications.AddRange(userApplications);

            var principalPermissions = await permissionService.GetPrincipalsForUserAsync(user.Id, ResourceType.Application);
            var accessibleApplicationIds = principalPermissions.Select(p => p.ResourceId).ToList();
            var accessibleApplications = await applicationRepository.GetApplicationsByIdsAsync(accessibleApplicationIds);
            allApplications.AddRange(accessibleApplications);

            return allApplications.Select(app => new ApplicationResponse
            (
                app.Id,
                app.Name,
                app.Description,
                app.OwnerUserId,
                app.OwnerUser?.Username,
                app.OwnerUserId == user.Id ? Enum.GetNames<Permission>().ToList() : 
                    principalPermissions.Where(p => p.ResourceId == app.Id)
                        .SelectMany(p => p.Permissions)
                        .Select(p => p.ToString())
                        .ToList()
            )).ToList();
        }

        public async Task SetUserPermissionsAsync(Guid applicationId, Guid userId, List<Permission> permissions)
        {
            if (permissions == null || permissions.Count == 0)
                throw new ArgumentException("Permissions list cannot be null or empty.");

            var currentUser = await userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new Exception("Failed to get signed in user.");

            if (currentUser.Id == userId)
                throw new ArgumentException("Users cannot set permissions for themselves.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null)
                throw new ArgumentException("Application not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner)
                throw new InvalidOperationException("Only the owner can set permissions.");

            await permissionService.SetPermissionsAsync(
                PrincipalType.User,
                userId,
                ResourceType.Application,
                applicationId,
                permissions);
        }

        public async Task<ApplicationUsersResponse> GetApplicationUsersAsync(Guid applicationId)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new Exception("Failed to get signed in user.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null)
                throw new ArgumentException("Application not found.");

            if (application.OwnerUserId != currentUser.Id)
                throw new InvalidOperationException("Only the owner can view user permissions.");

            var permissions = await permissionService.GetPrincipalsForResourceAsync(ResourceType.Application, applicationId);
            var userIds = permissions.Where(p => p.PrincipalType == PrincipalType.User).Select(p => p.PrincipalId).ToList();
            var users = await userService.GetUsersByIdsAsync(userIds);

            return new ApplicationUsersResponse
            (
                applicationId,
                users.Select(u => new ApplicationUserResponse
                (
                    u.Id,
                    u.Username,
                    permissions.Where(p => p.PrincipalType == PrincipalType.User && p.PrincipalId == u.Id)
                        .SelectMany(p => p.Permissions)
                        .Select(p => p.ToString())
                        .ToList()
                )).ToList()
            );
        }

        public async Task RemoveUserPermissionsAsync(Guid applicationId, Guid userId)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new Exception("Failed to get signed in user.");

            if (currentUser.Id == userId)
                throw new ArgumentException("Users cannot remove permissions for themselves.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null)
                throw new ArgumentException("Application not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner)
                throw new InvalidOperationException("Only the owner can remove permissions.");

            await permissionService.RemovePermissionsAsync(
                PrincipalType.User,
                userId,
                ResourceType.Application,
                applicationId);
        }
    }
}
