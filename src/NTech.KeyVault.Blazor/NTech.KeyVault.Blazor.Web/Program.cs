using NTech.KeyVault.Blazor.Web.Components;
using NTech.KeyVault.Blazor.Shared.Services;
using NTech.KeyVault.Blazor.Web.Services;
using NTech.KeyVault.Blazor.Web.Extensions;

namespace NTech.KeyVault.Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add device-specific services used by the NTech.KeyVault.Blazor.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        // Add authentication services
        builder.Services.AddAuthServices(builder.Configuration);

        // Add services used by the NTech.KeyVault.Blazor.Web project
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<LoginService>();

        builder.Services.AddHttpClient("dynamic");
        builder.Services.AddSingleton<ApiClientFactory>();

        builder.Services.AddScoped<ITokenStore, TokenStore>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IActiveServerService, ActiveServerService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(
                typeof(NTech.KeyVault.Blazor.Shared._Imports).Assembly);

        app.Run();
    }
}
