# Part 5: Providers & fallbacks (Azure primary; local + legacy)

This is the payoff of the whole workshop theme: **swap the provider, keep the same
`IChatClient` / `IEmbeddingGenerator`.** Everything you built in Parts 2–4 — the
chat loop, the RAG loop, the template app — runs unchanged when you point it at a
different model provider. The only thing that changes is *registration* (an
endpoint, a key, a model name), never your app code.

The real-world motivation: **GitHub Models retires July 30, 2026.** Because the app
depends on the abstraction and not on a specific provider, surviving that
retirement is a configuration change, not a rewrite.

## The four providers

| Provider | Best for | Chat | Embeddings | Runs offline | Notes |
| --- | --- | :---: | :---: | :---: | --- |
| **Azure AI Foundry** (Azure OpenAI) | **Primary** — the workshop default | ✅ | ✅ | ❌ | `gpt-4.1-mini` + `text-embedding-3-small` |
| **Foundry Local** | Local **chat** on your device | ✅ | ⏳ | ✅ | OpenAI-compatible local server; SLMs (Phi, Qwen, …). Embedding support is being evaluated (#496) |
| **Ollama** | Fully **offline RAG** (chat + embeddings) | ✅ | ✅ | ✅ | e.g. `llama3.2` for chat, `all-minilm` for embeddings |
| **GitHub Models** (legacy) | Zero-setup dev/demo | ✅ | ⚠️ | ❌ | **Retires 2026-07-30** (brownouts July 16 & 23) |

## Provider 1: Azure AI Foundry (primary)

This is what Parts 2–4 already use — the Azure-specific client, adapted to
`IChatClient`:

```csharp
using Azure;
using Azure.AI.OpenAI;

var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
IChatClient chat = client.GetChatClient("gpt-4.1-mini").AsIChatClient();
IEmbeddingGenerator<string, Embedding<float>> embeddings =
    client.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();
```

## The universal pattern for everything else

Foundry Local, Ollama, and GitHub Models **all** expose an **OpenAI-compatible**
endpoint. That means all three use the *same* client — `OpenAIClient` — pointed at
a different base URL and key:

```csharp
using OpenAI;
using System.ClientModel;

var client = new OpenAIClient(
    new ApiKeyCredential(key),
    new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

IChatClient chat = client.GetChatClient(modelId).AsIChatClient();
```

Notice: this is the **same three lines** regardless of provider. Only `key`,
`baseUrl`, and `modelId` change — and they live in configuration, not code.

## Provider 2: Foundry Local (offline chat)

[Foundry Local](https://learn.microsoft.com/azure/foundry-local/) runs
Microsoft-curated small language models entirely on-device — no Azure
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

Drop this `chat` into your Part 2 loop and it just works — offline. See the
[Foundry Local + inference SDK guide](https://learn.microsoft.com/azure/foundry-local/how-to/how-to-integrate-with-inference-sdks) for the full manager setup.

> Foundry Local's curated catalog is currently focused on chat/SLMs. Whether it
> serves **embeddings** for a fully-local RAG path is being evaluated (#496); for
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

Swap these two into your Part 3 project and the entire embed → store → search →
augment loop runs with no cloud at all.

## Provider 4: GitHub Models (legacy)

GitHub Models gave free, zero-setup access during development. It uses the same
universal pattern with a GitHub token as the key:

```csharp
var client = new OpenAIClient(
    new ApiKeyCredential(githubToken),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") });
IChatClient chat = client.GetChatClient("gpt-4o-mini").AsIChatClient();
```

> [!WARNING]
> **GitHub Models retires on July 30, 2026** (with brownouts on July 16 and 23).
> Do not build new work on it. It's shown here only because you may see it in older
> samples — and as the concrete reason this whole "swap the provider" design
> matters. Move to **Azure AI Foundry** (cloud) or **Foundry Local / Ollama**
> (local).

## The takeaway

| What changed between providers | Where it lives |
| --- | --- |
| Endpoint URL | configuration (user-secrets) |
| API key | configuration (user-secrets) |
| Model name | configuration (user-secrets) |
| **Your app code** | **unchanged** |

That's the entire point of `Microsoft.Extensions.AI`: your chat loop, your RAG
pipeline, and the template app are all written against `IChatClient` and
`IEmbeddingGenerator`, so the provider becomes a deployment decision — including
when a provider you relied on goes away.

## What's next

You've now covered the AI half end-to-end: build by hand, meet the template,
swap providers. The rest of the workshop moves to **tools and agents** — starting
with the **Model Context Protocol (MCP)**, where you build a tool your app (and
later an agent) can call.
