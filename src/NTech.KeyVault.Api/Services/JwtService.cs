

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
        /// <summary>
        /// Asynchronously generates a new refresh token and its corresponding hash for use in authentication workflows.
        /// </summary>
        /// <remarks>The generated refresh token can be provided to clients for subsequent authentication
        /// requests, while the hash should be stored securely for token validation purposes.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the generated
        /// refresh token as a string and its hash as a string.</returns>
        public Task<(string token, string hash)> GenerateRefreshTokenAsync();

        /// <summary>
        /// Validates the specified refresh token and returns the associated user if the token is valid.
        /// </summary>
        /// <param name="refreshToken">The refresh token to validate. Cannot be null or empty.</param>
        /// <param name="ipAddress">The IP address from which the validation request originates. Used for additional security checks.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user associated with the
        /// valid refresh token, or null if the token is invalid or expired.</returns>
        public Task<User?> ValidateRefreshTokenAsync(string refreshToken, string ipAddress);

        /// <summary>
        /// Revokes the specified refresh token and optionally associates a new token hash with the request.
        /// </summary>
        /// <remarks>This method is typically used during logout or when rotating refresh tokens to ensure
        /// that the specified token can no longer be used. The operation may be logged for security auditing.</remarks>
        /// <param name="refreshToken">The refresh token to be revoked. Cannot be null or empty.</param>
        /// <param name="ipAddress">The IP address from which the revocation request originated. Used for auditing and security purposes. Cannot
        /// be null or empty.</param>
        /// <param name="newTokenHash">The hash of the new refresh token to associate with the revocation, or null if no new token is issued.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string newTokenHash);

        /// <summary>
        /// Asynchronously creates a login response containing authentication tokens and user information for a
        /// successful authentication attempt.
        /// </summary>
        /// <param name="user">The user entity representing the authenticated user. Cannot be null.</param>
        /// <param name="userInfo">Additional information about the user to include in the response. Cannot be null.</param>
        /// <param name="refreshData">A tuple containing the refresh token and its associated hash to be included in the response.</param>
        /// <param name="ipAddress">The IP address from which the authentication request originated. Used for logging or security purposes.
        /// Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a LoginResponse object with
        /// authentication tokens and user details.</returns>
        public Task<LoginResponse> CreateAuthResponseAsync(User user, UserInfo userInfo, (string token, string hash) refreshData, string ipAddress);
    }

    public class JwtService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, IEncryptionService encryptionService) : IJwtService
    {
        /// <summary>
        /// Generates a JSON Web Token (JWT) containing user identity and role claims for authentication purposes.
        /// </summary>
        /// <remarks>The generated token includes claims for the user's ID, full name, email, and roles,
        /// and is valid for 30 minutes from the time of creation. The token is signed using the HMAC SHA-256 algorithm
        /// and configuration values for issuer, audience, and secret key.</remarks>
        /// <param name="user">The user whose unique identifier and roles are included as claims in the generated token. Cannot be null.</param>
        /// <param name="userInfo">Additional user information, such as full name and email, to be included as claims in the token. Cannot be
        /// null.</param>
        /// <returns>A string representation of the generated JWT, which can be used for authenticating the user in subsequent
        /// requests.</returns>
        private string GenerateJwtToken(User user, UserInfo userInfo)
        {
            var Jwt = configuration.GetSection("Jwt");
            var Issuer = Jwt["Issuer"];
            var Audience = Jwt["Audience"];
            var Secret = Jwt["Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create claims for the token, including user ID, full name, email, and roles
            var Claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("FullName", userInfo.FullName),
                new("Email", userInfo.Email)
            };

            // Add a claim for each role the user has
            foreach (var role in user.Roles)
            {
                Claims.Add(new(ClaimTypes.Role, role.ToString()));
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
