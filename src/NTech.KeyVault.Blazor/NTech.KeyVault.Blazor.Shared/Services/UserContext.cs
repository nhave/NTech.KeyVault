namespace NTech.KeyVault.Blazor.Shared.Services
{
    public interface IUserContext
    {
        string EntityId { get; }
    }

    public class UserContext : IUserContext
    {
        public string EntityId => "test";
    }
}
