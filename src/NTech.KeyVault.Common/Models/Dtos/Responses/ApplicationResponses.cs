namespace NTech.KeyVault.Common.Models.Dtos.Responses
{
    public record ApplicationResponse(Guid Id, string Name, string? Description, Guid? OwnerUserId, string? OwnerUsername);
}
