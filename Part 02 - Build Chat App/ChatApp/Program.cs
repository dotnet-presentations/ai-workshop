// =============================================================================
// Part 2 - Build a Chat App (hands-on, code-first)
// =============================================================================
// In this part you build a console chat app from scratch so you understand what
// the templates generate for you later. You will learn, by writing the code:
//
//   1. Secrets-first configuration   (never hardcode keys)
//   2. Creating an IChatClient        (the core Microsoft.Extensions.AI abstraction)
//   3. A chat loop with history       (multi-turn conversations)
//   4. Streaming responses            (token-by-token output)
//   5. Structured output              (AI answers as typed .NET objects)
//   6. A middleware pipeline          (add logging without touching your app code)
//
// Adapted with thanks from Steve Sanderson's dotnet-ai-workshop
// (https://github.com/SteveSandersonMS/dotnet-ai-workshop).
// =============================================================================

using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// -----------------------------------------------------------------------------
// 1. Secrets-first configuration
// -----------------------------------------------------------------------------
// Endpoint, key, and model name come from user-secrets, NOT source code.
// Before running, set them once (values are stored outside the repo):
//
//   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
//   dotnet user-secrets set "AzureOpenAI:Key" "YOUR-KEY"
//
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

// -----------------------------------------------------------------------------
// 2. Create an IChatClient
// -----------------------------------------------------------------------------
// AzureOpenAIClient is the provider-specific SDK client. We immediately adapt it
// to IChatClient - the provider-agnostic Microsoft.Extensions.AI abstraction.
// Every provider (Azure OpenAI, OpenAI, Ollama, Foundry Local, ...) gives you the
// same IChatClient, so the rest of your app never changes when you switch models.
//
// 6. A middleware pipeline
//    .AsBuilder() lets us wrap the client with cross-cutting behavior. Here we add
//    logging - every request/response is logged without changing our app code.
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

IChatClient chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
    .GetChatClient(chatModel)
    .AsIChatClient()
    .AsBuilder()
    .UseLogging(loggerFactory)
    .Build();

// -----------------------------------------------------------------------------
// 3. A chat loop with history
// -----------------------------------------------------------------------------
// We keep every message in a list so the model has the full conversation context
// on each turn. The system message sets the assistant's behavior.
var history = new List<ChatMessage>
{
    new(ChatRole.System, "You are a helpful, concise assistant for a .NET workshop.")
};

Console.WriteLine("Chat app ready. Type a message ('summary' for a typed summary, 'exit' to quit).");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    // ---------------------------------------------------------------------
    // 5. Structured output
    // ---------------------------------------------------------------------
    // Sometimes you want data, not prose. GetResponseAsync<T> asks the model for
    // JSON that matches T's schema and deserializes it for you, so the rest of
    // your app works with an ordinary .NET object instead of parsing text.
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

    history.Add(new ChatMessage(ChatRole.User, input));

    // ---------------------------------------------------------------------
    // 4. Streaming responses
    // ---------------------------------------------------------------------
    // GetStreamingResponseAsync yields the answer token-by-token so the user
    // sees output immediately. We accumulate the text to add it back to history.
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

// -----------------------------------------------------------------------------
// The shape of the structured output
// -----------------------------------------------------------------------------
// The model is given this type's JSON schema and must answer with matching JSON.
// Keep the type small and the property names descriptive: the names are part of
// the instructions the model sees.
record ConversationSummary(
    string Topic,
    string Sentiment,
    string[] FollowUpQuestions);
