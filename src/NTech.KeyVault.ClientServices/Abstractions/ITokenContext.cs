namespace NTech.KeyVault.ClientServices.Abstractions
{
    public interface ITokenContext
    {
        string? GetEntityId();
        bool IsAuthenticated();
    }
}
