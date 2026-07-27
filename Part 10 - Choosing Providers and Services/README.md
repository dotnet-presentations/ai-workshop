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

> [!NOTE]
> This part is a comparison, not a build. There is nothing to scaffold. If you
> want to try the swaps against real code, use any project from earlier in the
> day — the Part 3 `RagChatApp` is the most interesting one, because it exercises
> both chat and embeddings. If you don't have it, you can scaffold a fresh app in
> about a minute:
>
> ```bash
> dotnet new aichatweb --provider azureopenai --vector-store local --name ProviderTest --output ProviderTest
> ```

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
| **Qdrant in a container** (workshop default) | Local development, and deployments where you want to own the data | Cheap and portable, but you run and back up the container |
| **The template's local JSON store** | The Docker-free path, and quick experiments | No infrastructure at all, but not a production store |
| **[Azure AI Search](https://learn.microsoft.com/azure/search/vector-search-overview)** | Production apps that want a managed service, an SLA, and hybrid keyword + vector search | Billed per service hour even when idle |

Part 11 deploys the Qdrant path, because it is the one you have been running all
day and it needs no extra provisioning. If you want the managed option instead,
the swap is a registration change in two files rather than a rewrite — see
[Optional: using a managed vector store](../Part%2011%20-%20Deployment/README.md#optional-using-a-managed-vector-store)
in the next part.

## What's next

You have made the two decisions that deployment depends on: which provider serves
the model, and which service stores the vectors. Now put it in Azure.

**Continue to** → [Part 11: Deploy to Azure](../Part%2011%20-%20Deployment/README.md)

---

📖 **Return to**: [Workshop Overview](../README.md) | 🔄 **Previous**: [Part 9: Adding AI to an Existing App](../Part%2009%20-%20Adding%20AI%20to%20an%20Existing%20App/README.md) | ➡️ **Next**: [Part 11: Deploy to Azure](../Part%2011%20-%20Deployment/README.md)
