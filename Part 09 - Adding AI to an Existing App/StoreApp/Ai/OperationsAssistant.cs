// =============================================================================
// CAPABILITY 2 - Smarter operations
// =============================================================================
// The app already emits structured logs and traces. This reads them and answers
// operational questions in plain language.
//
// The interesting decision is not the prompt, it is WHICH MODEL RUNS IT.
// Operational data is verbose, high volume, and often sensitive, so this is the
// feature to point at a LOCAL model (Foundry Local or Ollama) while the
// customer-facing feature in ProductDiscovery uses the cloud.
//
// Because provider selection is configuration rather than code (the subject of Part 10), that
// split is a wiring change in Program.cs - not a rewrite of this class.
// =============================================================================

using Microsoft.Extensions.AI;
using StoreApp.Store;

namespace StoreApp.Ai;

public class OperationsAssistant(IChatClient chatClient, OperationsLog operationsLog)
{
    private const string Instructions =
        """
        You are an operations assistant for an e-commerce application.
        Use ONLY the log entries provided to answer. Do not speculate about
        systems that do not appear in them.
        Be concise and lead with the probable root cause, then the evidence
        (service names, trace IDs, timings), then the customer impact.
        If the logs do not explain the problem, say exactly what is missing.
        """;

    public async Task<string> AskAsync(string question)
    {
        // Grounding here is the whole log window rather than a retrieved subset.
        // That volume is another reason to keep this feature on a local model.
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                Instructions + Environment.NewLine + Environment.NewLine +
                "Recent log entries:" + Environment.NewLine + operationsLog.ToText()),
            new(ChatRole.User, question),
        };

        ChatResponse response = await chatClient.GetResponseAsync(messages);
        return response.Text;
    }
}
