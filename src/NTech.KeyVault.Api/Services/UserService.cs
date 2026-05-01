using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;
using System.Drawing;
using System.Security.Claims;

namespace NTech.KeyVault.Api.Services
{
    public interface IUserService
    {
        public Task<User> CreateUserAsync(string username, string fullName, string email, string password);
        public Task<List<User>> GetUsersAsync(int page, int pageSize);
        public Task<User?> GetUserById(string id, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<User?> GetUserByEmail(string email, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<User?> GetUserByEmailOrUsername(string emailOrUsername, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task UpdateUserAsync(User user);
        public UserInfo? GetUserInfo(User user);
        public Task StoreUserInfo(User user, UserInfo userInfo);
        public string CreateLookupHash(string normalizedValue);
        public Task AddRolesAsync(string userId, List<Roles> roles);
        public Task RemoveRolesAsync(string userId, List<Roles> roles);
        public Task DisableUserAsync(string userId);
        public Task EnableUserAsync(string userId);
        public Task DeleteUserAsync(string userId);
        public Task<User> GetCurrentUserAsync(Func<IQueryable<User>, IQueryable<User>>? include = null);
    }

    public class UserService(IUserRepository userRepository, IEncryptionService encryptionService, IHttpContextAccessor httpContextAccessor) : IUserService
    {
        public async Task<User> CreateUserAsync(string username, string fullName, string email, string password)
        {
            if (await userRepository.GetByUsername(username) != null)
                throw new InvalidOperationException("Username is already taken.");

            if (await userRepository.GetByEmail(email) != null)
                throw new InvalidOperationException("Email is already taken.");

            var lookupHash = CreateLookupHash(email.ToLowerInvariant());
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var userInfo = new UserInfo
            {
                FullName = fullName,
                Email = email
            };

            var serialized = userInfo.Serialize();
            var encrypted = encryptionService.Encrypt(serialized);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                EmailLookupHash = lookupHash,
                PasswordHash = passwordHash,
                EncryptedUserInfo = encrypted.EncryptedValue,
                EncryptedDataKey = encrypted.EncryptedDataKey,
                UserInfoNonce = encrypted.ValueNonce,
                DataKeyNonce = encrypted.DataKeyNonce
            };

            await userRepository.AddUser(user);

            return user;
        }

        public async Task<List<User>> GetUsersAsync(int page, int pageSize)
        {
            var currentUser = await GetCurrentUserAsync(q => q.Include(u => u.UserRoles));

            var roles = currentUser.Roles;
            bool isSystemAdmin = roles.Contains(Roles.SystemAdmin);

            Func<IQueryable<User>, IQueryable<User>>? query = q => 
                q.Include(u => u.UserRoles).Where(u => u.Id != currentUser.Id)
                    .Where(u => isSystemAdmin ||
                    !u.UserRoles.Any(ur => ur.Role == Roles.SystemAdmin || ur.Role == Roles.Admin));

            return await userRepository.GetUsersAsync(page, pageSize, query);
        }

        public async Task<User?> GetUserById(string id, Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            return await userRepository.GetById(id, include);
        }

        public async Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds, Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            return await userRepository.GetByIdsAsync(userIds, include);
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

        public async Task AddRolesAsync(string userId, List<Roles> roles)
        {
            var currentUser = await GetCurrentUserAsync(q => q.Include(u => u.UserRoles));

            var user = await GetUserById(userId, q => q.Include(u => u.UserRoles))
                ?? throw new KeyNotFoundException("User not found.");

            if (currentUser.Id == user.Id)
                throw new InvalidOperationException("Users cannot modify their own roles.");

            if (roles.Contains(Roles.SystemAdmin))
                throw new InvalidOperationException("Cannot assign SystemAdmin role through this method.");

            if (roles.Contains(Roles.User))
                throw new InvalidOperationException("User role is assigned by default and cannot be added through this method.");

            if (!currentUser.Roles.Contains(Roles.SystemAdmin) && roles.Contains(Roles.Admin))
                throw new InvalidOperationException("Only SystemAdmin can assign Admin role.");

            if (user.UserRoles.Any(ur => roles.Contains(ur.Role)))
                throw new InvalidOperationException("User already has one or more of the specified roles.");

            List<UserRole> userRoles = roles.Select(r => new UserRole
            {
                UserId = user.Id,
                Role = r
            }).ToList();

            user.UserRoles.AddRange(userRoles);
            await userRepository.Update(user);
        }

        public async Task RemoveRolesAsync(string userId, List<Roles> roles)
        {
            var currentUser = await GetCurrentUserAsync(q => q.Include(u => u.UserRoles));

            var user = await GetUserById(userId, q => q.Include(u => u.UserRoles))
                ?? throw new KeyNotFoundException("User not found.");

            if (currentUser.Id == user.Id)
                throw new InvalidOperationException("Users cannot modify their own roles.");

            if (roles.Contains(Roles.SystemAdmin))
                throw new InvalidOperationException("Cannot remove SystemAdmin role through this method.");

            if (roles.Contains(Roles.User))
                throw new InvalidOperationException("User role is assigned by default and cannot be removed through this method.");

            if (!currentUser.Roles.Contains(Roles.SystemAdmin) && roles.Contains(Roles.Admin))
                throw new InvalidOperationException("Only SystemAdmin can remove Admin role.");

            var userRolesToRemove = user.UserRoles.Where(ur => roles.Contains(ur.Role)).ToList();
            if (userRolesToRemove.Count == 0)
                throw new InvalidOperationException("User does not have one or more of the specified roles.");

            foreach (var userRole in userRolesToRemove)
            {
                user.UserRoles.Remove(userRole);
            }
            await userRepository.Update(user);
        }

        public async Task DisableUserAsync(string userId)
        {
            await SetDisabledAsync(userId, true);
        }

        public async Task EnableUserAsync(string userId)
        {
            await SetDisabledAsync(userId, false);
        }

        private async Task SetDisabledAsync(string userId, bool isDisabled)
        {
            var currentUser = await GetCurrentUserAsync(q => q.Include(u => u.UserRoles));

            var user = await GetUserById(userId, q => q.Include(u => u.UserRoles))
                ?? throw new KeyNotFoundException("User not found.");

            if (currentUser.Id == user.Id)
                throw new InvalidOperationException("Users cannot modify their own account status.");

            if (user.Roles.Contains(Roles.SystemAdmin))
                throw new InvalidOperationException("SystemAdmin accounts cannot be disabled.");

            if (!currentUser.Roles.Contains(Roles.SystemAdmin) && user.Roles.Contains(Roles.Admin))
                throw new InvalidOperationException("Only SystemAdmin can modify Admin account status.");

            if (user.IsDisabled == isDisabled)
                throw new InvalidOperationException($"User is already {(isDisabled ? "disabled" : "enabled")}.");

            user.IsDisabled = isDisabled;
            await userRepository.Update(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var currentUser = await GetCurrentUserAsync(q => q.Include(u => u.UserRoles));

            var user = await GetUserById(userId, q => q.Include(u => u.UserRoles))
                ?? throw new KeyNotFoundException("User not found.");

            if (currentUser.Id == user.Id)
                throw new InvalidOperationException("Users cannot delete their own account.");

            if (user.Roles.Contains(Roles.SystemAdmin))
                throw new InvalidOperationException("SystemAdmin accounts cannot be deleted.");

            if (!currentUser.Roles.Contains(Roles.SystemAdmin) && user.Roles.Contains(Roles.Admin))
                throw new InvalidOperationException("Only SystemAdmin can delete Admin accounts.");

            if (!user.IsDisabled)
                throw new InvalidOperationException("User account must be disabled before it can be deleted.");

            await userRepository.Delete(user);
        }

        public string CreateLookupHash(string normalizedValue)
        {
            return encryptionService.CreateLookupHash(normalizedValue);
        }

        public async Task<User> GetCurrentUserAsync(Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            var contextUser = httpContextAccessor.HttpContext?.User;

            if (contextUser?.Identity != null && contextUser.Identity.IsAuthenticated)
            {
                var userId = contextUser.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("Failed to get signed in user.");
                }

                var user = await GetUserById(userId, include) ?? throw new Exception("Failed to get signed in user.");
                if (user != null && user.IsDisabled && !user.Roles.Contains(Roles.SystemAdmin))
                {
                    throw new UnauthorizedAccessException("User account is disabled.");
                }

                return user!;
            }

            throw new Exception("Failed to get signed in user.");
        }
    }
}
