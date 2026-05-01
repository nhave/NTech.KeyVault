using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Services
{
    public interface IAuthService
    {
        public Task<LoginResponse> LoginAsync(LoginRequest dto);
        public Task<LoginResponse> RefreshAsync(RefreshTokenRequest dto);
        public Task ChangePassword(PasswordChangeRequest dto);
        public Task ChangeEmail(EmailChangeRequest dto);
        public Task<MeResponse> GetMeResponseAsync();
    }

    public class AuthService(IUserService userService, IJwtService jwtService, IHttpContextAccessor httpContextAccessor) : IAuthService
    {
        public async Task<LoginResponse> LoginAsync(LoginRequest dto)
        {
            var user = await userService.GetUserByEmailOrUsername(dto.Username, q => q.Include(u => u.UserRoles));
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (user.IsDisabled)
                throw new UnauthorizedAccessException("User account is disabled.");

            var userInfo = userService.GetUserInfo(user);
            if (userInfo == null)
                throw new Exception("Failed to fetch User information.");

            var ip = GetIpAddress();

            var refreshData = await jwtService.GenerateRefreshTokenAsync();

            return await jwtService.CreateAuthResponseAsync(user, userInfo, refreshData, ip);
        }

        public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest dto)
        {
            var ip = GetIpAddress();

            var user = await jwtService.ValidateRefreshTokenAsync(dto.RefreshToken, ip);
            if (user == null) throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            if (user.IsDisabled)
                throw new UnauthorizedAccessException("User account is disabled.");

            var userInfo = userService.GetUserInfo(user);
            if (userInfo == null)
                throw new Exception("Failed to fetch User information.");

            var refreshData = await jwtService.GenerateRefreshTokenAsync();

            var response = await jwtService.CreateAuthResponseAsync(user, userInfo, refreshData, ip);
            await jwtService.RevokeRefreshTokenAsync(dto.RefreshToken, ip, refreshData.hash);

            return response;
        }

        public async Task ChangePassword(PasswordChangeRequest dto)
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("Failed to get signed in user.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await userService.UpdateUserAsync(user);
        }

        public async Task ChangeEmail(EmailChangeRequest dto)
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("Failed to get signed in user.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            var lookupHash = userService.CreateLookupHash(dto.NewEmail.ToLowerInvariant());
            if (await userService.GetUserByEmail(lookupHash) != null)
                throw new UnauthorizedAccessException("Email is already in use.");

            user.EmailLookupHash = lookupHash;
            await userService.UpdateUserAsync(user);
        }

        public async Task<MeResponse> GetMeResponseAsync()
        {
            var user = await userService.GetCurrentUserAsync(q => q.Include(u => u.UserRoles));
            if (user == null)
                throw new Exception("Failed to get signed in user.");

            var userInfo = userService.GetUserInfo(user);
            if (userInfo == null)
                throw new Exception("Failed to fetch User information.");

            return new MeResponse(
                user.Id.ToString(),
                userInfo.FullName,
                userInfo.Email,
                user.Roles.Select(r => r.ToString()).ToList());
        }

        private string GetIpAddress()
        {
            return httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                   ?? "unknown";
        }
    }
}
