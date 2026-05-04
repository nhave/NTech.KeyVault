using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppIOController(IAppIOService iOService) : ControllerBase
    {
        /// <summary>
        /// Validates the application credentials provided in the request headers and returns the result of the
        /// validation.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application to validate. Must be provided in the request header.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Must be provided in the request header.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an ActionResult with a
        /// SimpleApplicationResponse indicating the outcome of the validation.</returns>
        [HttpGet("Validate")]
        public async Task<ActionResult<SimpleApplicationResponse>> Validate([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret)
        {
            var result = await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the application configuration for the specified application credentials.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application. Must correspond to a registered application.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Used to authenticate the request. Cannot be null or empty.</param>
        /// <returns>An ActionResult containing the application configuration if the credentials are valid; otherwise, an error
        /// response.</returns>
        [HttpGet("Config")]
        public async Task<ActionResult<ApplicationConfigurationResponse>> GetConfig([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            var result = await iOService.GetConfigurationByAppIdAsync(ApplicationId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the value of a specified secret from the vault using the provided application credentials.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application requesting the secret. Must correspond to a registered application.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Used to authenticate the request. Cannot be null or empty.</param>
        /// <param name="SecretName">The name of the secret to retrieve from the vault. Cannot be null or empty.</param>
        /// <returns>An ActionResult containing the value of the requested secret as a string if found and authorized; otherwise,
        /// an appropriate error response.</returns>
        [HttpGet("Secret")]
        public async Task<ActionResult<string>> GetSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromQuery] string SecretName)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            var result = await iOService.GetVaultSecretValueAsync(ApplicationId, SecretName);
            return Ok(result);
        }

        /// <summary>
        /// Creates or updates a secret in the vault for the specified application.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application. Used to authenticate and determine the target vault. Must be a
        /// valid GUID.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Used for authentication. Cannot be null or empty.</param>
        /// <param name="dto">An object containing the name and value of the secret to set. The name must be unique within the
        /// application's vault.</param>
        /// <returns>A result indicating the outcome of the operation. Returns a 204 No Content response if the secret is set
        /// successfully.</returns>
        [HttpPost("Secret")]
        public async Task<ActionResult> SetSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromBody] SetVaultSecretRequest dto)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            await iOService.SetVaultSecretAsync(ApplicationId, dto.Name, dto.Value);
            return NoContent();
        }

        /// <summary>
        /// Deletes a secret from the application's vault using the specified secret identifier.
        /// </summary>
        /// <remarks>The request requires valid application credentials provided in the headers. If the
        /// credentials are invalid or the secret does not exist, an appropriate error response is returned.</remarks>
        /// <param name="ApplicationId">The unique identifier of the application. Used to authenticate and authorize the request. Must correspond to
        /// a valid application.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Used for authentication. Cannot be null or empty.</param>
        /// <param name="SecretId">The unique identifier of the secret to delete from the vault. Must reference an existing secret.</param>
        /// <returns>A result indicating the outcome of the delete operation. Returns a 204 No Content response if the secret is
        /// successfully deleted.</returns>
        [HttpDelete("Secret")]
        public async Task<ActionResult> DeleteSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromQuery] Guid SecretId)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            await iOService.DeleteVaultSecretAsync(ApplicationId, SecretId);
            return NoContent();
        }
    }
}
