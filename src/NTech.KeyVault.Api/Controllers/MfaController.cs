using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]"), Authorize]
    public class MfaController(IMfaService mfaService) : ControllerBase
    {
        /// <summary>
        /// Retrieves the current multi-factor authentication (MFA) status for the authenticated user.
        /// </summary>
        /// <remarks>This endpoint requires the user to be authenticated. Use this method to determine
        /// whether MFA is enabled or required for the current user session.</remarks>
        /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="GetMfaStatusResponse"/> object with the user's MFA
        /// status information.</returns>
        [HttpGet("GetMfaStatus")]
        public async Task<ActionResult<GetMfaStatusResponse>> GetMfaStatusAsync()
        {
            var response = await mfaService.GetMfaStatusAsync();
            return Ok(response);
        }

        /// <summary>
        /// Enables multi-factor authentication (MFA) for a user account based on the provided request data.
        /// </summary>
        /// <param name="dto">An object containing the information required to enable MFA for the user. Cannot be null.</param>
        /// <returns>An ActionResult containing the result of the MFA enablement operation. Returns a successful response with
        /// details if MFA is enabled, a bad request if the input is invalid, or a 500 status code for unexpected
        /// errors.</returns>
        [HttpPost("EnableMFA")]
        public async Task<ActionResult<EnableMfaResponse>> EnableMfaAsync(EnableMfaRequest dto)
        {
            var response = await mfaService.EnableMfaAsync(dto);
            return Ok(response);
        }

        /// <summary>
        /// Generates a QR code image for configuring Time-based One-Time Password (TOTP) multi-factor authentication
        /// for the current user.
        /// </summary>
        /// <remarks>The generated QR code can be scanned by authenticator applications to set up
        /// TOTP-based multi-factor authentication. The response is not cached to ensure security.</remarks>
        /// <returns>An image file containing the QR code in PNG format if successful; otherwise, an appropriate error response
        /// such as Unauthorized, BadRequest, or Internal Server Error.</returns>
        [HttpGet("GenerateQRCode")]
        public async Task<ActionResult> GenerateQRCode()
        {
            // Set cache control headers to prevent caching of the QR code response for security reasons
            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";

            var qrCodeBytes = await mfaService.GenerateTotpQRCodeAsync();
            return File(qrCodeBytes, "image/png");
        }

        /// <summary>
        /// Verifies a time-based one-time password (TOTP) code as part of multi-factor authentication (MFA) setup.
        /// </summary>
        /// <remarks>This endpoint is typically used during the MFA enrollment process to confirm that the
        /// user can generate valid TOTP codes. The response indicates whether the provided code is valid and may
        /// include additional information relevant to the MFA setup process.</remarks>
        /// <param name="dto">The request containing the TOTP code and related information required for verification.</param>
        /// <returns>An HTTP response containing the result of the TOTP verification. Returns a 200 OK response with verification
        /// details if successful; otherwise, returns an appropriate error response.</returns>
        [HttpPost("VerifyTotp")]
        public async Task<ActionResult<VerifyTotpResponse>> VerifyTotp(VerifyTotpRequest dto)
        {
            var response = await mfaService.VerifyTotpAsync(dto);
            return Ok(response);
        }

        /// <summary>
        /// Disables multi-factor authentication (MFA) for the specified user account.
        /// </summary>
        /// <param name="dto">An object containing the information required to identify the user and process the MFA disable request.
        /// Cannot be null.</param>
        /// <returns>An HTTP response indicating the result of the operation. Returns 204 No Content if successful, 401
        /// Unauthorized if the user is not authorized, 400 Bad Request for invalid input or operation, or 500 Internal
        /// Server Error for unexpected failures.</returns>
        [HttpPost("DisableMfa")]
        public async Task<ActionResult> DisableMfa(DisableMfaRequest dto)
        {
            await mfaService.DisableMfaAsync(dto);
            return NoContent();
        }
    }
}
