namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record SimpleApplicationResponse(Guid Id, string Name, string? Description, Guid? OwnerUserId);
    public record ApplicationResponse(Guid Id, string Name, string? Description, Guid? OwnerUserId, string? OwnerUsername, List<string> Permissions);

    public record ApplicationUserResponse(Guid Id, string Username, List<string> Permissions);
    public record ApplicationUsersResponse(Guid ApplicationId, List<ApplicationUserResponse> Users);

    public class ApplicationConfigurationResponse
    {
        public Guid ApplicationId { get; }
        public int Version { get; }
        public Guid? CreatedById { get; }
        public string? CreatedByUsername { get; }
        public Dictionary<string, object> ConfigurationData { get; }

        public ApplicationConfigurationResponse(Guid ApplicationId, int Version, Guid? CreatedById, string? CreatedByUsername, Dictionary<string, object> ConfigurationData)
        {
            this.ApplicationId = ApplicationId;
            this.Version = Version;
            this.CreatedById = CreatedById;
            this.CreatedByUsername = CreatedByUsername;
            this.ConfigurationData = ConfigurationData;
        }

        public ApplicationConfigurationResponse(Guid ApplicationId)
            : this(ApplicationId, -1, null, null, new Dictionary<string, object>()) { }
    }

    public record AppSecretResponse(Guid ApplicationId, Guid SecretId, string Name, DateTime CreatedAt, DateTime UpdatedAt);
}
