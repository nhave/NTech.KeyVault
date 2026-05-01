
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Api.Extensions;
using NTech.KeyVault.Api.Middlewares;
using Swagger.Bootstrap;
using System.Text;

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
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var Jwt = builder.Configuration.GetSection("Jwt");
                var Issuer = Jwt["Issuer"];
                var Audience = Jwt["Audience"];
                var Secret = Jwt["Secret"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret!))
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<AppDbContext>();

            // Configure repositories and services
            builder.Services.AddApiRepositories();
            builder.Services.AddApiServices();

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

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insert your token here. A token can be obtained from \"/auth\" using a username and a password or from \"/auth/refresh\" using a token issued by the server.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            builder.Services.AddSwaggerBootstrap(options =>
            {
                options.UseExperimentalFeatures = true;
                options.UseAuthentication = true;
            });

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
            app.UseAuthentication();
            app.UseAuthorization();

            // Global exception handling middleware
            app.UseMiddleware<ApiExceptionMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
