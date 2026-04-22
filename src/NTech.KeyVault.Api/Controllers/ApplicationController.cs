using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]"), Authorize]
    public class ApplicationController(IApplicationService applicationService) : ControllerBase
    {
        /// <summary>
        /// Creates a new application using the specified request data.
        /// </summary>
        /// <param name="dto">The request object containing the details required to create the application. Cannot be null.</param>
        /// <returns>An ActionResult containing the newly created and decrypted application if successful; a BadRequest result
        /// with an error message if the request data is invalid; or a 500 Internal Server Error result if an unexpected
        /// error occurs.</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<DecryptedApplication>> CreateApplication(CreateApplicationRequest dto)
        {
            try
            {
                var application = await applicationService.CreateApplicationAsync(dto);
                return Ok(application);
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
        /// Retrieves a list of applications that the current user is authorized to access.
        /// </summary>
        /// <returns>An <see cref="ActionResult{T}"/> containing a list of <see cref="ApplicationResponse"/> objects representing
        /// the accessible applications. Returns a 500 status code if an error occurs.</returns>
        [HttpGet("List")]
        public async Task<ActionResult<List<ApplicationResponse>>> GetAccessibleApplications()
        {
            try
            {
                var applications = await applicationService.GetAccessibleApplicationsAsync();
                return Ok(applications);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
