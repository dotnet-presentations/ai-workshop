var builder = DistributedApplication.CreateBuilder(args);

// The workshop uses an existing Azure OpenAI resource, supplied through the
// "openai" connection string in user secrets. Swap this back to the template's
// builder.AddAzureOpenAI("openai") if you want Aspire to provision one for you.
var openai = builder.AddConnectionString("openai");

var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var markitdown = builder.AddContainer("markitdown", "mcp/markitdown")
    .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001")
    .WithHttpEndpoint(targetPort: 3001, name: "http");

var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app")
    .WithExternalHttpEndpoints();
webApp
    .WithReference(openai)
    .WaitFor(openai);
webApp
    .WithReference(vectorDB)
    .WaitFor(vectorDB);
webApp
    .WithEnvironment("MARKITDOWN_MCP_URL", markitdown.GetEndpoint("http"));

builder.Build().Run();
