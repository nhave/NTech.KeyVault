using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using NTech.KeyVault.Frontend.Models;
using NTech.KeyVault.Frontend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NTech.KeyVault.Frontend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(IHttpClientFactory clientFactory, ITokenStore tokenStore) : ControllerBase
    {
        private readonly HttpClient _client = clientFactory.CreateClient("Api");

        /// <summary>
        /// Authenticates a user with the provided credentials and initiates a login session.
        /// </summary>
        /// <remarks>If multi-factor authentication is required, the response will indicate that
        /// additional verification is needed before completing the login process. The method establishes a persistent
        /// authentication session upon successful login. The session is associated with a unique identifier and tokens
        /// are securely stored for future use.</remarks>
        /// <param name="dto">The login request containing user credentials and any additional authentication data required for login.</param>
        /// <returns>An ActionResult containing a LoginResponse that indicates the result of the login attempt. If multi-factor
        /// authentication is required, the response will indicate this. If authentication is successful, a session is
        /// established and the user is signed in.</returns>
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
        {
            var response = await _client.PostAsJsonAsync("Auth/Login", dto);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return Ok(new LoginUiResponse(false, false, responseContent));

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (payload == null) return Ok(new LoginUiResponse(false, false, "Login has failed."));
            if (payload.IsMfaNeeded) return Ok(new LoginUiResponse(false, true));

            // Create claims from the JWT token and sign in the user with cookie authentication.
            var claimsIdentity = new ClaimsIdentity(ParseClaimsFromJwt(payload.JwtToken!), CookieAuthenticationDefaults.AuthenticationScheme);

            // Generate a unique identifier for the session and store it in the claims. This will be used to associate the cookie with the stored tokens.
            var entityId = Guid.NewGuid().ToString();
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Sid, entityId));

            var principal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            if (!await tokenStore.SetAsync(entityId, new AuthTokenModel
            {
                JwtToken = payload.JwtToken!,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                RefreshToken = payload.RefreshToken
            }))
                return Ok(new LoginUiResponse(false, false, responseContent));

            await HttpContext.SignInAsync(principal, authProperties);

            return Ok(new LoginUiResponse(true, false));
        }

        /// <summary>
        /// Attempts to refresh the authentication token for the current user session using the stored refresh token.
        /// </summary>
        /// <remarks>This endpoint requires the user to be authenticated. The method retrieves the refresh
        /// token associated with the current session and attempts to obtain a new JWT token. If the refresh token is
        /// missing or invalid, the request fails. The authentication cookie is updated upon successful
        /// refresh.</remarks>
        /// <returns>An <see cref="ActionResult"/> indicating the result of the refresh operation. Returns <see cref="OkResult"/>
        /// if the token is successfully refreshed; otherwise, returns <see cref="BadRequestResult"/> or <see
        /// cref="StatusCodeResult"/> if the operation fails.</returns>
        [HttpPost("Refresh"), Authorize]
        public async Task<ActionResult> Refresh()
        {
            var cookieId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (cookieId == null) return StatusCode(500);

            var model = await tokenStore.GetAsync<AuthTokenModel>(cookieId);
            if (string.IsNullOrWhiteSpace(model?.RefreshToken)) return BadRequest();

            var response = await _client.PostAsJsonAsync($"/Auth/Refresh", new RefreshTokenRequest(model.RefreshToken));
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (payload == null) return StatusCode(500);

                var token = payload.JwtToken;
                var newExpires = DateTime.UtcNow.AddSeconds(payload.ExpiresIn);
                var newRefresh = payload.RefreshToken;

                await tokenStore.SetAsync(cookieId, new AuthTokenModel
                {
                    JwtToken = token,
                    ExpiresAt = newExpires,
                    RefreshToken = newRefresh
                });

                var claimsIdentity = new ClaimsIdentity(ParseClaimsFromJwt(payload.JwtToken), CookieAuthenticationDefaults.AuthenticationScheme);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Sid, cookieId));
                var principal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(principal, authProperties);

                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// Signs out the current user and redirects to the application's home page.
        /// </summary>
        /// <remarks>This method clears the user's authentication session and any associated tokens before
        /// redirecting.</remarks>
        /// <returns>A redirect result that sends the user to the root URL after sign-out.</returns>
        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            var cookieId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (cookieId != null)
                await tokenStore.ClearAsync(cookieId);

            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        /// <summary>
        /// Parses a JSON Web Token (JWT) and returns the collection of claims contained within the token.
        /// </summary>
        /// <param name="jwt">The JWT string to parse. Must be a valid, well-formed JWT.</param>
        /// <returns>An enumerable collection of claims extracted from the specified JWT. Returns an empty collection if the
        /// token contains no claims.</returns>
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
            return jsonToken?.Claims!;
        }
    }
}
