using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using RouteData = Microsoft.AspNetCore.Components.RouteData;

namespace NTech.KeyVault.Frontend.Components
{
    public partial class Routes
    {
        [Inject] private IHostEnvironment Environment { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;

        // private bool IsDevOnly(RouteData routeData)
        //     => Attribute.IsDefined(routeData.PageType, typeof(DevOnlyAttribute)) && !Env.IsDevelopment();

        private RenderFragment RedirectToLogin() => async builder =>
        {
            var currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            var encodedUrl = Uri.EscapeDataString("/" + currentUrl);

            var state = await AuthProvider.GetAuthenticationStateAsync();

            var uri = (state == null || state.User.Identity == null || !state.User.Identity.IsAuthenticated)
                ? $"/login?returnUrl={encodedUrl}"
                : "/account/accessdenied";

            _ = InvokeAsync(() =>
                NavigationManager.NavigateTo(uri, forceLoad: true, replace: true)
            );
        };

        private bool ShouldHideNavbar(RouteData routeData)
            => typeof(IHideNavbarPage).IsAssignableFrom(routeData.PageType);
    }
}
