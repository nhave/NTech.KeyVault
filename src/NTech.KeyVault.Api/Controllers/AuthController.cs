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
