using System.Net.Http.Headers;

namespace NTech.KeyVault.Blazor.Shared.Services
{
    public sealed class ApiClientFactory
    {
        private readonly IHttpClientFactory _factory;

        public ApiClientFactory(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public HttpClient Create(string baseAddress, string? token = null)
        {
            var client = _factory.CreateClient("dynamic");

            client.BaseAddress = new Uri(baseAddress);

            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }
}
