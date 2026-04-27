namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public record CreateApplicationRequest(string Name, string? Description = null);
    public record SetUserPermissionsRequest(Guid ApplicationId, Guid UserId, List<String> Permissions);
}
