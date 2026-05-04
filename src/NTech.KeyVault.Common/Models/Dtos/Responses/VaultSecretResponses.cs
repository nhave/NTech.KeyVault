namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record VaultSecretResponse(Guid ApplicationId, Guid SecretId, string Name, DateTime CreatedAt, DateTime UpdatedAt);
}
