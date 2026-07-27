# Part 2: Build a Chat App (hands-on, code-first)

> **⏱️ Estimated Time:** 45-60 minutes

In this part you build a console chat application **from scratch**. Instead of
starting with a template that hides the details, you write the code yourself so
you understand exactly how a .NET app talks to an AI model. In later parts you
will add retrieval (RAG) by hand, and *then* see the template that generates all
of this for you.

> Adapted with thanks from [Steve Sanderson's dotnet-ai-workshop](https://github.com/SteveSandersonMS/dotnet-ai-workshop).

## What you will learn

1. **Secrets-first configuration**: keep API keys out of source code
2. **`IChatClient`**: the core Microsoft.Extensions.AI abstraction
3. **A chat loop with history**: multi-turn conversations
4. **Streaming responses**: token-by-token output
5. **Structured output**: get typed .NET objects back instead of prose
6. **A middleware pipeline**: add logging without touching your app code

## Prerequisites

- .NET 10 SDK
- A [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) resource with a **`gpt-5-mini`** chat model deployed
  (see [Part 1 - Setup](../Part%2001%20-%20Setup/README.md))

> [!CAUTION]
> Never hardcode endpoints or API keys in source code. This project uses
> [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
> so your credentials stay out of the repository.

## Step 1: Create the console project

### Option A: Create from the command line

```bash
dotnet new console -n ChatApp
cd ChatApp
```

### Option B: Create in Visual Studio 2026

1. Select **File > New > Project**.
1. Search for **Console App**, pick the C# entry, and select **Next**.
1. Name the project `ChatApp`, choose a location, and select **Next**.
1. Set **Framework** to **.NET 10.0**, then select **Create**.

## Step 2: Add the packages

### Option A: Add packages from the command line

```bash
dotnet add package Microsoft.Extensions.AI
dotnet add package Microsoft.Extensions.AI.OpenAI
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Extensions.Configuration.UserSecrets
dotnet add package Microsoft.Extensions.Logging.Console
```

### Option B: Add packages in Visual Studio 2026

Open **Tools > NuGet Package Manager > Package Manager Console** and run:

```powershell
Install-Package Microsoft.Extensions.AI
Install-Package Microsoft.Extensions.AI.OpenAI
Install-Package Azure.AI.OpenAI
Install-Package Microsoft.Extensions.Configuration.UserSecrets
Install-Package Microsoft.Extensions.Logging.Console
```

You can also right-click the project and select **Manage NuGet Packages** and
search for each package on the **Browse** tab.

| Package | Why |
| --- | --- |
| `Microsoft.Extensions.AI` | Provider-agnostic AI abstractions (`IChatClient`) |
| `Microsoft.Extensions.AI.OpenAI` | Adapts the OpenAI/Azure client to `IChatClient` |
| `Azure.AI.OpenAI` | The Azure OpenAI SDK client |
| `Microsoft.Extensions.Configuration.UserSecrets` | Read secrets from outside the repo |
| `Microsoft.Extensions.Logging.Console` | Console logging for the middleware demo |

## Step 3: Store your credentials

Store your endpoint and key with .NET user-secrets. Get these values from the
[Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry)
portal (**[https://ai.azure.com](https://ai.azure.com/)**).

### Option A: Command line (PowerShell)

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:Key" "YOUR-KEY"
```

### Option B: Visual Studio 2026

1. Open your `ChatApp` project in Visual Studio 2026.
2. In Solution Explorer, right-click the `ChatApp` project.
3. Select **Manage User Secrets**.
4. Add these values to the opened `secrets.json` file:

```json
{
    "AzureOpenAI:Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "AzureOpenAI:Key": "YOUR-KEY"
}
```

For the full Visual Studio flow, see:
[Manage user secrets with Visual Studio](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=windows%2Cpowershell#manage-user-secrets-with-visual-studio).

## Step 4: Write the code

Open `Program.cs` and build it in sections from top to bottom.

### 4.1 Add using directives and configuration

```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException(
        "Missing 'AzureOpenAI:Endpoint'. Run: dotnet user-secrets set \"AzureOpenAI:Endpoint\" \"https://YOUR-RESOURCE.openai.azure.com/\"");
string key = config["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException(
        "Missing 'AzureOpenAI:Key'. Run: dotnet user-secrets set \"AzureOpenAI:Key\" \"YOUR-KEY\"");
const string chatModel = "gpt-5-mini";
```

This is a secrets-first setup: endpoint/key come from user-secrets, while the
model has a code default (`gpt-5-mini`) so students can easily experiment.

### 4.2 Create the chat client and add logging middleware

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

IChatClient chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
    .GetChatClient(chatModel)
    .AsIChatClient()
    .AsBuilder()
    .UseLogging(loggerFactory)
    .Build();
```

`AzureOpenAIClient` is the provider-specific SDK, and `.AsIChatClient()` adapts
it to the provider-agnostic `IChatClient` abstraction used by your app.

### 4.3 Add conversation history and startup messages

```csharp
var history = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful, concise assistant for a .NET workshop.")
};

Console.WriteLine("Chat app ready. Type a message (or 'exit' to quit).");
Console.WriteLine();
```

### 4.4 Add the chat loop with streaming responses

```csharp
while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    history.Add(new ChatMessage(ChatRole.User, input));

    Console.Write("Assistant: ");
    var assistantText = new System.Text.StringBuilder();

    await foreach (ChatResponseUpdate update in chatClient.GetStreamingResponseAsync(history))
    {
        Console.Write(update.Text);
        assistantText.Append(update.Text);
    }

    Console.WriteLine();
    Console.WriteLine();

    history.Add(new ChatMessage(ChatRole.Assistant, assistantText.ToString()));
}

Console.WriteLine("Goodbye!");
```

`GetStreamingResponseAsync` yields tokens as they arrive, so users see output
immediately instead of waiting for the full completion.

## Step 5: Run it

```bash
dotnet run
```

In Visual Studio 2026, press **Ctrl+F5** to run without the debugger attached, or
**F5** to debug.

```text
Chat app ready. Type a message (or 'exit' to quit).

You: Give me one tip for learning .NET
Assistant: Build small projects end-to-end...

You: What should I build first?
Assistant: A console app that calls an API you already use...

You: exit
Goodbye!
```

Ask a couple of follow-up questions before you exit. The model answers them in
context, which confirms the history list is doing its job.

## Step 6: Add structured output

Everything you just built returns prose. That is fine when a human reads the
answer, but an application usually needs *data*: something to store in a
database, bind to a grid, render as a row of buttons, or branch on in an `if`.
Getting that out of the text you just streamed would mean parsing English, and
that breaks the moment the model rephrases itself.

This is where a lot of real AI features live. The model does the language work,
and your code gets back something it can `switch` on, save, or pass to a query:

| Feature | The type you ask for |
| --- | --- |
| Triage an incoming support email | `record Ticket(string Category, int Priority, bool NeedsHuman)` |
| Pull fields off a scanned invoice | `record Invoice(string Vendor, decimal Total, DateOnly DueDate)` |
| Classify a product review | `record Review(int Stars, string[] Complaints)` |
| Turn "cheap red running shoes" into a filter | `record SearchFilter(string? Color, decimal? MaxPrice, string[] Tags)` |
| Suggest next steps in a UI | `record Suggestions(string[] Actions)` |

None of these are chatbots. The AI call is one step inside an ordinary feature,
and the model's answer has to survive being handed to the next line of C#.

`GetResponseAsync<T>` sends `T`'s JSON schema along with the request and
deserializes the reply into a real .NET object, so there is no string parsing at
all. The schema also constrains the model while it generates, which makes a
missing or invented field much less likely than asking for JSON in the prompt.

First, describe the shape you want. Add this at the **end** of `Program.cs`
(after the `Console.WriteLine("Goodbye!");` line):

```csharp
record ConversationSummary(
    string Topic,
    string Sentiment,
    string[] FollowUpQuestions);
```

Property names are part of the instructions the model sees, so make them
descriptive. `FollowUpQuestions` is the interesting one: it comes back as a real
`string[]`, so a UI could turn each element into a suggestion chip without any
extra work. Now add a `summary` command inside the chat loop, immediately
**before** the `history.Add(new ChatMessage(ChatRole.User, input));` line:

```csharp
if (input.Equals("summary", StringComparison.OrdinalIgnoreCase))
{
    List<ChatMessage> summaryPrompt =
    [
        .. history,
        new ChatMessage(ChatRole.User, "Summarize this conversation so far.")
    ];

    ChatResponse<ConversationSummary> response =
        await chatClient.GetResponseAsync<ConversationSummary>(summaryPrompt);

    if (response.TryGetResult(out ConversationSummary? summary))
    {
        Console.WriteLine($"Topic:      {summary.Topic}");
        Console.WriteLine($"Sentiment:  {summary.Sentiment}");
        Console.WriteLine($"Follow-ups: {string.Join(", ", summary.FollowUpQuestions)}");
    }
    else
    {
        Console.WriteLine("The model did not return a valid summary object.");
    }

    Console.WriteLine();
    continue;
}
```

`TryGetResult` matters: the model can still return something that doesn't fit the
schema, and structured output is a strong request, not a guarantee. Handle the
failure case rather than assuming a value.

While you're here, update the startup message so the new command is discoverable:

```csharp
Console.WriteLine("Chat app ready. Type a message ('summary' for a typed summary, 'exit' to quit).");
```

Your completed file should now match [ChatApp/Program.cs](ChatApp/Program.cs).

## Step 7: Run it again

```bash
dotnet run
```

Chat for a turn or two, then type `summary`:

```text
Chat app ready. Type a message ('summary' for a typed summary, 'exit' to quit).

You: Give me one tip for learning .NET
Assistant: Build small projects end-to-end...

You: summary
Topic:      Learning .NET
Sentiment:  Positive
Follow-ups: Which project should I build first?, How do I practice consistently?

You: exit
Goodbye!
```

The two runs are the contrast worth remembering: the same model and the same
conversation, but one call gives you text to show a person and the other gives
you an object your code can use.

## What's next

In **Part 3** you'll extend this app with **retrieval-augmented generation
(RAG)** by hand so you understand how embeddings and vector search feed
context to the model. After that, you'll compare your code with the template-based
version.

**Continue to** → [Part 3: Add RAG](../Part%2003%20-%20Add%20RAG/README.md)
