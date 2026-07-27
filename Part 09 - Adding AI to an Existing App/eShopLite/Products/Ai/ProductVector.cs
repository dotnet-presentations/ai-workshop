using Microsoft.Extensions.VectorData;

namespace Products.Ai;

/// <summary>
/// The catalog record as it is stored in the vector database. This is deliberately
/// separate from <see cref="DataEntities.Product"/>: the relational model is the source
/// of truth, and this is a derived search index built from it.
/// </summary>
public class ProductVector
{
    [VectorStoreKey]
    public int Id { get; set; }

    [VectorStoreData]
    public string Name { get; set; } = string.Empty;

    [VectorStoreData]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The text that gets embedded. Because the vector store is configured with an
    /// embedding generator, assigning a string here is enough - the connector calls the
    /// embedding model on upsert and on search.
    /// </summary>
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
    public string EmbeddingSource { get; set; } = string.Empty;
}
