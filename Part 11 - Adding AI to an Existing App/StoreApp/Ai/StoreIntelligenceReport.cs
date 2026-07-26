// =============================================================================
// CAPABILITY 3 - Smarter business signals
// =============================================================================
// The app already knows things nobody reads. Searches that returned nothing are
// unmet demand. Repeated searches are trending interest. Errors are customer
// impact. All of it is already in the app; none of it reaches a store manager.
//
// Two rules keep this honest:
//   - The numbers come from the app, not the model. The model summarizes and
//     prioritizes facts that were already recorded.
//   - The output is a RECOMMENDATION, not an action. A human decides whether to
//     stock the thing nobody could find.
// =============================================================================

using System.Text;
using Microsoft.Extensions.AI;
using StoreApp.Store;

namespace StoreApp.Ai;

public class StoreIntelligenceReport(
    IChatClient chatClient,
    Catalog catalog,
    SearchLog searchLog,
    OperationsLog operationsLog)
{
    public async Task<string> GenerateAsync()
    {
        // Step 1: the app computes the facts. No model involved.
        string facts = BuildFacts();

        // Step 2: the model turns facts into a briefing for a non-engineer.
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                """
                You write a short weekly briefing for a retail store manager who is
                not an engineer. Use ONLY the figures provided; never invent numbers
                or products. Structure the reply as:
                  What happened
                  What it probably means
                  What to consider doing
                Keep it under 250 words and recommend actions, never take them.
                """),
            new(ChatRole.User, facts),
        };

        ChatResponse response = await chatClient.GetResponseAsync(messages);
        return response.Text;
    }

    /// <summary>The signals the app already has, formatted for grounding.</summary>
    public string BuildFacts()
    {
        var sb = new StringBuilder();

        sb.AppendLine("SEARCHES THAT RETURNED NOTHING (unmet demand):");
        foreach ((string query, int count) in searchLog.UnmetDemand())
        {
            sb.AppendLine($"- \"{query}\" searched {count} time(s), 0 results");
        }

        sb.AppendLine();
        sb.AppendLine("MOST FREQUENT SEARCHES:");
        foreach ((string query, int count) in searchLog.TopQueries())
        {
            sb.AppendLine($"- \"{query}\": {count} time(s)");
        }

        sb.AppendLine();
        sb.AppendLine("LOW STOCK (10 or fewer):");
        foreach (Product product in catalog.LowStock())
        {
            sb.AppendLine($"- {product.Name}: {product.InStock} remaining");
        }

        sb.AppendLine();
        IReadOnlyList<LogEntry> errors = operationsLog.Errors();
        sb.AppendLine($"OPERATIONAL ERRORS IN THE LOG WINDOW: {errors.Count}");
        foreach (LogEntry entry in errors.Take(5))
        {
            sb.AppendLine($"- {entry.Service}: {entry.Message}");
        }

        return sb.ToString();
    }
}
