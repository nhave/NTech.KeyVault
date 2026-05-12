using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.ClientServices.Abstractions;
using NTech.KeyVault.ClientServices.Models;
using NTech.KeyVault.ClientServices.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NTech.KeyVault.Frontend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(LoginService loginService, ITokenStore tokenStore) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult<LoginUiResponse>> Login(LoginRequest dto)
        {
            //var response = await loginService.LoginAsync;
            //string responseContent = await response.Content.ReadAsStringAsync();
            //if (!response.IsSuccessStatusCode) return Ok(new LoginUiResponse(false, false, responseContent));

            var payload = await loginService.LoginAsync(dto.Username, dto.Password, dto.MfaMethod, dto.MfaCode);
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
                return Ok(new LoginUiResponse(false, false));

            await HttpContext.SignInAsync(principal, authProperties);

            return Ok(new LoginUiResponse(true, false));
        }

        [HttpPost("Refresh"), Authorize]
        public async Task<ActionResult> Refresh()
        {
            var cookieId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (cookieId == null) return StatusCode(500);

            var model = await tokenStore.GetAsync<AuthTokenModel>(cookieId);
            if (string.IsNullOrWhiteSpace(model?.RefreshToken)) return BadRequest();

            var payload = await loginService.RefreshAsync(model.RefreshToken);
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

            return NoContent();
        }

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
