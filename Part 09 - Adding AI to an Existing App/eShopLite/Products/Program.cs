using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Products.Ai;
using Products.Data;
using Products.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProductDataContext>(options =>
    options.UseSqlite("Data Source=catalog.db"));

// --- AI services -------------------------------------------------------------

var aiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint. Set it with dotnet user-secrets.");
var aiKey = builder.Configuration["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Key. Set it with dotnet user-secrets.");

var azureClient = new AzureOpenAIClient(new Uri(aiEndpoint), new AzureKeyCredential(aiKey));

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
    azureClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());

builder.Services.AddSingleton<SqliteVectorStore>(sp =>
    new SqliteVectorStore(
        "Data Source=vectors.db",
        new SqliteVectorStoreOptions
        {
            EmbeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
        }));

builder.Services.AddSingleton<VectorStoreCollection<int, ProductVector>>(sp =>
    sp.GetRequiredService<SqliteVectorStore>().GetCollection<int, ProductVector>("products"));

builder.Services.AddSingleton<ProductSemanticSearch>();

// -----------------------------------------------------------------------------

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapProductEndpoints();

// Create and seed the catalog database on first run, then build the search index from it.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductDataContext>();
    context.Database.EnsureCreated();
    SeedData.Initialize(context);

    var semanticSearch = app.Services.GetRequiredService<ProductSemanticSearch>();
    await semanticSearch.InitializeAsync(context.Product.ToList());
}

app.Run();
