namespace NTech.KeyVault.Blazor.Web.Services
{
    public interface ITokenStore
    {
        public Task<string> GetAsync(string entityId);
        public Task SaveAsync(string entityId, string json);
    }

    public class TokenStore : ITokenStore
    {
        public async Task<string> GetAsync(string entityId)
        {
            // Implementér logik for at hente token fra en sikker opbevaring
            return string.Empty; // Placeholder
        }

        public async Task SaveAsync(string entityId, string json)
        {
            // Implementér logik for at gemme token i en sikker opbevaring
        }
    }
}
