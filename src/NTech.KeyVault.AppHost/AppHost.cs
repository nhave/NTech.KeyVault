var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.NTech_KeyVault_Api>("ntech-keyvault-api");

builder.Build().Run();
