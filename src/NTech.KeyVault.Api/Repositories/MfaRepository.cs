using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IMfaRepository
    {
        public Task<UserMfaMethod?> GetMfaMethodAsync(Guid userId, MfaMethodType methodType);
        public Task AddMfaMethodAsync(UserMfaMethod mfaMethod);
        public Task UpdateMfaMethodAsync(UserMfaMethod mfaMethod);
        public Task DeleteMfaMethodAsync(UserMfaMethod mfaMethod);
        public Task<List<UserMfaMethod>> GetActiveMfaMethodsAsync(Guid userId);
    }

    public class MfaRepository(AppDbContext dbContext) : IMfaRepository
    {
        public async Task<List<UserMfaMethod>> GetActiveMfaMethodsAsync(Guid userId)
        {
            return await dbContext.UserMfaMethods
                .Where(m => m.UserId == userId && m.IsEnabled)
                .ToListAsync();
        }

        public async Task<UserMfaMethod?> GetMfaMethodAsync(Guid userId, MfaMethodType methodType)
        {
            return await dbContext.UserMfaMethods
                .FirstOrDefaultAsync(m => m.UserId == userId && m.Method == methodType);
        }

        public async Task AddMfaMethodAsync(UserMfaMethod mfaMethod)
        {
            dbContext.UserMfaMethods.Add(mfaMethod);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateMfaMethodAsync(UserMfaMethod mfaMethod)
        {
            dbContext.UserMfaMethods.Update(mfaMethod);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteMfaMethodAsync(UserMfaMethod mfaMethod)
        {
            dbContext.UserMfaMethods.Remove(mfaMethod);
            await dbContext.SaveChangesAsync();
        }
    }
}
