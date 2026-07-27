using System.Text;
using DataEntities;
using Microsoft.Extensions.AI;
using Store.Services;

namespace Store.Ai;

/// <summary>
/// Answers shopper questions in natural language, grounded in the product catalog.
/// The model never sees the whole catalog - it only sees the handful of products that
/// semantic search returned, which is what keeps the answer honest and the prompt small.
/// </summary>
public class ProductDiscovery(
    IChatClient chatClient,
    ProductService productService,
    SearchTelemetry telemetry,
    ILogger<ProductDiscovery> logger)
{
    private const string SystemPrompt = """
        You are a shopping assistant for an outdoor gear store.

        Answer the shopper's question using ONLY the products listed below. Recommend at
        most two of them and say briefly why each one fits. If none of the products are a
        good fit, say so plainly and do not suggest anything else. Never invent products,
        prices, or features. Keep the answer under 80 words.
        """;

    public async Task<DiscoveryResult> AskAsync(string question, CancellationToken ct = default)
    {
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var products = await productService.AiSearchProducts(question);
        var elapsedMs = (long)System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;

        telemetry.Record(question, products.Count, elapsedMs);

        if (products.Count == 0)
        {
            return new DiscoveryResult(
                "We do not stock anything that matches that. Try describing it differently.",
                products);
        }

        var catalog = new StringBuilder();
        foreach (var product in products)
        {
            catalog.AppendLine(
                $"- {product.Name} ({product.Price:C}): {product.Description}");
        }

        List<ChatMessage> messages =
        [
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"""
                Products available:
                {catalog}
                Shopper's question: {question}
                """)
        ];

        var response = await chatClient.GetResponseAsync(messages, cancellationToken: ct);
        logger.LogInformation("Grounded answer produced from {Count} candidate products.", products.Count);

        return new DiscoveryResult(response.Text, products);
    }
}

public record DiscoveryResult(string Answer, List<Product> Products);
