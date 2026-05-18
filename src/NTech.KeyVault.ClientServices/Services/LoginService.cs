using NTech.KeyVault.ClientServices.Abstractions;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Net.Http.Json;

namespace NTech.KeyVault.ClientServices.Services
{
    public sealed class LoginService
    {
        private readonly HttpClient _auth;
        private readonly IHostProvider _hostProvider;

        public LoginService(IHttpClientFactory factory, IHostProvider hostProvider)
        {
            _auth = factory.CreateClient("Auth");
            _hostProvider = hostProvider;
        }

        /// <summary>
        /// Authenticates a user asynchronously using the provided credentials and optional multi-factor authentication
        /// (MFA) information.
        /// </summary>
        /// <remarks>If MFA is required for the account, both MfaMethod and MfaCode must be provided. The
        /// method returns null if authentication fails due to invalid credentials or missing/incorrect MFA
        /// information.</remarks>
        /// <param name="username">The username of the account to authenticate. Cannot be null or empty.</param>
        /// <param name="password">The password associated with the specified username. Cannot be null or empty.</param>
        /// <param name="MfaMethod">The MFA method to use for authentication, or null if MFA is not required.</param>
        /// <param name="MfaCode">The MFA code to use for authentication, or null if MFA is not required.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a LoginResponse object if
        /// authentication is successful; otherwise, null.</returns>
        public async Task<LoginResponse?> LoginAsync(string username, string password, MfaMethodType? MfaMethod, string? MfaCode)
        {
            var uri = await _hostProvider.BuildUri("/auth/login");
            var resp = await _auth.PostAsJsonAsync(uri,
                new LoginRequest
                {
                    Username = username,
                    Password = password,
                    MfaMethod = MfaMethod,
                    MfaCode = MfaCode
                });

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponse>();
        }

        /// <summary>
        /// Requests a new access token using the specified refresh token.
        /// </summary>
        /// <remarks>Returns <see langword="null"/> if the refresh token is invalid or the server response
        /// indicates failure.</remarks>
        /// <param name="refreshToken">The refresh token to use for obtaining a new access token. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="LoginResponse"/>
        /// with the new access token if the refresh is successful; otherwise, <see langword="null"/>.</returns>
        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            var uri = await _hostProvider.BuildUri("/auth/refresh");
            var resp = await _auth.PostAsJsonAsync(uri,
                new RefreshTokenRequest(refreshToken));

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<LoginResponse>();
        }
    }
}
