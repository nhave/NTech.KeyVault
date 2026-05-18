namespace NTech.KeyVault.ClientServices.Abstractions
{
    public interface ITokenStore
    {
        public Task<bool> SetAsync<T>(string entityId, T data);
        public Task<T?> GetAsync<T>(string entityId);
        public Task ClearAsync(string entityId);
    }
}
