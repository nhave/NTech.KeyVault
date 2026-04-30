using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IPrincipalPermissionRepository
    {
        Task<PrincipalPermission?> GetPermissionsAsync(
            PrincipalType principalType,
            Guid principalId,
            ResourceType resourceType,
            Guid resourceId);
        public Task AddOrUpdatePermissionsAsync(PrincipalPermission permission);
        public Task RemoveAsync(PrincipalPermission permission);
        public Task RemoveRangeAsync(List<PrincipalPermission> permissions);
        public Task<List<PrincipalPermission>> GetPermissionsByResourceAsync(
            ResourceType resourceType,
            Guid resourceId);
        public Task<List<PrincipalPermission>> GetPermissionsByPrincipalAsync(
            PrincipalType principalType,
            Guid principalId);
    }

    public class PrincipalPermissionRepository(AppDbContext dbContext) : IPrincipalPermissionRepository
    {
        public Task<PrincipalPermission?> GetPermissionsAsync(
        PrincipalType principalType,
        Guid principalId,
        ResourceType resourceType,
        Guid resourceId)
        {
            return dbContext.PrincipalPermissions
                .FirstOrDefaultAsync(p =>
                    p.PrincipalType == principalType &&
                    p.PrincipalId == principalId &&
                    p.ResourceType == resourceType &&
                    p.ResourceId == resourceId);
        }

        public async Task AddOrUpdatePermissionsAsync(PrincipalPermission permission)
        {
            var existing = dbContext.PrincipalPermissions
                .FirstOrDefault(p =>
                    p.PrincipalType == permission.PrincipalType &&
                    p.PrincipalId == permission.PrincipalId &&
                    p.ResourceType == permission.ResourceType &&
                    p.ResourceId == permission.ResourceId);

            if (existing != null)
            {
                existing.Permissions = permission.Permissions;
                dbContext.PrincipalPermissions.Update(existing);
            }
            else
            {
                dbContext.PrincipalPermissions.Add(permission);
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveAsync(PrincipalPermission permission)
        {
            dbContext.PrincipalPermissions.Remove(permission);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(List<PrincipalPermission> permissions)
        {
            if (permissions == null || !permissions.Any())
            {
                return;
            }

            dbContext.PrincipalPermissions.RemoveRange(permissions);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<PrincipalPermission>> GetPermissionsByResourceAsync(
            ResourceType resourceType,
            Guid resourceId)
        {
            return await dbContext.PrincipalPermissions
                .Where(p =>
                    p.ResourceType == resourceType &&
                    p.ResourceId == resourceId)
                .ToListAsync();
        }

        public async Task<List<PrincipalPermission>> GetPermissionsByPrincipalAsync(
            PrincipalType principalType,
            Guid principalId)
        {
            return await dbContext.PrincipalPermissions
                .Where(p =>
                    p.PrincipalType == principalType &&
                    p.PrincipalId == principalId)
                .ToListAsync();
        }
    }
}
