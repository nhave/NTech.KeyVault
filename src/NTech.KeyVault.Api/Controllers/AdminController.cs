using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "SystemAdmin,Admin")]
    public class AdminController(IUserService userService) : ControllerBase
    {
        [HttpPost("User/Create")]
        public async Task<ActionResult<UserCreateResponse>> CreateUser(CreateUserRequest dto)
        {
            try
            {
                var user = await userService.CreateUserAsync(dto.Username, dto.FullName, dto.Email, dto.Password);

                return Ok(new UserCreateResponse(user.Id.ToString()));
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

        [HttpPost("User/AddRoles")]
        public async Task<ActionResult> AddRoles(UserRolesRequest dto)
        {
            try
            {
                List<Roles> roles = dto.Roles.Select(r => Enum.Parse<Roles>(r)).ToList();

                await userService.AddRolesAsync(dto.UserId, roles);
                return NoContent();
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

        [HttpPost("User/RemoveRoles")]
        public async Task<ActionResult> RemoveRoles(UserRolesRequest dto)
        {
            try
            {
                List<Roles> roles = dto.Roles.Select(r => Enum.Parse<Roles>(r)).ToList();

                await userService.RemoveRolesAsync(dto.UserId, roles);
                return NoContent();
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

        [HttpPost("User/Disable")]
        public async Task<ActionResult> DisableUser(GeneralUserRequest dto)
        {
            try
            {
                await userService.DisableUserAsync(dto.UserId);
                return NoContent();
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

        [HttpPost("User/Enable")]
        public async Task<ActionResult> EnableUser(GeneralUserRequest dto)
        {
            try
            {
                await userService.EnableUserAsync(dto.UserId);
                return NoContent();
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

        [HttpDelete("User/Delete")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            try
            {
                await userService.DeleteUserAsync(userId);
                return NoContent();
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
    }
}
