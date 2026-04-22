using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(IAuthService authService, IMfaService mfaService) : ControllerBase
    {
        /// <summary>
        /// Authenticates a user with the provided credentials and returns a JWT token if authentication is successful.
        /// </summary>
        /// <param name="dto">The login request containing user credentials. Cannot be null.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="LoginResponse"/> with the JWT token if
        /// authentication succeeds; returns 401 Unauthorized if credentials are invalid, or 500 Internal Server Error
        /// for unexpected failures.</returns>
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
        {
            try
            {
                var jwt = await authService.LoginAsync(dto);
                return Ok(jwt);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Generates a new access token and refresh token pair using the provided refresh token request.
        /// </summary>
        /// <param name="dto">The refresh token request containing the current refresh token and related authentication information.
        /// Cannot be null.</param>
        /// <returns>An ActionResult containing a LoginResponse with the new access and refresh tokens if the refresh is
        /// successful; returns Unauthorized if the request is invalid, or a 500 status code if an unexpected error
        /// occurs.</returns>
        [HttpPost("Refresh")]
        public async Task<ActionResult<LoginResponse>> Refresh(RefreshTokenRequest dto)
        {
            try
            {
                var jwt = await authService.RefreshAsync(dto);
                return Ok(jwt);
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires authentication. The response includes details about the
        /// authenticated user as determined by the current access token.</remarks>
        /// <returns>An <see cref="ActionResult{MeResponse}"/> containing the user's profile information if authentication is
        /// successful; otherwise, a 500 Internal Server Error response.</returns>
        [HttpGet("Me"), Authorize]
        public async Task<ActionResult<MeResponse>> Me()
        {
            try
            {
                return Ok(await authService.GetMeResponseAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <remarks>This action requires authentication. The request will fail if the provided password
        /// information is invalid or if an error occurs during the password change process.</remarks>
        /// <param name="dto">An object containing the current and new password information required to perform the password change.</param>
        /// <returns>An HTTP 204 No Content response if the password was changed successfully; otherwise, an appropriate error
        /// response.</returns>
        [HttpPost("ChangePassword"), Authorize]
        public async Task<ActionResult> ChangePassword(PasswordChangeRequest dto)
        {
            try
            {
                await authService.ChangePassword(dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Handles a request to change the authenticated user's email address.
        /// </summary>
        /// <remarks>This action requires authentication. The request may fail if the provided email
        /// address is invalid or does not meet application requirements.</remarks>
        /// <param name="dto">An object containing the new email address and any required verification information.</param>
        /// <returns>An HTTP 204 No Content response if the email was changed successfully; otherwise, an appropriate error
        /// response.</returns>
        [HttpPost("ChangeEmail"), Authorize]
        public async Task<ActionResult> ChangeEmail(EmailChangeRequest dto)
        {
            try
            {
                await authService.ChangeEmail(dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
