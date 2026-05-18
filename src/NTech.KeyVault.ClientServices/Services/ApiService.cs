using NTech.KeyVault.ClientServices.Abstractions;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using System.Net.Http.Json;

namespace NTech.KeyVault.ClientServices.Services
{
    public class ApiService(HttpClient httpClient, IHostProvider hostProvider)
    {
        #region Admin

        public async Task<List<AdminUserResponse>> ListUsersAsync(int page = 1, int pageSize = 10)
        {
            var uri = await hostProvider.BuildUri($"/admin/user/listusers?page={page}&pageSize={pageSize}");
            var resp = await httpClient.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return [];
            return await resp.Content.ReadFromJsonAsync<List<AdminUserResponse>>()
                ?? [];
        }
        #endregion

        #region Application
        public async Task<List<ApplicationResponse>> ListApplicationsAsync()
        {
            var uri = await hostProvider.BuildUri("/application/list");
            var resp = await httpClient.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return [];
            return await resp.Content.ReadFromJsonAsync<List<ApplicationResponse>>()
                ?? [];
        }

        public async Task<DecryptedApplication?> CreateApplicationAsync(CreateApplicationRequest model)
        {
            var uri = await hostProvider.BuildUri("/application/create");
            var resp = await httpClient.PostAsJsonAsync(uri, model);
            if (!resp.IsSuccessStatusCode)
                return null;
            return await resp.Content.ReadFromJsonAsync<DecryptedApplication>();
        }

        public async Task<ApplicationResponse?> GetApplicationByIdAsync(Guid applicationId)
        {
            var uri = await hostProvider.BuildUri($"/application/get?applicationId={applicationId}");
            var resp = await httpClient.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return null;
            return await resp.Content.ReadFromJsonAsync<ApplicationResponse>();
        }

        public async Task<bool> DeleteApplicationAsync(Guid applicationId, string applicationName)
        {
            var uri = await hostProvider.BuildUri($"/application/delete?applicationId={applicationId}&applicationName={applicationName}");
            var resp = await httpClient.DeleteAsync(uri);
            return resp.IsSuccessStatusCode;
        }

        public async Task<DecryptedApplication?> GetApplicationDetailsAsync(Guid applicationId)
        {
            var uri = await hostProvider.BuildUri($"/application/getdetails?applicationId={applicationId}");
            var resp = await httpClient.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return null;
            return await resp.Content.ReadFromJsonAsync<DecryptedApplication>();
        }

        public async Task<ApplicationConfigurationResponse?> GetApplicationConfigurationAsync(Guid applicationId)
        {
            var uri = await hostProvider.BuildUri($"/appconfiguration/{applicationId}");
            var resp = await httpClient.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
                return null;
            return await resp.Content.ReadFromJsonAsync<ApplicationConfigurationResponse>();
        }

        public async Task<bool> SetApplicationConfigurationAsync(Guid applicationId, Dictionary<string, object> configurationData)
        {
            var uri = await hostProvider.BuildUri("/appconfiguration");
            var model = new CreateAppConfigurationRequest(applicationId, configurationData);
            var resp = await httpClient.PostAsJsonAsync(uri, model);
            return resp.IsSuccessStatusCode;
        }
        #endregion
    }
}
