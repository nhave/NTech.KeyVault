using NTech.KeyVault.ClientServices.Abstractions;

namespace NTech.KeyVault.Frontend.Services
{
    public sealed class BlazorHostProvider : IHostProvider
    {
        private readonly string _baseUrl;

        public BlazorHostProvider(IConfiguration config)
        {
            _baseUrl = config["Api:BaseUrl"]
                ?? throw new InvalidOperationException("Missing Api:BaseUrl");
        }

        public Task<string> GetBaseAddressAsync()
            => Task.FromResult(_baseUrl);
    }
}
