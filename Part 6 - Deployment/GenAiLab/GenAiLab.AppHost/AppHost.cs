var builder = DistributedApplication.CreateBuilder(args);

// You will need to set the connection string to your own value
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
// Primary: Azure AI Foundry (Azure OpenAI)
//   dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://YOUR_RESOURCE_NAME.openai.azure.com/;Key=YOUR_API_KEY"
// Legacy fallback (GitHub Models, retiring 2026-07-30):
//   dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-GITHUB-TOKEN"
var openai = builder.AddConnectionString("openai");

var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app");
webApp.WithReference(openai);
webApp
    .WithReference(vectorDB)
    .WaitFor(vectorDB);

webApp.WithExternalHttpEndpoints();

builder.Build().Run();
