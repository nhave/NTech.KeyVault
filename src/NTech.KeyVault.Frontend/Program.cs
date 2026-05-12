using Microsoft.AspNetCore.Authentication.Cookies;
using NTech.KeyVault.Frontend.Components;
using NTech.KeyVault.Frontend.Services;

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

        var apiBaseAddress = builder.Configuration["Api:Host"];
        if (string.IsNullOrEmpty(apiBaseAddress))
            throw new InvalidOperationException("API base address is not configured.");

        builder.Services.AddScoped<ITokenStore, TokenStore>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddTransient<TokenAuthorizationHandler>();
        builder.Services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
        })
        .AddHttpMessageHandler<TokenAuthorizationHandler>();

        builder.Services.AddTransient<VaultApplicationHandler>();
        builder.Services.AddHttpClient("VaultApplication", client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress);
        })
        .AddHttpMessageHandler<VaultApplicationHandler>();

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
