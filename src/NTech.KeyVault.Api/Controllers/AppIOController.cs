using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppIOController(IAppIOService iOService) : ControllerBase
    {
        [HttpGet("Validate")]
        public async Task<ActionResult<SimpleApplicationResponse>> Validate([FromHeader] string ApplicationId, [FromHeader] string ApplicationSecret)
        {
            var result = await iOService.ValidateAsync(ApplicationId, ApplicationSecret);
            return Ok(result);
        }

        [HttpGet("Config")]
        public async Task<ActionResult> GetConfig()
        {
            return Ok();
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
