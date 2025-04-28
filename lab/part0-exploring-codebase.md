# Exploring the Codebase in a new AI Web Chat project

## In this lab

In this lab, you will explore the structure and components of a new AI Web Chat project created with the .NET AI project templates. You'll gain an understanding of how the different parts of the application work together to create an intelligent chatbot.

## Project Structure

The solution consists of three main projects:

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

The AppHost configuration does several key things:

1. **Connection String Resource**: Creates a resource for Azure OpenAI that can be used by other services

   ```csharp
   var openai = builder.AddConnectionString("openai");
   ```

   This sets up a connection string for the Azure OpenAI service that will be used for AI operations.

2. **Qdrant Vector Database**: Sets up a Qdrant vector database with persistent storage

   ```csharp
   var vectorDB = builder.AddQdrant("vectordb")
      .WithDataVolume()
      .WithLifetime(ContainerLifetime.Persistent);
   ```

   The `.WithDataVolume()` ensures data persists between runs, while `ContainerLifetime.Persistent` keeps the container running even after the AppHost shuts down.

3. **SQLite Database**: Adds a database for caching ingestion data

   ```csharp
   var ingestionCache = builder.AddSqlite("ingestionCache");
   ```

   This database will store metadata about ingested documents to avoid reprocessing them multiple times.

4. **Web Application Configuration**:

   ```csharp
   var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app");
   webApp.WithReference(openai);
   webApp
       .WithReference(vectorDB)
       .WaitFor(vectorDB);
   webApp
       .WithReference(ingestionCache)
       .WaitFor(ingestionCache);
   ```

   This sets up the main web application with references to all three resources and configures dependencies to ensure the web app waits for the databases to be ready before starting.

## The NuGet Packages in GenAiLab.Web

Next, let's examine the NuGet packages in the **GenAiLab.Web.csproj** file:

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

These packages can be grouped into several categories:

1. **Microsoft Extensions for AI (MEAI) Libraries**:
   - `Microsoft.Extensions.AI` (9.4.0-preview.1.25207.5) - The core library for AI integration in .NET
   - `Microsoft.Extensions.AI.OpenAI` (9.4.0-preview.1.25207.5) - AI integration with OpenAI models

2. **.NET Aspire Integration Packages**:
   - `Aspire.Azure.AI.OpenAI` (9.1.0-preview.1.25121.10) - Simplifies integration with Azure OpenAI in .NET Aspire applications
   - `CommunityToolkit.Aspire.Microsoft.EntityFrameworkCore.Sqlite` (9.3.1-beta.260) - Provides SQLite integration for .NET Aspire
   - `Aspire.Qdrant.Client` (9.1.0) - Integrates Qdrant vector database with .NET Aspire

3. **Semantic Kernel Components**:
   - `Microsoft.SemanticKernel.Core` (1.45.0) - Core components for building semantic applications
   - `Microsoft.SemanticKernel.Connectors.Qdrant` (1.45.0-preview) - Connects Semantic Kernel with Qdrant vector database

4. **PDF Processing**:
   - `PdfPig` (0.1.9) - Library for parsing and extracting text from PDF documents

5. **Utility Libraries**:
   - `System.Linq.Async` (6.0.1) - LINQ extensions for asynchronous operations

These packages together provide the foundation for building an AI-powered chatbot with vector search capabilities.

## Web Application Setup and PDF Ingestion

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

var app = builder.Build();
IngestionCacheDbContext.Initialize(app.Services);

// ... Standard ASP.NET Core middleware configuration ...

// PDF Ingestion
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.Run();
```

Key aspects of this configuration:

1. **Azure OpenAI Client Configuration**:

   ```csharp
   var openai = builder.AddAzureOpenAIClient("openai");
   openai.AddChatClient("gpt-4o-mini")
       .UseFunctionInvocation()
       .UseOpenTelemetry(configure: c =>
           c.EnableSensitiveData = builder.Environment.IsDevelopment());
   openai.AddEmbeddingGenerator("text-embedding-3-small");
   ```

   This configures:
   - The Azure OpenAI client, referencing the connection string set up in AppHost
   - A chat model (`gpt-4o-mini`) with function calling capabilities
   - OpenTelemetry for monitoring and diagnostics
   - An embedding model (`text-embedding-3-small`) for creating vector embeddings

2. **Vector Database Client**:

   ```csharp
   builder.AddQdrantClient("vectordb");
   ```

   This adds the Qdrant client to access the vector database.

3. **Service Registration**:

   ```csharp
   builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();
   builder.Services.AddScoped<DataIngestor>();
   builder.Services.AddSingleton<SemanticSearch>();
   builder.AddSqliteDbContext<IngestionCacheDbContext>("ingestionCache");
   ```

   These lines register:
   - `QdrantVectorStore` as the implementation of `IVectorStore`
   - `DataIngestor` for processing PDF files
   - `SemanticSearch` for performing vector searches
   - `IngestionCacheDbContext` for tracking document processing state

4. **PDF Ingestion Process**:

   ```csharp
   await DataIngestor.IngestDataAsync(
       app.Services,
       new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));
   ```

   This critical code:
   - Calls the static `IngestDataAsync` method of the `DataIngestor` class
   - Creates a `PDFDirectorySource` that points to the `wwwroot/Data` directory
   - Starts the process of extracting text from PDF files, creating vector embeddings, and storing them

## PDF Ingestion Flow

The PDF ingestion process follows these steps:

1. **PDF Location**: The application looks for PDF files in the `wwwroot/Data` directory
2. **Text Extraction**: Using PdfPig, it extracts text content from each PDF file
3. **Text Chunking**: The text is broken into smaller chunks suitable for embedding
4. **Vector Generation**: The OpenAI embedding model converts each text chunk into a vector
5. **Vector Storage**: The vectors are stored in the Qdrant database along with metadata linking back to the source document
6. **Caching**: The SQLite database tracks which documents have been processed to avoid duplication

The application includes two example PDF files in the `wwwroot/Data` directory:

- `Example_Emergency_Survival_Kit.pdf`
- `Example_GPS_Watch.pdf`

## Architecture Overview

The application uses a Retrieval Augmented Generation (RAG) architecture:

1. **User Query**: The user submits a question through the chat interface
2. **Vector Embedding**: The query is converted to a vector embedding
3. **Semantic Search**: The vector is used to find similar content in the Qdrant database
4. **Context Augmentation**: Relevant document content is retrieved and included as context
5. **AI Response Generation**: The LLM generates a response based on the query and retrieved context
6. **Response Display**: The answer is presented to the user in the chat interface

This approach allows the chatbot to provide responses that incorporate information from the ingested PDF documents, making it more accurate and useful within the specific knowledge domain.

## Hands-on Exercise

1. Open the solution in Visual Studio
2. Explore the AppHost configuration in `GenAiLab.AppHost/Program.cs`
3. Review the web application setup in `GenAiLab.Web/Program.cs`
4. Examine the PDF ingestion process by looking at `DataIngestor.cs` and `PDFDirectorySource.cs` in the `Services/Ingestion` folder
5. Try asking questions about the content in the PDF files once the application is running

## Next Steps

After exploring the codebase, proceed to [Vector data and embeddings](part3-vector-data.md) to learn more about how vector embeddings are used for semantic search.
