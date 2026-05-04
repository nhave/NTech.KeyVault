using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Text;

namespace NTech.KeyVault.Api.Services
{
    public interface IApplicationService
    {
        public Task<DecryptedApplication> CreateApplicationAsync(CreateApplicationRequest dto);
        public Task<DecryptedApplication> GetApplicationDetailsAsync(Guid applicationId);
        public Task DeleteApplicationAsync(Guid applicationId, string applicationName);
        public Task<List<ApplicationResponse>> GetAccessibleApplicationsAsync();
        public Task SetUserPermissionsAsync(Guid applicationId, Guid userId, List<Permission> permissions);
        public Task<ApplicationUserResponse> GetUserPermissionsAsync(Guid applicationId, Guid userId);
        public Task<ApplicationUsersResponse> GetApplicationUsersAsync(Guid applicationId);
        public Task RemoveUserPermissionsAsync(Guid applicationId, Guid userId);
    }

    public class ApplicationService(IApplicationRepository applicationRepository, IUserService userService, IPermissionService permissionService, IEncryptionService encryptionService) : IApplicationService
    {
        public async Task<DecryptedApplication> CreateApplicationAsync(CreateApplicationRequest dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentNullException(nameof(dto.Name));

            // Normalize description to null if it's empty or whitespace
            var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description;

            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("Failed to get signed in user.");

            return await applicationRepository.CreateApplicationAsync(user.Id, dto.Name, dto.Description);
        }

        public async Task<DecryptedApplication> GetApplicationDetailsAsync(Guid applicationId)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new Exception("Failed to get signed in user.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId)
                ?? throw new KeyNotFoundException($"Application with ID {applicationId} not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, applicationId, Permission.Application_Admin))
                throw new UnauthorizedAccessException("User does not have permission to access this application's information.");

            var decrypted = encryptionService.Decrypt(
                application.EncryptedSecret,
                application.EncryptedDataKey,
                application.SecretNonce,
                application.DataKeyNonce
            );
            var secret = Encoding.UTF8.GetString(decrypted.Value);

            return new DecryptedApplication
            {
                Id = application.Id,
                Name = application.Name,
                Description = application.Description,
                OwnerUserId = application.OwnerUserId,
                AppSecret = secret
            };
        }

        public async Task DeleteApplicationAsync(Guid applicationId, string applicationName)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new Exception("Failed to get signed in user.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner && !await permissionService.HasPermissionAsync(currentUser.Id, new List<Guid>(), ResourceType.Application, applicationId, Permission.Application_Admin))
                throw new UnauthorizedAccessException("User does not have permission to delete this application.");

            if (application.Name != applicationName)
                throw new ArgumentException("Application name does not match. Deletion aborted.");

            await permissionService.RemovePrincipalsFromResourceAsync(ResourceType.Application, applicationId);
            await applicationRepository.DeleteApplicationAsync(application);
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
                app.OwnerUserId == user.Id ? PermissionHelper.GetPermissionsForAsStrings(ResourceType.Application) : 
                    principalPermissions.Where(p => p.ResourceId == app.Id)
                        .SelectMany(p => p.Permissions)
                        .Where(p => PermissionHelper.IsValidPermissionFor(ResourceType.Application, p))
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
                throw new KeyNotFoundException("Application not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner)
                throw new UnauthorizedAccessException("Only the owner can set permissions.");

            var user = await userService.GetUserById(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

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
                throw new KeyNotFoundException("Application not found.");

            if (application.OwnerUserId != currentUser.Id)
                throw new UnauthorizedAccessException("Only the owner can view user permissions.");

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
                        .Where(p => PermissionHelper.IsValidPermissionFor(ResourceType.Application, p))
                        .Select(p => p.ToString())
                        .ToList()
                )).ToList()
            );
        }

        public async Task<ApplicationUserResponse> GetUserPermissionsAsync(Guid applicationId, Guid userId)
        {
            var currentUser = await userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new Exception("Failed to get signed in user.");

            var application = await applicationRepository.GetApplicationByIdAsync(applicationId);
            if (application == null)
                throw new KeyNotFoundException("Application not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            var isSelf = currentUser.Id == userId;
            if (!isOwner && !isSelf)
                throw new UnauthorizedAccessException("Only the owner or the user themselves can view permissions.");

            var user = await userService.GetUserById(userId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var permissions = await permissionService.GetPrincipalsForResourceAsync(ResourceType.Application, applicationId);
            var userPermissions = permissions.Where(p => p.PrincipalType == PrincipalType.User && p.PrincipalId == userId)
                .SelectMany(p => p.Permissions)
                .Where(p => PermissionHelper.IsValidPermissionFor(ResourceType.Application, p))
                .Select(p => p.ToString())
                .ToList();

            return new ApplicationUserResponse
            (
                user.Id,
                user.Username,
                userPermissions
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
                throw new KeyNotFoundException("Application not found.");

            var isOwner = application.OwnerUserId == currentUser.Id;
            if (!isOwner)
                throw new UnauthorizedAccessException("Only the owner can remove permissions.");

            await permissionService.RemovePermissionsAsync(
                PrincipalType.User,
                userId,
                ResourceType.Application,
                applicationId);
        }
    }
}
