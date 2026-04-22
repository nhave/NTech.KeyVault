using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IPrincipalPermissionRepository
    {
        Task<List<PrincipalPermission>> GetPermissionsAsync(
            PrincipalType principalType,
            Guid principalId,
            ResourceType resourceType,
            Guid resourceId);
    }

    public class PrincipalPermissionRepository(AppDbContext dbContext) : IPrincipalPermissionRepository
    {
        public Task<List<PrincipalPermission>> GetPermissionsAsync(
        PrincipalType principalType,
        Guid principalId,
        ResourceType resourceType,
        Guid resourceId)
        {
            //return dbContext.PrincipalPermissions
            //    .Where(p =>
            //        p.PrincipalType == principalType &&
            //        p.PrincipalId == principalId &&
            //        p.ResourceType == resourceType &&
            //        p.ResourceId == resourceId)
            //    .ToListAsync();

            throw new NotImplementedException();
        }
    }
}
