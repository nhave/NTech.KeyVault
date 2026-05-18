var builder = DistributedApplication.CreateBuilder(args);

// Add a PostgreSQL database to the application with the specified configuration.
var postgres = builder.AddPostgres("keyvault-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithPgWeb();

// Add a database to the PostgreSQL service with the name "PostgresDb".
var database = postgres.AddDatabase("PostgresDb");

// Add the "ntech-keyvault-api" project to the application,
// specifying its reference to the database and its dependencies on the PostgreSQL service and the database.
var api = builder.AddProject<Projects.NTech_KeyVault_Api>("API")
    .WithReference(database)
    .WaitFor(postgres)
    .WaitFor(database)
    .WithIconName("CloudArrowUp");

// Add the "ntech-keyvault-blazor-web" project to the application,
var blazor = builder.AddProject<Projects.NTech_KeyVault_Frontend>("Blazor-Web")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("Api__BaseUrl", api.GetEndpoint("https"))
    .WithIconName("Globe");

// Add the "ntech-keyvault-blazor-maui" project to the application,
//builder.AddProject<Projects.NTech_KeyVault_Blazor_Maui>("Blazor-MAUI")
//    .WithParentRelationship(blazor)
//    .WithExplicitStart()
//    .WithIconName("PhoneDesktop");

builder.Build().Run();
