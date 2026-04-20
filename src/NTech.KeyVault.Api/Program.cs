
using Microsoft.OpenApi;
using Swagger.Bootstrap;

namespace NTech.KeyVault.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Add services to the container.

            builder.Services.AddAntiforgery();
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication();

            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "NTech.KeyVault.Api",
                    Description = "A simple example ASP.NET Core Web API",
                });
            });
            builder.Services.AddSwaggerBootstrap();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerBootstrap();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();
            app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();

            app.Run();
        }
    }
}
