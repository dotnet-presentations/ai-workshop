// =============================================================================
// CAPABILITY 5 - Agents collaborate
// =============================================================================
// "Rain jacket sales dropped this week - is that demand or a bug?" spans the
// catalog, the telemetry, and the business data. One agent holding every tool
// for all three becomes unreliable, exactly as described in Part 10.
//
// So: three SPECIALIST agents, each with a narrow instruction set and a small
// tool list, plus an orchestrator that fans the question out, collects the
// findings, and synthesizes one answer. Each specialist is just the single agent
// you already built in Part 10.
//
// This runs the specialists concurrently in-process. eShopLite scenario 17 shows
// the same three roles talking over A2A across service boundaries.
// =============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace StoreApp.Ai;

public class StoreAgentNetwork
{
    private readonly AIAgent _catalogAgent;
    private readonly AIAgent _observabilityAgent;
    private readonly AIAgent _businessAgent;
    private readonly AIAgent _orchestrator;

    public StoreAgentNetwork(IChatClient chatClient, StoreTools tools)
    {
        _catalogAgent = chatClient.AsAIAgent(
            name: "CatalogAgent",
            instructions:
                """
                You are the catalog specialist for an outdoor gear store.
                You answer only about products, categories, pricing, and stock levels.
                Always call a tool for product facts; never answer from memory.
                If a question is not about the catalog, say it is outside your area.
                """,
            tools: tools.CatalogTools);

        _observabilityAgent = chatClient.AsAIAgent(
            name: "ObservabilityAgent",
            instructions:
                """
                You are the observability specialist for an e-commerce application.
                You answer only about application health: errors, latency, failing
                dependencies, and their customer impact. Ground every claim in log
                entries you retrieved with a tool, and cite service names and trace IDs.
                If a question is not about application health, say it is outside your area.
                """,
            tools: tools.ObservabilityTools);

        _businessAgent = chatClient.AsAIAgent(
            name: "BusinessInsightsAgent",
            instructions:
                """
                You are the business insights specialist for a retail store.
                You answer only about demand signals: what customers searched for,
                what they could not find, and what is running low. Use tools for every
                figure. Recommend actions; never claim to have taken one.
                If a question is not about demand or inventory signals, say it is outside your area.
                """,
            tools: tools.BusinessTools);

        _orchestrator = chatClient.AsAIAgent(
            name: "StoreOperationsOrchestrator",
            instructions:
                """
                You combine findings from three specialist agents into one answer for a
                store operations lead. Use only what the specialists reported. Where they
                disagree or a specialist had nothing relevant, say so.
                Lead with the most likely explanation, then the supporting evidence, then
                the recommended next step. Keep it under 200 words.
                """);
    }

    /// <summary>
    /// Fan out to the specialists concurrently, then synthesize. This is the
    /// "concurrent" orchestration pattern from Part 10, written explicitly so you
    /// can see there is no magic in it.
    /// </summary>
    public async Task<string> AskAsync(string question, Action<string>? onSpecialistReply = null)
    {
        var specialists = new[] { _catalogAgent, _observabilityAgent, _businessAgent };

        AgentResponse[] replies = await Task.WhenAll(
            specialists.Select(agent => agent.RunAsync(question)));

        var findings = new List<string>();
        for (int i = 0; i < specialists.Length; i++)
        {
            string finding = $"{specialists[i].Name} reported:{Environment.NewLine}{replies[i].Text}";
            findings.Add(finding);
            onSpecialistReply?.Invoke(finding);
        }

        string combined =
            $"Question: {question}{Environment.NewLine}{Environment.NewLine}" +
            string.Join(Environment.NewLine + Environment.NewLine, findings);

        AgentResponse synthesis = await _orchestrator.RunAsync(combined);
        return synthesis.Text;
    }
}
