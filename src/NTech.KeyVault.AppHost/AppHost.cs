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
builder.AddProject<Projects.NTech_KeyVault_Api>("ntech-keyvault-api")
    .WithReference(database)
    .WaitFor(postgres)
    .WaitFor(database);

builder.Build().Run();
