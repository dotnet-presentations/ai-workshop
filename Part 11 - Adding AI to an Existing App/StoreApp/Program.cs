// =============================================================================
// Part 11 - Adding AI to an Existing App
// =============================================================================
// Contoso Outdoors is an existing store application: a catalog, a search log,
// and operational telemetry (everything under Store/). This program adds five
// targeted AI capabilities on top of it - not one big chatbot:
//
//   1. Product discovery       semantic search + honesty gate + grounded answer
//   2. Operations assistant    a LOCAL model reading the logs the app emits
//   3. Store intelligence      a business briefing built from existing signals
//   4. Capabilities as tools   app services exposed as reviewed, read-only tools
//   5. Agent collaboration     three specialists plus an orchestrator
//
// Everything the model touches comes from the existing app. Nothing here reaches
// past the app into its database.
//
// Setup:
//   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
//   dotnet user-secrets set "AzureOpenAI:Key" "YOUR-KEY"
//
// Optional, to run capability 2 against a local model instead of the cloud:
//   dotnet user-secrets set "LocalModel:Endpoint" "http://localhost:11434/v1"
//   dotnet user-secrets set "LocalModel:Model" "llama3.2"
// =============================================================================

using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using StoreApp.Ai;
using StoreApp.Store;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException(
        "Missing 'AzureOpenAI:Endpoint'. Run: dotnet user-secrets set \"AzureOpenAI:Endpoint\" \"https://YOUR-RESOURCE.openai.azure.com/\"");
string key = config["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException(
        "Missing 'AzureOpenAI:Key'. Run: dotnet user-secrets set \"AzureOpenAI:Key\" \"YOUR-KEY\"");

const string chatModel = "gpt-4o-mini";
const string embeddingModel = "text-embedding-3-small";

// -----------------------------------------------------------------------------
// The existing app. No AI packages are involved in constructing any of this.
// -----------------------------------------------------------------------------
var catalog = new Catalog();
var searchLog = new SearchLog();
var operationsLog = new OperationsLog();

// -----------------------------------------------------------------------------
// The AI clients. Cloud for customer-facing language, optionally local for
// operations - the Part 5 provider abstraction paying off.
// -----------------------------------------------------------------------------
var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

IChatClient cloudChat = azureClient.GetChatClient(chatModel)
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()     // required so agents can actually call their tools
    .Build();

IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    azureClient.GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator();

IChatClient operationsChat = CreateOperationsChatClient(config) ?? cloudChat;
bool usingLocalModel = !ReferenceEquals(operationsChat, cloudChat);

// -----------------------------------------------------------------------------
// The five capabilities.
// -----------------------------------------------------------------------------
var discovery = new ProductDiscovery(embeddingGenerator, cloudChat, catalog, searchLog);
var operationsAssistant = new OperationsAssistant(operationsChat, operationsLog);
var report = new StoreIntelligenceReport(cloudChat, catalog, searchLog, operationsLog);
var tools = new StoreTools(catalog, searchLog, operationsLog);
var agents = new StoreAgentNetwork(cloudChat, tools);

Console.WriteLine("Contoso Outdoors - existing store app with targeted AI");
Console.WriteLine($"Catalog: {catalog.All.Count} products. Operations assistant model: " +
                  (usingLocalModel ? "local" : "cloud (set LocalModel:Endpoint to run it locally)"));
Console.WriteLine();

while (true)
{
    Console.WriteLine("""
        1) Product discovery      (semantic search + honesty gate)
        2) Operations assistant   (ask about the logs)
        3) Store intelligence     (business briefing from app signals)
        4) Compare search         (keyword vs. semantic, side by side)
        5) Agent network          (three specialists + orchestrator)
        q) Quit
        """);
    Console.Write("Choose: ");
    string? choice = Console.ReadLine();
    Console.WriteLine();

    switch (choice?.Trim().ToLowerInvariant())
    {
        case "1":
            await RunDiscoveryAsync();
            break;
        case "2":
            await RunOperationsAsync();
            break;
        case "3":
            await RunReportAsync();
            break;
        case "4":
            await RunComparisonAsync();
            break;
        case "5":
            await RunAgentNetworkAsync();
            break;
        case "q":
        case "exit":
        case null:
            Console.WriteLine("Goodbye!");
            return;
        default:
            Console.WriteLine("Pick 1-5 or q.");
            break;
    }

    Console.WriteLine();
}

