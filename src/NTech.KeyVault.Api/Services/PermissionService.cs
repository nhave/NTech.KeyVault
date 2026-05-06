using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Services
{
    public interface IPermissionService
    {
        public Task<List<Permission>> GetPermissionsAsync(
            Guid userId,
            IEnumerable<Guid> teamIds,
            ResourceType resourceType,
            Guid resourceId);
        public Task<bool> HasPermissionAsync(
            Guid userId,
            IEnumerable<Guid> teamIds,
            ResourceType resourceType,
            Guid resourceId,
            Permission required);
        public Task SetPermissionsAsync(
            PrincipalType principalType,
            Guid principalId,
            ResourceType resourceType,
            Guid resourceId,
            List<Permission> permissions);
        public Task RemovePermissionsAsync(
            PrincipalType principalType,
            Guid principalId,
            ResourceType resourceType,
            Guid resourceId);
        public Task RemovePrincipalsFromResourceAsync(
            ResourceType resourceType,
            Guid resourceId);
        public Task<List<PrincipalPermission>> GetPrincipalsForUserAsync(
            Guid userId,
            ResourceType resourceType);
        public Task<List<PrincipalPermission>> GetPrincipalsForTeamAsync(
            Guid teamId,
            ResourceType resourceType);
        public Task<List<PrincipalPermission>> GetPrincipalsForResourceAsync(
            ResourceType resourceType,
            Guid resourceId);
    }

    public class PermissionService(IPrincipalPermissionRepository permissionRepository) : IPermissionService
    {
        public async Task<List<Permission>> GetPermissionsAsync(
            Guid userId,
            IEnumerable<Guid> teamIds,
            ResourceType resourceType,
            Guid resourceId)
        {
            var all = new List<PrincipalPermission>();

            // Brugerens permissions
            var userPerms = await permissionRepository.GetPermissionsAsync(
                PrincipalType.User,
                userId,
                resourceType,
                resourceId);

            if (userPerms != null)
                all.AddRange(userPerms);

            // Team permissions
            foreach (var teamId in teamIds)
            {
                var teamPerms = await permissionRepository.GetPermissionsAsync(
                    PrincipalType.Team,
                    teamId,
                    resourceType,
                    resourceId);

                if (teamPerms != null)
                    all.AddRange(teamPerms);
            }

            // Flad liste af enum-permissions
            return all
                .SelectMany(p => p.Permissions)
                .Distinct()
                .ToList()
                .ExpandPermissions();
        }

        public async Task<bool> HasPermissionAsync(
            Guid userId,
            IEnumerable<Guid> teamIds,
            ResourceType resourceType,
            Guid resourceId,
            Permission required)
        {
            var perms = await GetPermissionsAsync(
                userId,
                teamIds,
                resourceType,
                resourceId);

            return perms.Contains(required);
        }

        public async Task SetPermissionsAsync(
            PrincipalType principalType,
            Guid principalId,
            ResourceType resourceType,
            Guid resourceId,
            List<Permission> permissions)
        {
            var perm = new PrincipalPermission
            {
                PrincipalType = principalType,
                PrincipalId = principalId,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Permissions = permissions.ExpandPermissions()
            };
            await permissionRepository.AddOrUpdatePermissionsAsync(perm);
        }

        public async Task RemovePermissionsAsync(
            PrincipalType principalType,
            Guid principalId,
            ResourceType resourceType,
            Guid resourceId)
        {
            var principalPerms = await permissionRepository.GetPermissionsAsync(
                principalType,
                principalId,
                resourceType,
                resourceId);

            if (principalPerms == null)
                throw new KeyNotFoundException("Permissions not found for principal and resource");

            await permissionRepository.RemoveAsync(principalPerms);
        }

        public async Task RemovePrincipalsFromResourceAsync(ResourceType resourceType, Guid resourceId)
        {
            var permissions = await permissionRepository.GetPermissionsByResourceAsync(resourceType, resourceId);
            await permissionRepository.RemoveRangeAsync(permissions);
        }

        public async Task<List<PrincipalPermission>> GetPrincipalsForUserAsync(
            Guid userId,
            ResourceType resourceType)
        {
            var userPerms = await permissionRepository.GetPermissionsByPrincipalAsync(PrincipalType.User, userId);

            return userPerms
                .Where(p => p.ResourceType == resourceType)
                .ToList();
        }

        public async Task<List<PrincipalPermission>> GetPrincipalsForTeamAsync(Guid teamId, ResourceType resourceType)
        {
            throw new NotImplementedException();
        }

        public async Task<List<PrincipalPermission>> GetPrincipalsForResourceAsync(ResourceType resourceType, Guid resourceId)
        {
            return await permissionRepository.GetPermissionsByResourceAsync(resourceType, resourceId);
        }
    }
}
