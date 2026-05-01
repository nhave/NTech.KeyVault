using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
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
            var result = await iOService.GetByAppIdAsync(ApplicationId, ApplicationSecret);
            return Ok(result);
        }

        [HttpGet("Secret")]
        public async Task<ActionResult> GetSecret()
        {
            return Ok();
        }

        [HttpPost("Secret")]
        public async Task<ActionResult> SerSecret()
        {
            return Ok();
        }
    }
}
