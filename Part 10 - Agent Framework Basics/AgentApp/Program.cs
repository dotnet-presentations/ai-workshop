using System.ComponentModel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// --- Configuration (same pattern as Part 2) ---
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint");
string key = config["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Key");

// --- Create the chat client ---
IChatClient chatClient = new AzureOpenAIClient(
        new Uri(endpoint), new AzureKeyCredential(key))
    .GetChatClient("gpt-4o-mini")
    .AsIChatClient();

// --- Define a tool ---
[Description("Get the current status of a customer order")]
static string GetOrderStatus(
    [Description("The order ID, e.g. ORD-1001")] string orderId)
{
    // In a real app this would call your order service or database
    return orderId switch
    {
        "ORD-1001" => $"Shipped on {DateTime.Now.AddDays(-3):yyyy-MM-dd}, arriving {DateTime.Now.AddDays(3):MMMM d}",
        "ORD-1002" => "Processing, expected to ship tomorrow",
        _ => $"No order found with ID {orderId}"
    };
}

// --- Create the agent with the tool ---
AIAgent agent = chatClient.AsAIAgent(
    name: "OrdersAssistant",
    instructions: """
        You help support staff answer questions about customer orders.
        Use the GetOrderStatus tool when someone asks about a specific order.
        """,
    tools: [AIFunctionFactory.Create(GetOrderStatus)]);

// --- Run a multi-turn conversation ---
AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("Order Assistant ready. Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    var response = await agent.RunAsync(input, session);
    Console.WriteLine($"Agent: {response.Text}\n");
}
