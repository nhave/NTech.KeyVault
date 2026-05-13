namespace NTech.KeyVault.ClientServices.Abstractions
{
    public interface IHostProvider
    {
        public Task<string> GetBaseAddressAsync();
        public Task<Uri> BuildUri(string path)
        {
            return GetBaseAddressAsync()
                .ContinueWith(t => new Uri(new Uri(t.Result), path));
        }
    }
}
