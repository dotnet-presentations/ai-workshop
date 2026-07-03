# Part 4: Template reveal — scaffold `aichatweb` and understand the code

You've now built a chat app (Part 2) and a RAG loop (Part 3) **by hand**. You know
what an `IChatClient` is, what an embedding is, what a vector search does, and why
an in-memory store doesn't scale.

Now for the payoff. In this part you scaffold the **`aichatweb` template** — and
discover it generates everything you just wrote, wired into a real web app with a
persistent vector database and orchestration. The difference from the old workshop:
you now read the generated code **with comprehension** instead of taking it on
faith.

> **Aspire enters here**, motivated by a real need: your Part 3 vectors lived in
> memory and vanished on exit. A production app needs a persistent, orchestrated
> vector database.

> RAG evaluation (the "RAG triad": groundedness, relevance, context quality) is a
> great optional enrichment for this part — see
> [Steve Sanderson's dotnet-ai-workshop, Ch 6](https://github.com/SteveSandersonMS/dotnet-ai-workshop). Reference and adapt with attribution.

## Prerequisites

- Completed [Part 2](../Part%202%20-%20Build%20Chat%20App/README.md) and [Part 3](../Part%203%20-%20Add%20RAG/README.md)
- **Docker Desktop** (or Podman) running — the template uses a Qdrant container
- Azure AI Foundry with `gpt-4.1-mini` + `text-embedding-3-small` (see [Part 1](../Part%201%20-%20Setup/README.md))

## Step 1: Install the template and scaffold

```bash
dotnet new install Microsoft.Extensions.AI.Templates
dotnet new aichatweb --provider azureopenai --vector-store qdrant --aspire --name GenAiLab
```

This generates a solution with three projects:

| Project | Role |
| --- | --- |
| `GenAiLab.Web` | The Blazor chat web app |
| `GenAiLab.AppHost` | The **.NET Aspire** orchestrator (starts the app + Qdrant) |
| `GenAiLab.ServiceDefaults` | Shared telemetry, health checks, resilience |

## Step 2: Map the generated code to what you built by hand

Everything below is code the template wrote — and you now recognize all of it.

### Chat client: your Part 2 pipeline, as DI configuration

`GenAiLab.Web/Program.cs`:

```csharp
var openai = builder.AddAzureOpenAIClient("openai");
openai.AddChatClient("gpt-4.1-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
openai.AddEmbeddingGenerator("text-embedding-3-small");
```

| Part 2/3 (by hand) | Template (generated) |
| --- | --- |
| `new AzureOpenAIClient(endpoint, key).GetChatClient(...).AsIChatClient()` | `builder.AddAzureOpenAIClient("openai").AddChatClient(...)` |
| `.AsBuilder().UseLogging(...).Build()` | `.UseFunctionInvocation().UseOpenTelemetry(...)` |
| `azureClient.GetEmbeddingClient(...).AsIEmbeddingGenerator()` | `openai.AddEmbeddingGenerator("text-embedding-3-small")` |

Same abstractions — now registered in DI and wrapped with a richer middleware
pipeline (function calling + telemetry instead of your hand-added logging).

### Retrieval: your Part 3 cosine search, as a service

`GenAiLab.Web/Services/SemanticSearch.cs`:

```csharp
public class SemanticSearch(VectorStoreCollection<Guid, IngestedChunk> vectorCollection)
{
    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk> { /* ... */ });
        return await nearest.Select(result => result.Record).ToListAsync();
    }
}
```

| Part 3 (by hand) | Template (generated) |
| --- | --- |
| Your `CosineSimilarity(...)` loop over a `List` | `vectorCollection.SearchAsync(...)` on a real vector store |
| Manual top-k `.OrderByDescending().Take(k)` | `maxResults` argument |
| No filtering | `Filter` by document id |

You wrote the math; the template delegates it to Qdrant through the
`Microsoft.Extensions.VectorData` abstraction — the **same swap** idea, one more
level up (swap the vector store).

### Ingestion: your Part 3 chunk-and-embed step, as a pipeline

`GenAiLab.Web/Services/Ingestion/DataIngestor.cs` upserts chunks and documents into
Qdrant collections, tracking what's already ingested so it only processes new or
changed files. `PDFDirectorySource` reads PDFs from `wwwroot/Data`.

| Part 3 (by hand) | Template (generated) |
| --- | --- |
| Split doc into paragraph chunks | `PDFDirectorySource` + chunking |
| `GenerateAsync(chunks)` once at startup | `DataIngestor.IngestDataAsync(...)` with change tracking |
| `store.Add((text, vector))` in a `List` | `chunksCollection.UpsertAsync(...)` into Qdrant |

`Program.cs` kicks it off at startup:

```csharp
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));
```

> ⚠️ Only ingest **trusted** content — ingested text is reflected back to users and
> is a prompt-injection risk.

### Persistence + orchestration: the answer to "in-memory doesn't scale"

`GenAiLab.AppHost/AppHost.cs`:

```csharp
var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app");
webApp.WithReference(openai);
webApp.WithReference(vectorDB).WaitFor(vectorDB);
```

This is the direct fix for Part 3's limitation: the vectors now live in a **Qdrant
container with a persistent data volume**, and Aspire starts the database, waits
for it to be ready, then starts the web app — wiring the connection strings between
them automatically.

## Step 3: The Aspire dashboard

Run the AppHost:

```bash
cd GenAiLab
dotnet run --project GenAiLab.AppHost
```

Aspire launches a **dashboard** (URL printed in the console) showing every service,
its health, logs, traces, and metrics. This is why `UseOpenTelemetry(...)` was in
`Program.cs` — the telemetry you saw registered now has somewhere to go. Open the
web app from the dashboard and chat with your ingested documents.

## Step 4: Configure secrets

The AppHost reads the Azure connection string from user-secrets (same secrets-first
rule as Parts 2–3):

```bash
dotnet user-secrets --project GenAiLab.AppHost set ConnectionStrings:openai "Endpoint=https://YOUR-RESOURCE.openai.azure.com/;Key=YOUR-KEY"
```

## What you learned

You can now open **any** generated file and explain it, because you built each
concept by hand first:

- `IChatClient` + middleware → Part 2
- `IEmbeddingGenerator`, chunking, vector search, prompt augmentation → Part 3
- Qdrant + Aspire → the persistence and orchestration your hand-built version lacked

## What's next

In **Part 5** you'll make the provider story explicit: Azure AI Foundry as the
primary, with Foundry Local / Ollama / GitHub Models (legacy) as swap-in
alternatives — the payoff of everything running on `IChatClient` and
`IEmbeddingGenerator`.
