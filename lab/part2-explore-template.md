# Explore the Template Code

## In this lab

In this lab, you'll explore the code structure of the AI Web Chat template. You'll learn about the different services configured in the .NET Aspire AppHost, understand the application configuration in the Web project, explore how `IChatClient` is configured and used, and dive into Microsoft Extensions for Vector Data.

## Services in .NET Aspire AppHost Program.cs

Let's start by examining the `Program.cs` file in the `GenAiLab.AppHost` project:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// You will need to set the connection string to your own value
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-API-KEY"
var openai = builder.AddConnectionString("openai");

var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var ingestionCache = builder.AddSqlite("ingestionCache");

var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app");
webApp.WithReference(openai);
webApp
    .WithReference(vectorDB)
    .WaitFor(vectorDB);
webApp
    .WithReference(ingestionCache)
    .WaitFor(ingestionCache);

builder.Build().Run();
```

Key components in the AppHost:

1. **OpenAI Connection**: Added as a connection string reference that will be passed to the web app
1. **Qdrant Vector Database**: Added as a containerized service with persistent storage
1. **SQLite Ingestion Cache**: Added as a lightweight database for caching ingestion state
1. **Web Application**: The main app that references all other services

## Application configuration in Web Program.cs

Now let's look at the `Program.cs` file in the `GenAiLab.Web` project:

```csharp
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using GenAiLab.Web.Components;
using GenAiLab.Web.Services;
using GenAiLab.Web.Services.Ingestion;
using OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var openai = builder.AddAzureOpenAIClient("openai");
openai.AddChatClient("gpt-4o-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
openai.AddEmbeddingGenerator("text-embedding-3-small");

builder.AddQdrantClient("vectordb");

builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();
builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.AddSqliteDbContext<IngestionCacheDbContext>("ingestionCache");

var app = builder.Build();
IngestionCacheDbContext.Initialize(app.Services);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.Run();
```

Key components in the Web Program.cs:

1. **Service Registration**: Setting up Razor components, service defaults, etc.
1. **GitHub Models Setup**:
   - Adding GitHub Models as the AI provider
   - Configuring a chat client with the "gpt-4o-mini" model
   - Setting up an embedding generator with "text-embedding-3-small" model
1. **Qdrant Client**: Connecting to the Qdrant vector database
1. **Service Implementations**:
   - Vector store
   - Data ingestor
   - Semantic search
   - Ingestion cache database context
1. **Data Ingestion**: Processing PDF files from the wwwroot/Data directory

## IChatClient configuration and use

The `IChatClient` interface is a key part of Microsoft Extensions for AI. Let's look at how it's configured and used:

```csharp
// Configuration in Program.cs
var openai = builder.AddGitHubModels();
openai.AddChatClient("gpt-4o-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
```

The `IChatClient` is used in the `Chat.razor` component to handle user messages and generate AI responses:

```csharp
@code {
    [Inject]
    private IChatClient ChatClient { get; set; } = default!;
    
    private async Task HandleUserMessageAsync(string userMessage)
    {
        // ...
        var response = await ChatClient.GetResponseAsync(
            SystemPrompt, 
            chatHistory.Select(m => new ChatMessage(m.Role, m.Content)).ToArray());
        // ...
    }
}
```

Key points about `IChatClient`:

1. It provides a consistent interface for interacting with any AI service
1. It abstracts away the specifics of different AI providers
1. It supports both one-off responses and conversation history
1. It enables function calling and other advanced features

## Microsoft Extensions for Vector Data in Web.Services.SemanticSearchRecord

The template uses Microsoft Extensions for Vector Data to implement semantic search. Let's look at the `SemanticSearchRecord.cs` file:

```csharp
namespace GenAiLab.Web.Services;

public class SemanticSearchRecord
{
    [VectorStoreRecordKey]
    public required Guid Key { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string FileName { get; set; }

    [VectorStoreRecordData]
    public int PageNumber { get; set; }

    [VectorStoreRecordData]
    public required string Text { get; set; }

    [VectorStoreRecordVector(1536, DistanceFunction.CosineSimilarity)] // 1536 is the default vector size for the OpenAI text-embedding-3-small model
    public ReadOnlyMemory<float> Vector { get; set; }
}
```

This class represents the data stored in the vector database with specific attributes for vector storage:

- `Key`: The unique identifier for the record, marked with `[VectorStoreRecordKey]`
- `FileName`: The source document's name, marked as filterable with `[VectorStoreRecordData(IsFilterable = true)]`
- `PageNumber`: The page number in the source document
- `Text`: A chunk of text from the document
- `Vector`: The embedding vector configured for the OpenAI text-embedding-3-small model's 1536 dimensions using cosine similarity

The `SemanticSearch.cs` file shows how these records are queried:

```csharp
public class SemanticSearch(
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    IVectorStore vectorStore,
    ILogger<SemanticSearch> logger)
{
    private const string CollectionName = "data-genailab-ingested";
    
    public async Task<SearchResults> Search(string query)
    {
        try
        {
            // Generate an embedding vector for the query
            var queryEmbedding = await embedder.GenerateEmbeddingVectorAsync(query);
            
            // Search the vector database for similar document chunks
            var collection = vectorStore.GetCollection<Guid, SemanticSearchRecord>(CollectionName);
            var searchResults = await collection.VectorizedSearchAsync(
                queryEmbedding,
                new VectorSearchOptions<SemanticSearchRecord> { Top = 5 }
            );
            
            // Process and return results
            var results = new List<DocumentResult>();
            await foreach (var match in searchResults.Results)
            {
                results.Add(new DocumentResult
                {
                    FileName = match.Record.FileName,
                    Text = match.Record.Text,
                    Score = match.Score
                });
            }
            
            return new SearchResults(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error performing semantic search");
            return new SearchResults(new List<DocumentResult>());
        }
    }
}
```

## Embedding model use in PDFDirectorySource.CreateRecordsForDocumentAsync

Let's examine how embeddings are generated during document ingestion in the `DataIngestor.cs` file:

```csharp
private async Task IngestFromSourceAsync(IIngestionSource source)
{
    // Create or get the vector collection
    var vectorCollection = _vectorStore.GetCollection<Guid, SemanticSearchRecord>(CollectionName);

    // Get documents from the source
    var documents = await source.GetDocumentsAsync();
    foreach (var document in documents)
    {
        // Split the content into chunks
        var chunks = SplitTextIntoChunks(document.Content, chunkSize: 1000);
        
        // For each chunk, create a record with embeddings
        foreach (var chunk in chunks)
        {
            // Skip empty chunks
            if (string.IsNullOrWhiteSpace(chunk)) continue;
            
            // Generate embedding for the chunk
            var embedding = await _embedder.GenerateEmbeddingVectorAsync(chunk);
            
            // Create and store the record
            var record = new SemanticSearchRecord
            {
                FileName = document.FileName,
                Text = chunk
            };
            
            // Add the record to the vector database
            await vectorCollection.UpsertAsync(Guid.NewGuid(), record, embedding.Data.ToArray());
        }
    }
}
```

Key steps in the embedding workflow:

1. Documents are retrieved from a source (like PDFs)
1. Each document is split into smaller chunks for better search precision
1. For each chunk, an embedding vector is generated using `IEmbeddingGenerator`
1. The text chunk and its embedding are stored in the vector database
1. During search, query text is also converted to an embedding, and vector similarity is used to find relevant chunks

## What You've Learned

- How services are configured and orchestrated in .NET Aspire
- How the main application is structured and configured
- How `IChatClient` is set up and used for interacting with AI models
- How vector embeddings are used to implement semantic search
- How document ingestion works with embeddings and vector storage

## Next Steps

Now that you understand the code structure of the template, proceed to [Convert from GitHub Models to Azure OpenAI](part3-azure-openai.md) to learn how to migrate your application from using GitHub Models to Azure OpenAI for production use.
