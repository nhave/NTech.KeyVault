using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Services
{
    public interface IPermissionService
    {
        Task<List<Permission>> GetPermissionsAsync(
            Guid userId,
            IEnumerable<Guid> teamIds,
            ResourceType resourceType,
            Guid resourceId);

        Task<bool> HasPermissionAsync(
            Guid userId,
            IEnumerable<Guid> teamIds,
            ResourceType resourceType,
            Guid resourceId,
            Permission required);
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

            all.AddRange(userPerms);

            // Team permissions
            foreach (var teamId in teamIds)
            {
                var teamPerms = await permissionRepository.GetPermissionsAsync(
                    PrincipalType.Team,
                    teamId,
                    resourceType,
                    resourceId);

                all.AddRange(teamPerms);
            }

            // Flad liste af enum-permissions
            return all
                .SelectMany(p => p.Permissions)
                .Distinct()
                .ToList();
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
    }
}
