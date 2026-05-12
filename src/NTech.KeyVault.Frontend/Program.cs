using Microsoft.AspNetCore.Authentication.Cookies;
using NTech.KeyVault.ClientServices.Abstractions;
using NTech.KeyVault.ClientServices.Services;
using NTech.KeyVault.Frontend.Components;
using NTech.KeyVault.Frontend.Services;
using System.Buffers.Text;

namespace NTech.KeyVault.Frontend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddBlazorBootstrap();

        CookieSecurePolicy securePolicy = CookieSecurePolicy.Always;

        // Add authentication with cookie scheme
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
                options.Cookie.Name = "NTech.KeyVault.Auth";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = securePolicy;
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/login";
                options.SlidingExpiration = true;
            });

        // Add authorization services
        builder.Services.AddAuthorization();

        // Get the API base address from configuration
        var apiBaseAddress = builder.Configuration["Api:Host"];
        if (string.IsNullOrEmpty(apiBaseAddress))
            throw new InvalidOperationException("API base address is not configured.");

        // Register HttpContextAccessor and TokenContext to allow the api services to access the current user's token
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITokenContext, BlazorTokenContext>();

        // Register the VaultApplicationHandler and configure the HttpClient for ITokenStore to use it
        builder.Services.AddTransient<VaultApplicationHandler>();
        builder.Services.AddHttpClient<ITokenStore, TokenStore>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
        })
        .AddHttpMessageHandler<VaultApplicationHandler>();

        // Register API services with the base address
        builder.Services.AddApiServices(apiBaseAddress);

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
        app.UseMiddleware<VaultTokenValidationMiddleware>();
        app.UseAuthorization();

        app.MapControllers();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
