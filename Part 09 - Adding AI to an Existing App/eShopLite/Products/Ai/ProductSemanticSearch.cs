using DataEntities;
using Microsoft.Extensions.VectorData;

namespace Products.Ai;

/// <summary>
/// Semantic search over the product catalog. This sits alongside the existing keyword
/// search rather than replacing it - keyword search is still the right tool when the
/// shopper knows the product name.
/// </summary>
public class ProductSemanticSearch(
    VectorStoreCollection<int, ProductVector> collection,
    ILogger<ProductSemanticSearch> logger)
{
    /// <summary>
    /// Builds the search index from the relational catalog. Called once at startup.
    /// </summary>
    public async Task InitializeAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        await collection.EnsureCollectionExistsAsync(ct);

        var records = products.Select(p => new ProductVector
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            // Embedding the name together with the description gives the model more to
            // work with than the description alone.
            EmbeddingSource = $"{p.Name}. {p.Description}"
        });

        await collection.UpsertAsync(records, ct);
        logger.LogInformation("Product search index is ready.");
    }

    /// <summary>
    /// Finds the ids of products related to a natural language query, best match first.
    /// The vector store is only an index - the database stays the source of truth, so the
    /// caller hydrates these ids back into full products.
    /// </summary>
    /// <param name="maxDistance">
    /// Vector search always returns its nearest neighbours, even when nothing is actually
    /// relevant. Without a ceiling, "socket wrench" would still return a sleeping bag.
    /// SQLite scores with cosine distance, so lower is a closer match.
    /// </param>
    public async Task<List<int>> SearchAsync(
        string query,
        int maxResults = 3,
        double maxDistance = 0.75,
        CancellationToken ct = default)
    {
        var matches = new List<int>();

        await foreach (var result in collection.SearchAsync(query, maxResults, cancellationToken: ct))
        {
            logger.LogInformation(
                "Semantic match {Name} scored {Score:F3}", result.Record.Name, result.Score);

            if (result.Score > maxDistance)
            {
                continue;
            }

            matches.Add(result.Record.Id);
        }

        return matches;
    }
}
