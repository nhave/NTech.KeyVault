namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record ApplicationResponse(Guid Id, string Name, string? Description, Guid? OwnerUserId, string? OwnerUsername, List<string> Permissions);

    public record ApplicationUserResponse(Guid Id, string Username, List<string> Permissions);
    public record ApplicationUsersResponse(Guid ApplicationId, List<ApplicationUserResponse> Users);

    public record ApplicationConfigurationResponse(Guid ApplicationId, int Version, Guid? CreatedById, string? CreatedByUsername, Dictionary<string, object> ConfigurationData);
}
