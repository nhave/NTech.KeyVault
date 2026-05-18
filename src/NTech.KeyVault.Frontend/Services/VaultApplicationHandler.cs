namespace NTech.KeyVault.Frontend.Services
{
    public sealed class VaultApplicationHandler : DelegatingHandler
    {
        private readonly string _appId;
        private readonly string _appSecret;

        public VaultApplicationHandler(IConfiguration config)
        {
            _appId = config["Vault:ApplicationId"]
                ?? throw new InvalidOperationException("Missing Vault:ApplicationId");

            _appSecret = config["Vault:ApplicationSecret"]
                ?? throw new InvalidOperationException("Missing Vault:ApplicationSecret");
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Add("ApplicationId", _appId); // X-Application-Id
            request.Headers.Add("ApplicationSecret", _appSecret); // X-Application-Secret

            return base.SendAsync(request, cancellationToken);
        }
    }
}
