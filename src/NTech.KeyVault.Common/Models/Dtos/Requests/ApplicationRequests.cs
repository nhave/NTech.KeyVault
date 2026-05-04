namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public record CreateApplicationRequest(string Name, string? Description = null);
    public record SetUserPermissionsRequest(Guid ApplicationId, Guid UserId, List<String> Permissions);
    public record UpdateApplicationRequest(Guid ApplicationId, string Name, string? Description = null);
    public record CreateAppConfigurationRequest(Guid ApplicationId, Dictionary<string, object> ConfigurationData);
    public record SetAppSecretRequest(Guid ApplicationId, string Name, string Value);
}
