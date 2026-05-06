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
        /// <summary>
        /// Creates a new user account with the specified details.
        /// </summary>
        /// <param name="dto">An object containing the information required to create the user, including username, full name, email, and
        /// password. Cannot be null.</param>
        /// <returns>An ActionResult containing a UserCreateResponse with the identifier of the newly created user if successful;
        /// otherwise, a BadRequest result with an error message for invalid input, or a 500 status code for unexpected
        /// errors.</returns>
        [HttpPost("User/Create")]
        public async Task<ActionResult<UserCreateResponse>> CreateUser(CreateUserRequest dto)
        {
            var user = await userService.CreateUserAsync(dto.Username, dto.FullName, dto.Email, dto.Password);
            return Ok(new UserCreateResponse(user.Id.ToString()));
        }

        /// <summary>
        /// Retrieves a paginated list of user accounts that the current user is permitted to manage.
        /// </summary>
        /// <remarks>Users with a lower role hierarchy cannot manage or view accounts with a higher role
        /// hierarchy. The result never includes the current user's own account, even if they have permission to view
        /// it.</remarks>
        /// <param name="page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The maximum number of user accounts to include in a single page. Must be greater than 0.</param>
        /// <returns>An asynchronous operation that returns an HTTP action result containing a list of user account details. The
        /// list is empty if no users are available for the specified page.</returns>
        [HttpGet("User/ListUsers")]
        public async Task<ActionResult<List<AdminUserResponse>>> ListUsers(int page = 1, int pageSize = 10)
        {
            var users = await userService.GetUsersAsync(page, pageSize);
            return Ok(users.Select(u => new AdminUserResponse(u.Id, u.Username, u.Roles.Select(r => r.ToString()).ToList())).ToList());
        }

        /// <summary>
        /// Adds one or more roles to a user based on the specified request data.
        /// </summary>
        /// <remarks>Only users with sufficient privileges can assign certain roles. The operation may
        /// fail if the specified roles are not valid or if the user does not have permission to assign them.</remarks>
        /// <param name="dto">An object containing the user identifier and the list of roles to assign. The roles must be valid role names
        /// defined in the system.</param>
        /// <returns>A result indicating the outcome of the operation. Returns 204 No Content if the roles are added
        /// successfully; 400 Bad Request if the request is invalid; or 500 Internal Server Error if an unexpected error
        /// occurs.</returns>
        [HttpPost("User/AddRoles")]
        public async Task<ActionResult> AddRoles(UserRolesRequest dto)
        {
            List<Roles> roles = dto.Roles.Select(r => Enum.Parse<Roles>(r)).ToList();
            await userService.AddRolesAsync(dto.UserId, roles);
            return NoContent();
        }

        /// <summary>
        /// Removes one or more roles from the specified user.
        /// </summary>
        /// <remarks>Only users with the SystemAdmin role can remove the Admin role, and the SystemAdmin
        /// role itself cannot be removed.</remarks>
        /// <param name="dto">An object containing the user identifier and the list of roles to remove. The user identifier must refer to
        /// an existing user. The roles list must contain valid role names.</param>
        /// <returns>A 204 No Content response if the roles are successfully removed; a 400 Bad Request response if the request
        /// is invalid; or a 500 Internal Server Error response if an unexpected error occurs.</returns>
        [HttpPost("User/RemoveRoles")]
        public async Task<ActionResult> RemoveRoles(UserRolesRequest dto)
        {
            List<Roles> roles = dto.Roles.Select(r => Enum.Parse<Roles>(r)).ToList();
            await userService.RemoveRolesAsync(dto.UserId, roles);
            return NoContent();
        }

        /// <summary>
        /// Disables a user account specified by the request data.
        /// </summary>
        /// <remarks>Only users with a lower role hierarchy than the caller can be disabled. The current
        /// user cannot disable their own account.</remarks>
        /// <param name="dto">The request containing the user identifier of the account to disable. Must not refer to the current user or
        /// a user with equal or higher role hierarchy.</param>
        /// <returns>An HTTP 204 No Content response if the user is successfully disabled; otherwise, an appropriate error
        /// response.</returns>
        [HttpPost("User/Disable")]
        public async Task<ActionResult> DisableUser(GeneralUserRequest dto)
        {
            await userService.DisableUserAsync(dto.UserId);
            return NoContent();
        }

        /// <summary>
        /// Enables a user account specified by the provided request data.
        /// </summary>
        /// <remarks>This action can only be performed on users with a lower role hierarchy than the
        /// caller and cannot be used to enable the caller's own account.</remarks>
        /// <param name="dto">An object containing the user identifier of the account to enable. The user must not be the current user and
        /// must have a lower role hierarchy.</param>
        /// <returns>An HTTP 204 No Content response if the user is successfully enabled; otherwise, an appropriate error
        /// response.</returns>
        [HttpPost("User/Enable")]
        public async Task<ActionResult> EnableUser(GeneralUserRequest dto)
        {
            await userService.EnableUserAsync(dto.UserId);
            return NoContent();
        }

        /// <summary>
        /// Deletes the specified user account if the user is disabled and has a lower role hierarchy than the caller.
        /// </summary>
        /// <remarks>The user must be disabled before deletion and must have a lower role hierarchy than
        /// the caller. Only authorized callers with sufficient privileges can perform this operation.</remarks>
        /// <param name="userId">The unique identifier of the user to delete. Cannot be null or empty.</param>
        /// <returns>An HTTP 204 No Content response if the user is successfully deleted; otherwise, an appropriate error
        /// response.</returns>
        [HttpDelete("User/Delete")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            await userService.DeleteUserAsync(userId);
            return NoContent();
        }
    }
}
