# Exploring the Codebase in a new AI Web Chat project

## In this lab

In this lab, you will explore the structure and components of a new AI Web Chat project created with the .NET AI project templates. You'll gain an understanding of how the different parts of the application work together to create an intelligent chatbot. We'll focus on the AppHost configuration, required NuGet packages, and the PDF ingestion process.

## Project Structure

The solution consists of three projects:

- **GenAiLab.Web**: The main web application that hosts the chatbot interface
- **GenAiLab.AppHost**: The .NET Aspire application host that orchestrates the different services
- **GenAiLab.ServiceDefaults**: Common service configuration settings for all services in the application

## Exploring the .NET Aspire AppHost Configuration

Let's first look at the `Program.cs` file in the **GenAiLab.AppHost** project:

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

This configuration file does several important things:

1. Creates a connection string resource for Azure OpenAI that can be used by other services
2. Sets up a Qdrant vector database with persistent storage for vector embeddings
3. Adds a SQLite database for caching ingestion data
4. Configures the main web application with references to these resources
5. Sets up dependencies so the web app waits for the databases to be ready before starting

## Required NuGet Packages

Next, let's examine the NuGet packages in the **GenAiLab.Web** project:

```xml
<ItemGroup>
  <PackageReference Include="Aspire.Azure.AI.OpenAI" Version="9.1.0-preview.1.25121.10" />
  <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.4.0-preview.1.25207.5" />
  <PackageReference Include="CommunityToolkit.Aspire.Microsoft.EntityFrameworkCore.Sqlite" Version="9.3.1-beta.260" />
  <PackageReference Include="Microsoft.Extensions.AI" Version="9.4.0-preview.1.25207.5" />
  <PackageReference Include="Microsoft.SemanticKernel.Core" Version="1.45.0" />
  <PackageReference Include="PdfPig" Version="0.1.9" />
  <PackageReference Include="System.Linq.Async" Version="6.0.1" />
  <PackageReference Include="Aspire.Qdrant.Client" Version="9.1.0" />
  <PackageReference Include="Microsoft.SemanticKernel.Connectors.Qdrant" Version="1.45.0-preview" />
</ItemGroup>
```

These packages provide:

1. **Microsoft.Extensions.AI** - Core library for AI integration in .NET
2. **Microsoft.Extensions.AI.OpenAI** - AI integration with OpenAI services
3. **Aspire.Azure.AI.OpenAI** - .NET Aspire integration with Azure OpenAI
4. **Microsoft.SemanticKernel.Core** - Components for AI orchestration
5. **Aspire.Qdrant.Client** - Client for the Qdrant vector database
6. **Microsoft.SemanticKernel.Connectors.Qdrant** - Qdrant integration with Semantic Kernel
7. **PdfPig** - For parsing and extracting text from PDF documents
8. **System.Linq.Async** - LINQ extensions for asynchronous operations
9. **CommunityToolkit.Aspire.Microsoft.EntityFrameworkCore.Sqlite** - SQLite integration for .NET Aspire

## The Web Application Setup

Now let's look at the `Program.cs` file in the **GenAiLab.Web** project:

```csharp
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

// ... more code ...
```

This configuration:

1. Sets up the Azure OpenAI client with a chat model (gpt-4o-mini) and embedding model (text-embedding-3-small)
2. Configures the Qdrant vector database client
3. Registers services for vector storage, data ingestion, and semantic search
4. Sets up the SQLite database context for ingestion caching

## PDF Ingestion Process

At the end of the `Program.cs` file in **GenAiLab.Web**, there's code for ingesting PDF data:

```csharp
// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));
```

This code:

1. Calls the `IngestDataAsync` method of the `DataIngestor` class
2. Passes a `PDFDirectorySource` that points to the `/wwwroot/Data` directory
3. This process extracts text from PDF files, creates vector embeddings, and stores them in the Qdrant vector database
4. These embeddings will be used later for semantic search to find relevant information when answering user queries

The application includes two example PDF files in the `wwwroot/Data` directory:

- Example_Emergency_Survival_Kit.pdf
- Example_GPS_Watch.pdf

## Architecture Overview

The application uses a modern architecture with:

1. **Blazor web interface**: For the chat UI
2. **Azure OpenAI integration**: For generating chat responses
3. **Vector database (Qdrant)**: For storing and querying document embeddings
4. **Retrieval Augmented Generation (RAG) pattern**: To enhance AI responses with relevant document content

This approach allows the chatbot to provide responses that incorporate information from the ingested PDF documents, making it more accurate and useful within the specific knowledge domain.

## Hands-on Exercise

1. Open the solution in Visual Studio
2. Explore the AppHost configuration in `GenAiLab.AppHost/Program.cs`
3. Review the web application setup in `GenAiLab.Web/Program.cs`
4. Examine the PDF ingestion process by looking at `DataIngestor.cs` and `PDFDirectorySource.cs`
5. Start the application using the AppHost project as the startup project
6. Try asking questions about the content in the PDF files

## Next Steps

After exploring the codebase, proceed to [Vector data and embeddings](part3-vector-data.md) to learn more about how vector embeddings are used for semantic search.
