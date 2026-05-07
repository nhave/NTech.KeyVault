using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NTech.KeyVault.Blazor.Maui.Services;
using NTech.KeyVault.Blazor.Shared.Services;

namespace NTech.KeyVault.Blazor.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the NTech.KeyVault.Blazor.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<LoginService>();
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddHttpClient("dynamic");
        builder.Services.AddSingleton<ApiClientFactory>();

        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IActiveServerService, ActiveServerService>();

        builder.Services.AddScoped<MauiAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<MauiAuthStateProvider>());
        builder.Services.AddAuthorizationCore();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
