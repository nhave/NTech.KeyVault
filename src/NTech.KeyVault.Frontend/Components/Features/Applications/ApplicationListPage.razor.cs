using Microsoft.AspNetCore.Components;
using NTech.KeyVault.ClientServices.Services;
using NTech.KeyVault.Common.Models.Dtos.Responses;

namespace NTech.KeyVault.Frontend.Components.Features.Applications
{
    public partial class ApplicationListPage
    {
        [Inject] ApiService ApiService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;

        private CreateModal createModal = default!;

        private List<ApplicationResponse>? applications = default!;

        protected override async Task OnInitializedAsync()
        {
            applications = await ApiService.ListApplicationsAsync();
        }

        private async Task HandleApplicationCreated(string id)
        {
            applications = await ApiService.ListApplicationsAsync();
            await InvokeAsync(StateHasChanged);
        }

        private void NavigateToApplication(string id)
        {
            NavigationManager.NavigateTo($"/applications/{id}");
        }
    }
}
