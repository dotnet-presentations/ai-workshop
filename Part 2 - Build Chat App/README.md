# Part 2: Build a Chat App (hands-on, code-first)

In this part you build a console chat application **from scratch**. Instead of
starting with a template that hides the details, you write the code yourself so
you understand exactly how a .NET app talks to an AI model. In later parts you
will add retrieval (RAG) by hand, and *then* see the template that generates all
of this for you.

> Adapted with thanks from [Steve Sanderson's dotnet-ai-workshop](https://github.com/SteveSandersonMS/dotnet-ai-workshop).

## What you will learn

1. **Secrets-first configuration** – keep API keys out of source code
2. **`IChatClient`** – the core Microsoft.Extensions.AI abstraction
3. **A chat loop with history** – multi-turn conversations
4. **Streaming responses** – token-by-token output
5. **A middleware pipeline** – add logging without touching your app code

## Prerequisites

- .NET 10 SDK
- An Azure AI Foundry resource with a **`gpt-4.1-mini`** chat model deployed
  (see [Part 1 - Setup](../Part%201%20-%20Setup/README.md))

> [!CAUTION]
> Never hardcode endpoints or API keys in source code. This project uses
> [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
> so your credentials stay out of the repository.

## Step 1: Create the console project

```bash
dotnet new console -n ChatApp
cd ChatApp
```

## Step 2: Add the packages

```bash
dotnet add package Microsoft.Extensions.AI
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Extensions.Configuration.UserSecrets
dotnet add package Microsoft.Extensions.Logging.Console
```

| Package | Why |
| --- | --- |
| `Microsoft.Extensions.AI` | Provider-agnostic AI abstractions (`IChatClient`) |
| `Microsoft.Extensions.AI.OpenAI` | Adapts the OpenAI/Azure client to `IChatClient` |
| `Azure.AI.OpenAI` | The Azure OpenAI SDK client |
| `Microsoft.Extensions.Configuration.UserSecrets` | Read secrets from outside the repo |
| `Microsoft.Extensions.Logging.Console` | Console logging for the middleware demo |

## Step 3: Store your credentials

Initialize user-secrets, then set your endpoint, key, and model. Get these from
the Azure AI Foundry portal (**https://ai.azure.com**).

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:Key" "YOUR-KEY"
dotnet user-secrets set "AzureOpenAI:ChatModel" "gpt-4.1-mini"
```

## Step 4: Write the code

Replace the contents of `Program.cs` with the code in
[ChatApp/Program.cs](ChatApp/Program.cs). Work through it section by section:

### Configuration (secrets-first)

`ConfigurationBuilder().AddUserSecrets<Program>()` reads the values you set in
Step 3. If a required value is missing, the app throws a clear error telling you
which secret to set — no silent failures, no keys in code.

### Create an `IChatClient`

```csharp
IChatClient chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
    .GetChatClient(chatModel)
    .AsIChatClient();
```

`AzureOpenAIClient` is the provider-specific SDK. `.AsIChatClient()` adapts it to
`IChatClient`, the abstraction the rest of your app uses. Because every provider
(Azure OpenAI, OpenAI, Ollama, Foundry Local, …) surfaces the same `IChatClient`,
you can swap models later without changing your app logic.

### A chat loop with history

The conversation lives in a `List<ChatMessage>`. Each turn you add the user's
message, send the **whole list** to the model (so it has full context), then add
the assistant's reply. The first message is a `ChatRole.System` prompt that sets
the assistant's behavior.

### Streaming responses

```csharp
await foreach (ChatResponseUpdate update in chatClient.GetStreamingResponseAsync(history))
{
    Console.Write(update.Text);
}
```

`GetStreamingResponseAsync` yields the answer token-by-token so the user sees
output immediately instead of waiting for the full response.

### A middleware pipeline

```csharp
IChatClient chatClient = /* ... */
    .AsIChatClient()
    .AsBuilder()
    .UseLogging(loggerFactory)
    .Build();
```

`.AsBuilder()` lets you wrap the client with cross-cutting behavior. `UseLogging`
logs every request and response — and you added it **without changing any of your
chat-loop code**. This same pattern is how you'll later add function calling,
caching, and telemetry.

## Step 5: Run it

```bash
dotnet run
```

```
Chat app ready. Type a message (or 'exit' to quit).

You: Give me one tip for learning .NET
Assistant: Build small projects end-to-end...

You: exit
Goodbye!
```

## What's next

In **Part 3** you'll extend this app with **retrieval-augmented generation
(RAG)** — by hand — so you understand how embeddings and vector search feed
context to the model. Only after that do you meet the template that wires it all
up for you.
