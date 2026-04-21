using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MfaController(IAuthService authService, IMfaService mfaService) : ControllerBase
    {
        [HttpPost("EnableMFA"), Authorize]
        public async Task<ActionResult<EnableMfaResponse>> EnableMfaAsync(EnableMfaRequest dto)
        {
            try
            {
                var response = await mfaService.EnableMfaAsync(dto);
                return Ok(response);
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

        [HttpGet("GenerateQRCode"), Authorize]
        public async Task<ActionResult> GenerateQRCode()
        {
            try
            {
                Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                Response.Headers.Pragma = "no-cache";
                Response.Headers.Expires = "0";

                var qrCodeBytes = await mfaService.GenerateTotpQRCodeAsync();
                return File(qrCodeBytes, "image/png");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("VerifyTotp"), Authorize]
        public async Task<ActionResult<VerifyTotpResponse>> VerifyTotp(VerifyTotpRequest dto)
        {
            try
            {
                var response = await mfaService.VerifyTotpAsync(dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("DisableMfa"), Authorize]
        public async Task<ActionResult> DisableMfa(DisableMfaRequest dto)
        {
            try
            {
                await mfaService.DisableMfaAsync(dto);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
