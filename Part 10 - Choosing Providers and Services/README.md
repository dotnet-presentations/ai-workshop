# Part 10: Choosing Providers and Services

> **⏱️ Estimated Time:** 20-30 minutes

You are about to deploy. Before you do, there is a decision to make that you have
been able to postpone all day: **which model provider does this application
actually run against in production?**

Everything you have written so far talks to `IChatClient` and
`IEmbeddingGenerator`, not to Azure OpenAI. That indirection has been quietly
doing a job, and this is where it pays off: **swap the provider, keep the same
interfaces.** The chat loop from Part 2, the RAG loop from Part 3, the template
app from Part 4, and the capstone from Part 9 all run against a different
provider with no application code changes. The only thing that changes is
*registration* — an endpoint, a key, a model name.

That matters at deployment time for reasons that are not academic: cost per
token, data residency, whether a feature is allowed to call the cloud at all,
and what happens in a demo when the conference wifi drops.

This part and Part 11 form one deployment block. If the workshop schedule does
not include deployment, skip both parts and protect the Part 9 capstone time.

You will return to the `GenAiLab` project you created in Part 4 and prepare it for
production deployment. If you do not have that project, use the completed
[`Part 11 - Deployment/GenAiLab`](../Part%2011%20-%20Deployment/GenAiLab/) snapshot,
or scaffold the deployment variant directly:

```bash
dotnet new aichatweb --provider azureopenai --vector-store azureaisearch --aspire --name GenAiLab --output GenAiLab
```

If you scaffold this variant, follow Part 4 Steps 2.1-2.4, but do not run its
three Qdrant-specific package commands. Use these Azure AI Search packages
instead:

```bash
dotnet add GenAiLab.AppHost package Aspire.Hosting.Azure.Search --version 13.4.6
dotnet add GenAiLab.Web package Aspire.Azure.Search.Documents --version 13.4.6
dotnet add GenAiLab.Web package CommunityToolkit.VectorData.AzureAISearch --version 1.0.0
```

Then skip the Qdrant replacement steps below because the template has already
generated the Azure AI Search wiring.

## The three providers

