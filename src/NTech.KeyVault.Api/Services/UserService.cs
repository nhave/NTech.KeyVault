using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;
using System.Security.Claims;

namespace NTech.KeyVault.Api.Services
{
    public interface IUserService
    {
        public Task<User?> GetUserById(string id, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<User?> GetUserByEmail(string email, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<User?> GetUserByEmailOrUsername(string emailOrUsername, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task UpdateUserAsync(User user);
        public UserInfo? GetUserInfo(User user);
        public Task StoreUserInfo(User user, UserInfo userInfo);
        public string CreateLookupHash(string normalizedValue);
        public Task<User?> GetCurrentUserAsync(Func<IQueryable<User>, IQueryable<User>>? include = null);
    }

    public class UserService(IUserRepository userRepository, IEncryptionService encryptionService, IHttpContextAccessor httpContextAccessor) : IUserService
    {
        public async Task<User?> GetUserById(string id, Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            return await userRepository.GetById(id, include);
        }

        public async Task<User?> GetUserByEmail(string email, Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            return await userRepository.GetByEmail(email, include);
        }

        public async Task<User?> GetUserByEmailOrUsername(string emailOrUsername, Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            var user = await userRepository.GetByUsername(emailOrUsername, include);
            if (user == null)
            {
                var lookupHash = CreateLookupHash(emailOrUsername.ToLowerInvariant());
                user = await userRepository.GetByEmail(lookupHash, include);
            }
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            await userRepository.Update(user);
        }

        public UserInfo? GetUserInfo(User user)
        {
            var decrypted = encryptionService.Decrypt(
                user.EncryptedUserInfo,
                user.EncryptedDataKey,
                user.UserInfoNonce,
                user.DataKeyNonce
            );
            return UserInfo.Deserialize(decrypted.Value);
        }

        public async Task StoreUserInfo(User user, UserInfo userInfo)
        {
            var serialized = userInfo.Serialize();
            var encrypted = encryptionService.Encrypt(serialized);

            user.EncryptedUserInfo = encrypted.EncryptedValue;
            user.EncryptedDataKey = encrypted.EncryptedDataKey;
            user.UserInfoNonce = encrypted.ValueNonce;
            user.DataKeyNonce = encrypted.DataKeyNonce;

            await userRepository.Update(user);
        }

        public string CreateLookupHash(string normalizedValue)
        {
            return encryptionService.CreateLookupHash(normalizedValue);
        }

        public async Task<User?> GetCurrentUserAsync(Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            var contextUser = httpContextAccessor.HttpContext?.User;

            if (contextUser?.Identity != null && contextUser.Identity.IsAuthenticated)
            {
                var userId = contextUser.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                var user = await GetUserById(userId, include);

                return user;
            }

            return null;
        }
    }
}
