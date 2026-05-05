using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppIOController(IAppIOService iOService, IVaultSecretService vaultSecretService) : ControllerBase
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
        /// Retrieves the value of a named secret associated with the specified application.
        /// </summary>
        /// <remarks>The caller must provide valid application credentials in the request headers. If
        /// authentication fails or the secret does not exist, an error response is returned.</remarks>
        /// <param name="ApplicationId">The unique identifier of the application requesting the secret. Must correspond to a registered application.</param>
        /// <param name="ApplicationSecret">The secret key used to authenticate the application. Cannot be null or empty.</param>
        /// <param name="SecretName">The name of the secret to retrieve. Cannot be null or empty.</param>
        /// <returns>An HTTP 200 response containing the secret value as a string if the request is authenticated and the secret
        /// exists; otherwise, an appropriate error response.</returns>
        [HttpGet("Secret")]
        public async Task<ActionResult<string>> GetSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromQuery] string SecretName)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            var result = await iOService.GetAppSecretValueAsync(ApplicationId, SecretName);
            return Ok(result);
        }

        /// <summary>
        /// Sets or updates the secret value for the specified application.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application for which the secret is being set. Must correspond to a valid
        /// application.</param>
        /// <param name="ApplicationSecret">The secret key used to authenticate the application. Must match the application's current secret.</param>
        /// <param name="dto">An object containing the name and value of the secret to set for the application. Cannot be null.</param>
        /// <returns>A result indicating the outcome of the operation. Returns a 204 No Content response if the secret is set
        /// successfully.</returns>
        [HttpPost("Secret")]
        public async Task<ActionResult> SetSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromBody] SetAppSecretRequest dto)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            await iOService.SetAppSecretAsync(ApplicationId, dto.Name, dto.Value);
            return NoContent();
        }

        /// <summary>
        /// Deletes the specified secret associated with the given application.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application. Used to authenticate and authorize the request.</param>
        /// <param name="ApplicationSecret">The secret key for the application. Must match the application's credentials for authentication.</param>
        /// <param name="SecretId">The unique identifier of the secret to delete.</param>
        /// <returns>A result indicating the outcome of the delete operation. Returns a 204 No Content response if the secret is
        /// successfully deleted.</returns>
        [HttpDelete("Secret")]
        public async Task<ActionResult> DeleteSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromQuery] Guid SecretId)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            await iOService.DeleteAppSecretAsync(ApplicationId, SecretId);
            return NoContent();
        }

        /// <summary>
        /// Retrieves the secret value associated with the specified entity from the vault.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application making the request. Used for authentication and authorization. Must
        /// be a valid GUID.</param>
        /// <param name="ApplicationSecret">The secret key for the application. Used to authenticate the request. Cannot be null or empty.</param>
        /// <param name="EntityId">The identifier of the entity whose secret is to be retrieved from the vault. Cannot be null or empty.</param>
        /// <returns>An ActionResult containing the secret value as a string if the request is authorized and the entity exists;
        /// otherwise, an appropriate error response.</returns>
        [HttpGet("VaultSecret/{EntityId}")]
        public async Task<ActionResult<string>> GetVaultSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromRoute] string EntityId)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            var result = await vaultSecretService.GetVaultSecretAsync(ApplicationId, EntityId);
            return Ok(result);
        }

        /// <summary>
        /// Creates or updates a secret in the vault for the specified entity.
        /// </summary>
        /// <remarks>The caller must provide valid application credentials in the request headers. If the
        /// secret already exists for the specified entity, it will be overwritten.</remarks>
        /// <param name="ApplicationId">The unique identifier of the application making the request. Used to authenticate the caller.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Used to authenticate the caller.</param>
        /// <param name="EntityId">The identifier of the entity for which the secret is being set.</param>
        /// <param name="dto">An object containing the secret value and optional expiration date to be stored in the vault.</param>
        /// <returns>A result indicating the outcome of the operation. Returns a 204 No Content response if the secret is set
        /// successfully.</returns>
        [HttpPost("VaultSecret/{EntityId}")]
        public async Task<ActionResult> SetVaultSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromRoute] string EntityId, [FromBody] SetVaultSecretRequest dto)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            await vaultSecretService.SetVaultSecretAsync(ApplicationId, EntityId, dto.SecretValue, dto.ExpirationDate);
            return NoContent();
        }

        /// <summary>
        /// Deletes a vault secret identified by the specified entity ID for the given application.
        /// </summary>
        /// <param name="ApplicationId">The unique identifier of the application making the request. Used to authenticate and authorize the
        /// operation.</param>
        /// <param name="ApplicationSecret">The secret key associated with the application. Required for authentication.</param>
        /// <param name="EntityId">The identifier of the vault secret to delete.</param>
        /// <returns>A result indicating the outcome of the delete operation. Returns a 204 No Content response if the deletion
        /// is successful.</returns>
        [HttpDelete("VaultSecret/{EntityId}")]
        public async Task<ActionResult> DeleteVaultSecret([FromHeader] Guid ApplicationId, [FromHeader] string ApplicationSecret, [FromRoute] string EntityId)
        {
            await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            await vaultSecretService.DeleteVaultSecretAsync(ApplicationId, EntityId);
            return NoContent();
        }
    }
}