| Provider | Best for | Chat | Embeddings | Runs offline | Notes |
| --- | --- | :---: | :---: | :---: | --- |
| **[Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry)** (Azure OpenAI) | **Primary**, the workshop default | ✅ | ✅ | ❌ | `gpt-5-mini` + `text-embedding-3-small` |
| **Foundry Local** | Local **chat** on your device | ✅ | ⏳ | ✅ | OpenAI-compatible local server; SLMs (Phi, Qwen, …). Embedding support is being evaluated (#496) |
| **Ollama** | Fully **offline RAG** (chat + embeddings) | ✅ | ✅ | ✅ | e.g. `llama3.2` for chat, `all-minilm` for embeddings |

## Provider 1: [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) (primary)

This is what Parts 2-4 already use: the Azure-specific client, adapted to `IChatClient`:

```csharp
using Azure;
using Azure.AI.OpenAI;

var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
IChatClient chat = client.GetChatClient("gpt-5-mini").AsIChatClient();
IEmbeddingGenerator<string, Embedding<float>> embeddings =
    client.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();
```

## The universal pattern for everything else

Foundry Local and Ollama both expose an **OpenAI-compatible** endpoint. That means
both use the *same* client, `OpenAIClient`, pointed at a different base URL and
key:

```csharp
using OpenAI;
using System.ClientModel;

var client = new OpenAIClient(
    new ApiKeyCredential(key),
    new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

IChatClient chat = client.GetChatClient(modelId).AsIChatClient();
```

Notice: this is the **same three lines** regardless of provider. Only `key`,
`baseUrl`, and `modelId` change, and they live in configuration, not code.

> [!NOTE]
> This pattern is not limited to the three providers in this workshop. Any provider that exposes an `IChatClient` implementation, or can be adapted to one, can fit the same application code. For example, [Microsoft.Extensions.AI.OpenAI](https://www.nuget.org/packages/Microsoft.Extensions.AI.OpenAI) provides adapters such as `AsIChatClient(...)` for OpenAI clients, and you can browse community-supported implementations through the [NuGet `IChatClient` tag search](https://www.nuget.org/packages?q=Tags%3A%22ichatclient%22).

## Provider 2: Foundry Local (offline chat)

[Foundry Local](https://learn.microsoft.com/azure/foundry-local/) runs
Microsoft-curated small language models entirely on-device, with no Azure
subscription, no network, no per-token cost.

```bash
dotnet add package Microsoft.AI.Foundry.Local   # or ...Local.WinML on Windows
dotnet add package OpenAI
```

The manager downloads a model and starts a local OpenAI-compatible web service;
you then use the **universal pattern** against it:

```csharp
using Microsoft.AI.Foundry.Local;

// Start Foundry Local and load a small model (abbreviated - see docs link below).
var mgr = FoundryLocalManager.Instance;
var model = await (await mgr.GetCatalogAsync()).GetModelAsync("qwen2.5-0.5b");
await model.DownloadAsync(_ => { });
await model.LoadAsync();
await mgr.StartWebServiceAsync();

// Same universal pattern - just a local endpoint and a throwaway key:
var client = new OpenAIClient(
    new ApiKeyCredential("notneeded"),
    new OpenAIClientOptions { Endpoint = new Uri(config.Web.Urls + "/v1") });
IChatClient chat = client.GetChatClient(model.Id).AsIChatClient();
```

Drop this `chat` into your Part 2 loop to run it offline. See the
[Foundry Local + inference SDK guide](https://learn.microsoft.com/azure/foundry-local/how-to/how-to-integrate-with-inference-sdks) for the full manager setup.

> Foundry Local's curated catalog is currently focused on chat/SLMs. Whether it
> serves **embeddings** for a fully-local RAG path is being evaluated (#496). For
> guaranteed offline RAG today, use Ollama below.

## Provider 3: Ollama (fully-offline RAG)

[Ollama](https://ollama.com) serves both chat and embedding models on an
OpenAI-compatible endpoint (`http://localhost:11434/v1`), so it covers the **whole
Part 3 RAG loop** offline.

```bash
ollama pull llama3.2       # chat
ollama pull all-minilm     # embeddings
```

```csharp
var client = new OpenAIClient(
    new ApiKeyCredential("ollama"),  // Ollama ignores the key
    new OpenAIClientOptions { Endpoint = new Uri("http://localhost:11434/v1") });

IChatClient chat = client.GetChatClient("llama3.2").AsIChatClient();
IEmbeddingGenerator<string, Embedding<float>> embeddings =
    client.GetEmbeddingClient("all-minilm").AsIEmbeddingGenerator();
```

Swap these two into your Part 3 project and the entire embed -> store -> search ->
augment loop runs with no cloud dependency.

## Takeaway

| What changed between providers | Where it lives |
| --- | --- |
| Endpoint URL | configuration (user-secrets) |
| API key | configuration (user-secrets) |
| Model name | configuration (user-secrets) |
| **Your app code** | **unchanged** |

That is the main point of `Microsoft.Extensions.AI`: your chat loop, your RAG
pipeline, and the template app are all written against `IChatClient` and
`IEmbeddingGenerator`, so the provider becomes a deployment decision.

## The other decision: supporting services

The model provider is not the only thing you pick before you deploy. A RAG
application also needs somewhere to keep its vectors, and the same argument
applies: your code depends on `Microsoft.Extensions.VectorData`, not on a
specific store.

| Vector store | Best for | Trade-off |
| --- | --- | --- |
| **Qdrant in a container** (fallback) | Local development, and deployments where you want to own the data | Cheap and portable, but you run and back up the container |
| **The template's local SQLite store** | The Docker-free path, and quick experiments | No infrastructure at all, but not a production store |
| **[Azure AI Search](https://learn.microsoft.com/azure/search/vector-search-overview)** (recommended for deployment) | Production apps that want a managed service, an SLA, and hybrid keyword + vector search | Billed per service hour even when idle |

## Prepare the production vector store

The morning project used Qdrant because it is easy to run locally and makes
Aspire orchestration visible. For the production deployment, replace that
container with Azure AI Search. Aspire and `azd` will provision one Search
service; the application creates its vector index during ingestion.

You need an Azure subscription where you can create Azure AI Search and role
assignments. If your workshop account cannot do that, use the Qdrant fallback in
Step 4.

> [!TIP]
> The completed `Part 11 - Deployment/GenAiLab` snapshot already contains these
> changes. Use it if you want to inspect the result or did not keep your Part 4
> project.

### Step 1: replace the packages

From the `GenAiLab` solution directory:

```bash
dotnet remove GenAiLab.AppHost package Aspire.Hosting.Qdrant
dotnet add GenAiLab.AppHost package Aspire.Hosting.Azure.Search --version 13.4.6

dotnet remove GenAiLab.Web package Aspire.Qdrant.Client
dotnet remove GenAiLab.Web package Microsoft.SemanticKernel.Connectors.Qdrant
dotnet add GenAiLab.Web package Aspire.Azure.Search.Documents --version 13.4.6
dotnet add GenAiLab.Web package CommunityToolkit.VectorData.AzureAISearch --version 1.0.0
```

### Step 2: replace the AppHost resource

In `GenAiLab.AppHost/AppHost.cs`, replace:

```csharp
var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
```

with:

```csharp
var search = builder.AddAzureSearch("search");
```

Then replace the `vectorDB` reference:

```csharp
webApp
    .WithReference(search)
    .WaitFor(search);
```

### Step 3: replace the web registrations

In `GenAiLab.Web/Program.cs`, replace the three Qdrant registrations with:

```csharp
builder.AddAzureSearchClient("search");
builder.Services.AddAzureAISearchVectorStore();
builder.Services.AddAzureAISearchCollection<IngestedChunk>(
    IngestedChunk.CollectionName);
```

The rest of the application stays unchanged because ingestion and semantic search
depend on `Microsoft.Extensions.VectorData` abstractions.

### Step 4: choose the fallback if needed

Keep Qdrant instead if your subscription cannot provision Azure AI Search, your
account cannot create role assignments, or you want the lowest-cost workshop
path. Do not make the changes above; Part 11 can deploy the Qdrant container
alongside the web app.

With Azure AI Search, `azd` provisions the service and assigns the deployed app
access. You do not need to create a Search endpoint or index in Part 1.

## What's next

You have made the two decisions that deployment depends on: which provider serves
the model, and which service stores the vectors. Now put it in Azure.

**Continue to** → [Part 11: Deploy to Azure](../Part%2011%20-%20Deployment/README.md)

---

📖 **Return to**: [Workshop Overview](../README.md) | 🔄 **Previous**: [Part 9: Adding AI to an Existing App](../Part%2009%20-%20Adding%20AI%20to%20an%20Existing%20App/README.md) | ➡️ **Next**: [Part 11: Deploy to Azure](../Part%2011%20-%20Deployment/README.md)
