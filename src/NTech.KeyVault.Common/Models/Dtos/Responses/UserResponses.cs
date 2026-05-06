using NTech.KeyVault.Common.Enums;

namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record UserCreateResponse(string Id);
    public record UserResponse(Guid Id, string Username);
    public record AdminUserResponse(Guid Id, string Username, List<string> Roles);
}
