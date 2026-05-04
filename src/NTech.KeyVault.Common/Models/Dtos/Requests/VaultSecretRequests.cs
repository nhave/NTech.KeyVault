namespace NTech.KeyVault.Common.Models.Dtos.Requests
{
    public record SetVaultSecretRequest(Guid ApplicationId, string Name, string Value);
}
