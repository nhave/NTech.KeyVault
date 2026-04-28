using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Enums;
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

        /// <summary>
        /// Sets the specified permissions for a user within an application.
        /// </summary>
        /// <remarks>This method is intended to be called by authorized clients to update a user's
        /// permissions. The caller must ensure that the provided permissions are valid for the target
        /// application.</remarks>
        /// <param name="dto">An object containing the application identifier, user identifier, and the list of permissions to assign.
        /// Cannot be null.</param>
        /// <returns>An HTTP response indicating the result of the operation. Returns 204 No Content if successful, 400 Bad
        /// Request if the input is invalid, 401 Unauthorized if the operation is not permitted, or 500 Internal Server
        /// Error for unexpected failures.</returns>
        [HttpPost("SetUserPermissions")]
        public async Task<ActionResult> SetUserPermissions(SetUserPermissionsRequest dto)
        {
            try
            {
                List<Permission> enumPermissions = PermissionHelper.Parse(ResourceType.Application, dto.Permissions);
                await applicationService.SetUserPermissionsAsync(dto.ApplicationId, dto.UserId, enumPermissions);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Retrieves the list of permissions assigned to a specific user for a given application.
        /// </summary>
        /// <remarks>Only application owners and the user themselves are authorized to access this
        /// endpoint. The response includes only the permissions for the specified user, not all users with permissions
        /// for the application.</remarks>
        /// <param name="applicationId">The unique identifier of the application for which to retrieve the user's permissions.</param>
        /// <param name="userId">The unique identifier of the user whose permissions are being requested.</param>
        /// <returns>An ActionResult containing a list of permission names assigned to the specified user for the application.
        /// Returns an empty list if the user has no permissions.</returns>
        [HttpGet("GetUserPermissions")]
        public async Task<ActionResult<List<string>>> GetUserPermissions(Guid applicationId, Guid userId)
        {
            try
            {
                var response = await applicationService.GetUserPermissionsAsync(applicationId, userId);
                return Ok(response.Permissions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Retrieves all users who have permissions for the specified application.
        /// </summary>
        /// <remarks>Only application owners can access this
        /// endpoint. The response includes all users with permissions for the application, not just the permissions of
        /// the caller.</remarks>
        /// <param name="applicationId">The unique identifier of the application for which to retrieve users with permissions.</param>
        /// <returns>An ActionResult containing an ApplicationUsersResponse with the list of users who have permissions for the
        /// application. Returns a 400 Bad Request if the application ID is invalid, a 401 Unauthorized if the caller
        /// lacks access, or a 500 Internal Server Error for unexpected failures.</returns>
        [HttpGet("GetUsers")]
        public async Task<ActionResult<ApplicationUsersResponse>> GetApplicationUsers(Guid applicationId)
        {
            try
            {
                var response = await applicationService.GetApplicationUsersAsync(applicationId);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Removes all permissions for the specified user within the given application.
        /// </summary>
        /// <remarks>Only application owners are authorized to remove user permissions using this
        /// endpoint. All permissions for the specified user in the application will be removed.</remarks>
        /// <param name="applicationId">The unique identifier of the application from which to remove the user's permissions.</param>
        /// <param name="userId">The unique identifier of the user whose permissions will be removed.</param>
        /// <returns>An HTTP response indicating the result of the operation. Returns 204 No Content if successful, 400 Bad
        /// Request if the input is invalid, 401 Unauthorized if the caller is not permitted, or 500 Internal Server
        /// Error for unexpected failures.</returns>
        [HttpDelete("RemoveUserPermissions")]
        public async Task<ActionResult> RemoveUserPermissions(Guid applicationId, Guid userId)
        {
            try
            {
                await applicationService.RemoveUserPermissionsAsync(applicationId, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}