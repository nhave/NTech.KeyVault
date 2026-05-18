using System.ComponentModel.DataAnnotations;

namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public class CreateApplicationRequest
    {
        [Required(ErrorMessage = "Application name is required.")]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
    public record SetUserPermissionsRequest(Guid ApplicationId, Guid UserId, List<String> Permissions);
    public record UpdateApplicationRequest(Guid ApplicationId, string Name, string? Description = null);
    public record CreateAppConfigurationRequest(Guid ApplicationId, Dictionary<string, object> ConfigurationData);
    public record SetAppSecretRequest(Guid ApplicationId, string Name, string Value);
    public record SetVaultSecretRequest(string SecretValue, DateTime ExpirationDate);
}
