// =============================================================================
// CAPABILITY 1 - Smarter product discovery
// =============================================================================
// Keyword search fails when a customer describes a need instead of a product.
// This adds semantic search over the catalog the app already owns, and then does
// the part most demos skip: an HONESTY GATE.
//
//   1. Embed the catalog once      (at startup; re-embed only when products change)
//   2. Embed the user's intent     (per query)
//   3. Select the top matches      (cosine similarity, as in Part 3)
//   4. Apply a score threshold     <-- if nothing clears the bar, return nothing
//   5. Ground the answer           (send ONLY matched products to the chat model)
//
// Step 4 is why "we don't carry anything like that" is a valid answer instead of
// a confident description of a product that does not exist.
// =============================================================================

using Microsoft.Extensions.AI;
using StoreApp.Store;

namespace StoreApp.Ai;

public class ProductDiscovery(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IChatClient chatClient,
    Catalog catalog,
    SearchLog searchLog,
    float relevanceThreshold = 0.30f)
{
    private readonly List<(Product Product, ReadOnlyMemory<float> Vector)> _index = [];

    /// <summary>Step 1: embed the catalog once. Cost is paid at startup, not per query.</summary>
    public async Task BuildIndexAsync()
    {
        string[] texts = catalog.All.Select(p => p.ToEmbeddingText()).ToArray();
        GeneratedEmbeddings<Embedding<float>> embeddings = await embeddingGenerator.GenerateAsync(texts);

        _index.Clear();
        for (int i = 0; i < catalog.All.Count; i++)
        {
            _index.Add((catalog.All[i], embeddings[i].Vector));
        }
    }

    /// <summary>
    /// Steps 2-4: embed the query, rank the catalog, and gate on score.
    /// Returns an empty list when nothing is relevant enough - that is the feature.
    /// </summary>
    public async Task<IReadOnlyList<(Product Product, float Score)>> SearchAsync(string query, int topK = 3)
    {
        if (_index.Count == 0)
        {
            throw new InvalidOperationException("Call BuildIndexAsync() before searching.");
        }

        ReadOnlyMemory<float> queryVector = (await embeddingGenerator.GenerateAsync(query)).Vector;

        var matches = _index
            .Select(item => (item.Product, Score: CosineSimilarity(queryVector.Span, item.Vector.Span)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Where(x => x.Score >= relevanceThreshold)   // <-- the honesty gate
            .ToList();

        // The existing app keeps recording searches, so this AI feature feeds the
        // same signal that capability 3 reports on. Zero results is unmet demand.
        searchLog.Record(query, matches.Count);

        return matches;
    }

    /// <summary>Step 5: answer using ONLY the matched products.</summary>
    public async Task<string> AskAsync(string query)
    {
        var matches = await SearchAsync(query);

        if (matches.Count == 0)
        {
            // No model call at all. Nothing relevant was retrieved, so there is
            // nothing to ground an answer in, and an ungrounded answer is a risk.
            return "I couldn't find anything in our catalog that matches that. " +
                   "We may not carry it yet.";
        }

        string context = string.Join(
            Environment.NewLine + Environment.NewLine,
            matches.Select(m =>
                $"- {m.Product.Name} (${m.Product.Price}, {m.Product.Category}, {m.Product.InStock} in stock)\n  {m.Product.Description}"));

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                """
                You are a shopping assistant for an outdoor gear store.
                Answer using ONLY the products listed below. Never invent a product,
                a price, or a feature. If the listed products do not fully solve the
                customer's problem, say so plainly.
                Recommend at most three products and explain briefly why each fits.
                """ + Environment.NewLine + Environment.NewLine + "Products:" + Environment.NewLine + context),
            new(ChatRole.User, query),
        };

        ChatResponse response = await chatClient.GetResponseAsync(messages);
        return response.Text;
    }

    /// <summary>Cosine similarity, same helper you wrote by hand in Part 3.</summary>
    private static float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException(
                $"Embedding dimensions do not match ({a.Length} vs {b.Length}). " +
                "Check that the same embedding model generated both vectors.", nameof(b));
        }

        float dot = 0f, magA = 0f, magB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return magA == 0f || magB == 0f ? 0f : dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}
