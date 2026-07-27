using System.Text;
using Microsoft.Extensions.AI;

namespace Store.Ai;

/// <summary>
/// Summarises the store's own search telemetry.
///
/// This runs against a local model rather than Azure OpenAI. Operational data is high
/// volume, it is generated continuously rather than in response to a user waiting for an
/// answer, and it can contain things you would rather not send anywhere. Those three
/// traits together are the usual argument for running a small model on your own hardware.
/// </summary>
public class OperationsAssistant(
    [FromKeyedServices("local")] IChatClient chatClient,
    SearchTelemetry telemetry,
    ILogger<OperationsAssistant> logger)
{
    private const string SystemPrompt = """
        You are an analyst for an online outdoor gear store.

        You will be given a log of recent shopper searches. Report, in at most four short
        bullet points:
          - what shoppers seem to be looking for
          - any searches that returned no results, which are lost sales
          - anything the store should consider stocking

        Use only the log. Do not invent numbers.
        """;

    public async Task<string> SummarizeAsync(CancellationToken ct = default)
    {
        var events = telemetry.Recent();

        if (events.Count == 0)
        {
            return "No searches have been recorded yet. Run a few searches first.";
        }

        var log = new StringBuilder();

        // Small local models have small context windows, so send only the recent tail.
        foreach (var e in events.TakeLast(40))
        {
            log.AppendLine(
                $"{e.At:HH:mm:ss} query=\"{e.Query}\" results={e.ResultCount} took={e.ElapsedMs}ms");
        }

        List<ChatMessage> messages =
        [
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"Recent searches:\n{log}")
        ];

        logger.LogInformation("Summarising {Count} search events on the local model.", events.Count);

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: ct);
        return StripReasoning(response.Text);
    }

    // Reasoning models such as Phi-4-mini-reasoning narrate their thinking in a <think>
    // block before answering. That is useful when debugging and noise in a UI.
    private static string StripReasoning(string text)
    {
        var end = text.LastIndexOf("</think>", StringComparison.OrdinalIgnoreCase);
        return end >= 0 ? text[(end + "</think>".Length)..].Trim() : text.Trim();
    }
}
