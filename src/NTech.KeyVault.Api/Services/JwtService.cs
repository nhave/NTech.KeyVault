

using Microsoft.IdentityModel.Tokens;
using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NTech.KeyVault.Api.Services
{
    public interface IJwtService
    {
        public Task<(string token, string hash)> GenerateRefreshTokenAsync();
        public Task<User?> ValidateRefreshTokenAsync(string refreshToken, string ipAddress);
        public Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string newTokenHash);
        public Task<LoginResponse> CreateAuthResponseAsync(User user, UserInfo userInfo, (string token, string hash) refreshData, string ipAddress);
    }

    public class JwtService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, IEncryptionService encryptionService) : IJwtService
    {
        private string GenerateJwtToken(User user, UserInfo userInfo)
        {
            var Jwt = configuration.GetSection("Jwt");
            var Issuer = Jwt["Issuer"];
            var Audience = Jwt["Audience"];
            var Secret = Jwt["Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var Claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("FullName", userInfo.FullName),
                new Claim("Email", userInfo.Email)
            };

            foreach (var userRole in user.UserRoles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, userRole.Role.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: Claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User?> ValidateRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            // Check if the refresh token is provided
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var hash = encryptionService.CreateLookupHash(refreshToken);
            var token = await refreshTokenRepository.GetRefreshTokenAsync(hash);

            if (token == null || !token.IsActive) return null;

            return token.User;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string newTokenHash)
        {
            var hash = encryptionService.CreateLookupHash(refreshToken);
            var token = await refreshTokenRepository.GetRefreshTokenAsync(hash);
            if (token != null)
            {
                await refreshTokenRepository.RevokeRefreshTokenAsync(token, ipAddress, newTokenHash);
            }
        }

        public async Task<(string token, string hash)> GenerateRefreshTokenAsync()
        {
            while (true)
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                var token = Convert.ToBase64String(bytes);
                var hash = encryptionService.CreateLookupHash(token);

                if (await refreshTokenRepository.GetRefreshTokenAsync(hash) == null)
                    return (token, hash);
            }
        }

        public async Task<LoginResponse> CreateAuthResponseAsync(User user, UserInfo userInfo, (string token, string hash) refreshData, string ipAddress)
        {
            var jwtToken = GenerateJwtToken(user, userInfo);
            int expiry = 1800;

            // Store refresh token in database
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                User = user,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                TokenHash = refreshData.hash,
                CreatedByIp = ipAddress
            };
            await refreshTokenRepository.AddRefreshTokenAsync(refreshToken);

            // Return authentication response
            return new LoginResponse(jwtToken, expiry, refreshData.token);
        }
    }
}
