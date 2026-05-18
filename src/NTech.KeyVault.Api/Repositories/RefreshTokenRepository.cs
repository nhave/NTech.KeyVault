using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IRefreshTokenRepository
    {
        public Task AddRefreshTokenAsync(RefreshToken refreshToken);
        public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
        public Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string newTokenHash);
        public Task DeleteExpiredTokensAsync();
    }

    public class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
    {
        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await dbContext.RefreshTokens.AddAsync(refreshToken);
            await dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
        {
            return await dbContext.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string newTokenHash)
        {
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByTokenHash = newTokenHash;

                dbContext.RefreshTokens.Update(refreshToken);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var expiredTokens = dbContext.RefreshTokens.Where(rt => rt.ExpiryDate <= DateTime.UtcNow);
            dbContext.RefreshTokens.RemoveRange(expiredTokens);
            await dbContext.SaveChangesAsync();
        }
    }
}
