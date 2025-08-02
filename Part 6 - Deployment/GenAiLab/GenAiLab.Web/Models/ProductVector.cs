using Microsoft.Extensions.VectorData;
using System.Text.Json.Serialization;

namespace GenAiLab.Web.Models;

/// <summary>
/// Represents a product stored in the vector database with both metadata and searchable content
/// </summary>
public class ProductVector
{
    private const int VectorDimensions = 1536; // 1536 is the default vector size for the OpenAI text-embedding-3-small model
    private const string VectorDistanceFunction = DistanceFunction.CosineSimilarity;

    [VectorStoreKey]
    public Guid Id { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string Name { get; set; }

    [VectorStoreData]
    public required string ShortDescription { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string Category { get; set; }

    [VectorStoreData]
    public required string FileName { get; set; }

    [VectorStoreData]
    public string FullContent { get; set; } = string.Empty;

    [VectorStoreVector(VectorDimensions, DistanceFunction = VectorDistanceFunction)]
    public ReadOnlyMemory<float> Vector { get; set; }

    /// <summary>
    /// Convert to ProductInfo for UI compatibility
    /// </summary>
    public ProductInfo ToProductInfo()
    {
        return new ProductInfo
        {
            Id = Id,
            Name = Name,
            ShortDescription = ShortDescription,
            Category = Category,
            FileName = FileName
        };
    }

    /// <summary>
    /// Create from ProductInfo and content
    /// </summary>
    public static ProductVector FromProductInfo(ProductInfo productInfo, string fullContent, ReadOnlyMemory<float> vector)
    {
        return new ProductVector
        {
            Id = productInfo.Id,
            Name = productInfo.Name,
            ShortDescription = productInfo.ShortDescription,
            Category = productInfo.Category,
            FileName = productInfo.FileName,
            FullContent = fullContent,
            Vector = vector
        };
    }
}