// -----------------------------------------------------------------------------
// 1. Product discovery
// -----------------------------------------------------------------------------
async Task RunDiscoveryAsync()
{
    Console.WriteLine("Embedding the catalog (once)...");
    await discovery.BuildIndexAsync();
    Console.WriteLine("Describe what you need. Try 'something warm for a rainy hike',");
    Console.WriteLine("then try 'scuba tank' to watch the honesty gate refuse to guess.");
    Console.WriteLine("(Blank line to go back.)");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Customer: ");
        string? query = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        string answer = await discovery.AskAsync(query);
        Console.WriteLine($"Assistant: {answer}");
        Console.WriteLine();
    }
}

// -----------------------------------------------------------------------------
// 2. Operations assistant
// -----------------------------------------------------------------------------
async Task RunOperationsAsync()
{
    Console.WriteLine($"Reading {operationsLog.Recent().Count} log entries with the " +
                      (usingLocalModel ? "local" : "cloud") + " model.");
    Console.WriteLine("Try 'what is failing right now and who is affected?'");
    Console.WriteLine("(Blank line to go back.)");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Operator: ");
        string? question = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(question))
        {
            return;
        }

        string answer = await operationsAssistant.AskAsync(question);
        Console.WriteLine($"Assistant: {answer}");
        Console.WriteLine();
    }
}

// -----------------------------------------------------------------------------
// 3. Store intelligence report
// -----------------------------------------------------------------------------
async Task RunReportAsync()
{
    Console.WriteLine("Signals the app already recorded:");
    Console.WriteLine(report.BuildFacts());
    Console.WriteLine("Briefing:");
    Console.WriteLine(await report.GenerateAsync());
}

// -----------------------------------------------------------------------------
// 4. Keyword vs. semantic, so the improvement is visible rather than asserted
// -----------------------------------------------------------------------------
async Task RunComparisonAsync()
{
    await discovery.BuildIndexAsync();

    string[] queries =
    [
        "something warm for a rainy hike",
        "how do I carry water on a long walk",
        "scuba tank",
    ];

    foreach (string query in queries)
    {
        Console.WriteLine($"Query: \"{query}\"");

        IReadOnlyList<Product> keyword = catalog.KeywordSearch(query);
        Console.WriteLine($"  Keyword search: {keyword.Count} result(s)");

        var semantic = await discovery.SearchAsync(query);
        if (semantic.Count == 0)
        {
            Console.WriteLine("  Semantic search: 0 results above the relevance threshold (honesty gate held)");
        }
        else
        {
            foreach ((Product product, float score) in semantic)
            {
                Console.WriteLine($"  Semantic search: {score:F2}  {product.Name}");
            }
        }

        Console.WriteLine();
    }
}

// -----------------------------------------------------------------------------
// 5. Agent network
// -----------------------------------------------------------------------------
async Task RunAgentNetworkAsync()
{
    Console.WriteLine("Ask something that crosses domains, for example:");
    Console.WriteLine("  'Rain shells are selling badly this week. Is that demand or a bug?'");
    Console.WriteLine("(Blank line to go back.)");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Ops lead: ");
        string? question = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(question))
        {
            return;
        }

        string answer = await agents.AskAsync(
            question,
            finding => Console.WriteLine($"  [{finding.Split(Environment.NewLine)[0]}]"));

        Console.WriteLine();
        Console.WriteLine($"Orchestrator: {answer}");
        Console.WriteLine();
    }
}

// -----------------------------------------------------------------------------
// Local model wiring: any OpenAI-compatible endpoint (Foundry Local, Ollama).
// Returns null when it is not configured, and the app falls back to the cloud.
// -----------------------------------------------------------------------------
static IChatClient? CreateOperationsChatClient(IConfiguration configuration)
{
    string? localEndpoint = configuration["LocalModel:Endpoint"];
    string? localModel = configuration["LocalModel:Model"];

    if (string.IsNullOrWhiteSpace(localEndpoint) || string.IsNullOrWhiteSpace(localModel))
    {
        return null;
    }

    var options = new OpenAIClientOptions { Endpoint = new Uri(localEndpoint) };
    return new OpenAIClient(new ApiKeyCredential("not-used-by-local-servers"), options)
        .GetChatClient(localModel)
        .AsIChatClient();
}
