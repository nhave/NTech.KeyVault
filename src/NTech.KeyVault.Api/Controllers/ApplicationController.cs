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
