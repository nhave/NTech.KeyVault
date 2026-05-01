using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]"), Authorize]
    public class AppConfigurationController(IAppConfigurationService configurationService) : ControllerBase
    {
        /// <summary>
        /// Creates a new application configuration or updates an existing one based on the specified request data.
        /// </summary>
        /// <param name="dto">The request containing the application identifier and configuration data to set or update. Cannot be null.</param>
        /// <returns>An HTTP 200 OK result if the configuration is created or updated successfully; an HTTP 404 Not Found result
        /// if the specified application does not exist; an HTTP 401 Unauthorized result if the caller does not have
        /// permission; or an HTTP 500 Internal Server Error result for other failures.</returns>
        [HttpPost]
        public async Task<ActionResult> SetOrCreateAppConfiguration(CreateAppConfigurationRequest dto)
        {
            await configurationService.AddOrUpdateAsync(dto.ApplicationId, dto.ConfigurationData);
            return Ok();
        }

        /// <summary>
        /// Retrieves the configuration settings for the specified application.
        /// </summary>
        /// <remarks>Returns a 404 Not Found response if the application configuration does not exist, a
        /// 401 Unauthorized response if the caller lacks permission, or a 500 Internal Server Error for unexpected
        /// failures.</remarks>
        /// <param name="appId">The unique identifier of the application whose configuration is to be retrieved.</param>
        /// <returns>An <see cref="ActionResult{ApplicationConfigurationResponse}"/> containing the application's configuration
        /// if found; otherwise, a result indicating the error condition.</returns>
        [HttpGet("{appId}")]
        public async Task<ActionResult<ApplicationConfigurationResponse>> GetAppConfiguration(Guid appId)
        {
            var config = await configurationService.GetByAppIdWithPermissionsAsync(appId);
            return Ok(config);
        }

        /// <summary>
        /// Retrieves all available configuration version numbers for the specified application.
        /// </summary>
        /// <param name="appId">The unique identifier of the application for which to retrieve configuration versions.</param>
        /// <returns>An HTTP response containing a list of integers representing the available configuration version numbers for
        /// the specified application. Returns 404 if the application is not found, 401 if the caller is unauthorized,
        /// or 500 for other errors.</returns>
        [HttpGet("{appId}/versions")]
        public async Task<ActionResult<List<int>>> GetAppConfigurationVersions(Guid appId)
        {
            var versions = await configurationService.GetAllVersionsByAppIdAsync(appId);
            return Ok(versions);
        }

        /// <summary>
        /// Retrieves the application configuration for the specified application and version.
        /// </summary>
        /// <param name="appId">The unique identifier of the application whose configuration is to be retrieved.</param>
        /// <param name="version">The version number of the application configuration to retrieve. Must be a positive integer.</param>
        /// <returns>An ActionResult containing the application configuration for the specified application and version. Returns
        /// a 404 response if the configuration is not found, a 401 response if access is unauthorized, or a 500
        /// response for other errors.</returns>
        [HttpGet("{appId}/versions/{version}")]
        public async Task<ActionResult<ApplicationConfigurationResponse>> GetAppConfigurationByVersion(Guid appId, int version)
        {
            var config = await configurationService.GetByAppIdAndVersionAsync(appId, version);
            return Ok(config);
        }

        /// <summary>
        /// Deletes the configuration associated with the specified application identifier.
        /// </summary>
        /// <param name="appId">The unique identifier of the application whose configuration is to be deleted.</param>
        /// <returns>An HTTP 204 No Content response if the configuration is successfully deleted; HTTP 404 Not Found if the
        /// application configuration does not exist; HTTP 401 Unauthorized if the caller does not have permission; or
        /// HTTP 500 Internal Server Error for unexpected failures.</returns>
        [HttpDelete("{appId}")]
        public async Task<ActionResult> DeleteAppConfiguration(Guid appId)
        {
            await configurationService.DeleteByAppIdAsync(appId);
            return NoContent();
        }

        /// <summary>
        /// Deletes outdated configuration records associated with the specified application.
        /// </summary>
        /// <remarks>Use this endpoint to remove obsolete configuration data for an application. Only
        /// authorized users can perform this operation.</remarks>
        /// <param name="appId">The unique identifier of the application whose old configurations are to be removed.</param>
        /// <returns>An HTTP 204 response if the cleanup is successful; 404 if the application is not found; 401 if the caller is
        /// not authorized; or 500 for other errors.</returns>
        [HttpDelete("{appId}/cleanup")]
        public async Task<ActionResult> CleanupOldConfigurations(Guid appId)
        {
            await configurationService.CleanupOldConfigurations(appId);
            return NoContent();
        }
    }
}
