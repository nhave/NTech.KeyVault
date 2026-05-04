using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]"), Authorize]
    public class VaultSecretController(IVaultSecretService secretService) : ControllerBase
    {
        /// <summary>
        /// Retrieves a secret associated with the specified application and secret name.
        /// </summary>
        /// <param name="applicationId">The unique identifier of the application for which to retrieve the secret.</param>
        /// <param name="name">The name of the secret to retrieve. Cannot be null or empty.</param>
        /// <returns>An ActionResult containing the secret information for the specified application and name.</returns>
        [HttpGet("GetSecret")]
        public async Task<ActionResult<VaultSecretResponse>> GetSecret(Guid applicationId, string name)
        {
            var result = await secretService.GetVaultSecretAsync(applicationId, name);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the value of a secret identified by the specified secret ID.
        /// </summary>
        /// <param name="secretId">The unique identifier of the secret to retrieve.</param>
        /// <returns>An HTTP 200 response containing the secret value as a string if found; otherwise, an appropriate error
        /// response.</returns>
        [HttpGet("GetSecretValue")]
        public async Task<ActionResult<string>> GetSecretValue(Guid secretId)
        {
            var result = await secretService.GetSecretValueAsync(secretId);
            return Ok(result);
        }

        /// <summary>
        /// Creates or updates a secret in the vault for the specified application.
        /// </summary>
        /// <param name="dto">The request containing the application identifier, secret name, and secret value to be stored or updated.
        /// Cannot be null.</param>
        /// <returns>An ActionResult containing the response with details of the stored secret.</returns>
        [HttpPost("SetSecret")]
        public async Task<ActionResult<VaultSecretResponse>> SetSecret(SetVaultSecretRequest dto)
        {
            var result = await secretService.SetSecretAsync(dto.ApplicationId, dto.Name, dto.Value);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all secrets associated with the specified application.
        /// </summary>
        /// <param name="applicationId">The unique identifier of the application for which to retrieve secrets.</param>
        /// <returns>A list of vault secret responses for the specified application. Returns an empty list if no secrets are
        /// found.</returns>
        [HttpGet("GetApplicationSecrets")]
        public async Task<ActionResult<List<VaultSecretResponse>>> GetApplicationSecrets(Guid applicationId)
        {
            var result = await secretService.GetVaultSecretsByApplicationIdAsync(applicationId);
            return Ok(result);
        }

        /// <summary>
        /// Deletes the specified secret from the vault.
        /// </summary>
        /// <remarks>Use this method to permanently remove a secret from the vault. The operation is
        /// idempotent; attempting to delete a non-existent secret does not result in an error.</remarks>
        /// <param name="secretId">The unique identifier of the secret to delete.</param>
        /// <param name="name">The name of the secret to delete. Cannot be null or empty.</param>
        /// <returns>A result indicating that the operation completed successfully with no content.</returns>
        [HttpDelete("DeleteSecret")]
        public async Task<IActionResult> DeleteSecret(Guid secretId, string name)
        {
            await secretService.DeleteVaultSecretAsync(secretId, name);
            return NoContent();
        }
    }
}
