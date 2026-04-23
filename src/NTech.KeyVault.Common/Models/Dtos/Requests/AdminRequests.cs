namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public record CreateUserRequest(string Username, string FullName, string Email, string Password);
    public record UserRolesRequest(string UserId, List<string> Roles);
    public record GeneralUserRequest(string UserId);
}
