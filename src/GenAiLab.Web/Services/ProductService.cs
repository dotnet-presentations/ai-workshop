using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using GenAiLab.Web.Models;
using System.Text;
using OpenAI;
using Microsoft.EntityFrameworkCore;

namespace GenAiLab.Web.Services;

public class ProductService(
        IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator,
        IVectorStore _vectorStore,
        ProductDbContext _dbContext,
        IChatClient _chatClient,
        ILogger<ProductService> _logger)
{
    public async Task<IEnumerable<ProductInfo>> GetProductsAsync(string? categoryFilter = null)
    {
        // Make sure we have products
        await EnsureProductsExistAsync();

        // Simple filtering by category if specified
        var query = string.IsNullOrEmpty(categoryFilter)
            ? _dbContext.Products
            : _dbContext.Products.Where(p => p.Category == categoryFilter);

        return await query.ToListAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        await EnsureProductsExistAsync();
        return await _dbContext.Categories.Select(c => c.Name).ToListAsync();
    }

    private async Task EnsureProductsExistAsync()
    {
        if (!await _dbContext.Products.AnyAsync())
        {
            await GenerateAndSaveProductsAsync();
        }
    }

    private async Task GenerateAndSaveProductsAsync()
    {
        // Get documents from vector store
        var fileNames = await GetUniqueFileNamesAsync();
        if (fileNames.Count == 0)
        {
            _logger.LogWarning("No documents found in vector store");
            return;
        }

        var categories = new HashSet<string>();

        // Process each file
        foreach (var fileName in fileNames)
        {
            var productName = Path.GetFileNameWithoutExtension(fileName)
                .Replace("Example_", "")
                .Replace("_", " ");

            // Get document content
            var content = await GetDocumentContentAsync(fileName, productName);

            // The key part - using AI to generate product info
            var (description, category) = await AskAIForProductInfoAsync(content, productName);

            // Save to database
            _dbContext.Products.Add(new ProductInfo
            {
                Name = productName,
                ShortDescription = description,
                Category = category,
                FileName = fileName
            });

            categories.Add(category);
        }

        // Save categories
        foreach (var category in categories)
        {
            _dbContext.Categories.Add(new ProductCategory { Name = category });
        }

        ProductInfo.AvailableCategories = categories.ToList();
        await _dbContext.SaveChangesAsync();
    }

    private async Task<List<string>> GetUniqueFileNamesAsync()
    {
        var vectorCollection = _vectorStore.GetCollection<Guid, SemanticSearchRecord>("data-genailab-ingested");

        try
        {
            var dummyEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync("all documents");
            var searchResults = await vectorCollection.VectorizedSearchAsync(
                dummyEmbedding,
                new VectorSearchOptions<SemanticSearchRecord> { Top = 1000 });

            var uniqueFileNames = new HashSet<string>();
            await foreach (var result in searchResults.Results)
            {
                uniqueFileNames.Add(result.Record.FileName);
            }

            return uniqueFileNames.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents from vector store");
            return new List<string>();
        }
    }

    private async Task<string> GetDocumentContentAsync(string fileName, string productName)
    {
        var vectorCollection = _vectorStore.GetCollection<Guid, SemanticSearchRecord>("data-genailab-ingested");

        try
        {
            var contentEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync($"Information about {productName}");
            var contentResults = await vectorCollection.VectorizedSearchAsync(
                contentEmbedding,
                new VectorSearchOptions<SemanticSearchRecord>
                {
                    Top = 5,
                    Filter = record => record.FileName == fileName
                });

            var contentBuilder = new StringBuilder();
            await foreach (var item in contentResults.Results)
            {
                contentBuilder.AppendLine(item.Record.Text);
            }

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting content for {FileName}", fileName);
            return string.Empty;
        }
    }

    // Simple record for JSON deserialization
    private record ProductResponse(string Description, string Category);

    // This is the key method that uses IChatClient
    private async Task<(string Description, string Category)> AskAIForProductInfoAsync(string content, string productName)
    {
        try
        {
            // Create a simple prompt requesting JSON response
            var prompt = $@"Based on this content about '{productName}', provide a JSON object with these properties:
1. description: A concise product description (max 200 characters)
2. category: One of: 'Electronics', 'Safety Equipment', 'Outdoor Gear', or 'General'

Content: {content}";

            // Get response from the chat client
            var chatResponse = await _chatClient.GetResponseAsync(
                new[] {
                    new ChatMessage(ChatRole.System, "You are a product information assistant. Respond with valid JSON only."),
                    new ChatMessage(ChatRole.User, prompt)
                });            // Try to parse the JSON response
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var responseJson = System.Text.Json.JsonSerializer.Deserialize<ProductResponse>(chatResponse.Text, options);

            if (responseJson != null)
            {
                return (responseJson.Description, responseJson.Category);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("AI processing error for {ProductName}: {Error}", productName, ex.Message);
        }

        // Simple fallback
        return ($"A high-quality {productName}", "General");
    }
}
