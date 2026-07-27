using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using Store.Ai;
using Store.Components;
using Store.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// "products" is resolved by Aspire service discovery to the Products project.
builder.Services.AddHttpClient<ProductService>(client =>
    client.BaseAddress = new Uri("https+http://products"));

// --- AI services -------------------------------------------------------------

var aiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint. Set it with dotnet user-secrets.");
var aiKey = builder.Configuration["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Key. Set it with dotnet user-secrets.");

builder.Services.AddChatClient(
    new AzureOpenAIClient(new Uri(aiEndpoint), new AzureKeyCredential(aiKey))
        .GetChatClient("gpt-5-mini")
        .AsIChatClient());

builder.Services.AddScoped<ProductDiscovery>();
builder.Services.AddSingleton<SearchTelemetry>();

// Optional: the operations assistant runs on a local model when one is configured.
// Start Foundry Local, then set LocalModel:Endpoint and LocalModel:Model in user secrets.
var localEndpoint = builder.Configuration["LocalModel:Endpoint"];
var localModelName = builder.Configuration["LocalModel:Model"];

if (!string.IsNullOrWhiteSpace(localEndpoint) && !string.IsNullOrWhiteSpace(localModelName))
{
    builder.Services.AddKeyedChatClient(
        "local",
        new OpenAIClient(
                new ApiKeyCredential("not-used-by-a-local-model"),
                new OpenAIClientOptions { Endpoint = new Uri(localEndpoint) })
            .GetChatClient(localModelName)
            .AsIChatClient());

    builder.Services.AddScoped<OperationsAssistant>();
}

// -----------------------------------------------------------------------------

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
