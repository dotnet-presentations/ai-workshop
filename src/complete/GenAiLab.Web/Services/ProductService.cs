using Microsoft.Extensions.VectorData;
using GenAiLab.Web.Models;
using System.Text;
using Microsoft.Extensions.AI;

namespace GenAiLab.Web.Services;

public class ProductService(
        VectorStoreCollection<Guid, IngestedChunk> _chunkCollection,
        VectorStoreCollection<Guid, ProductVector> _productCollection,
        IChatClient _chatClient,
        IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator,
        ILogger<ProductService> _logger)
{
    private static readonly List<string> _availableCategories = new();

    public async Task<IEnumerable<ProductInfo>> GetProductsAsync(string? categoryFilter = null)
    {
        // Ensure products exist in vector store
        await EnsureProductsExistAsync();

        // Get all products from vector store
        var products = new List<ProductInfo>();
        
        try
        {
            // Get all products using the GetAsync method
            var allProducts = await _productCollection.GetAsync(product => true, top: 1000).ToListAsync();

            foreach (var productVector in allProducts)
            {
                var product = productVector.ToProductInfo();
                
                // Apply category filter if specified
                if (string.IsNullOrEmpty(categoryFilter) || product.Category == categoryFilter)
                {
                    products.Add(product);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products from vector store");
        }

        return products.OrderBy(p => p.Name);
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        await EnsureProductsExistAsync();
        return _availableCategories.ToList();
    }

    private async Task EnsureProductsExistAsync()
    {
        try
        {
            // Ensure collection exists
            await _productCollection.EnsureCollectionExistsAsync();

            // Check if we already have products in the vector store
            var existingProducts = await _productCollection.GetAsync(product => true, top: 1).ToListAsync();
            
            if (!existingProducts.Any())
            {
                await GenerateAndSaveProductsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for existing products");
            // Try to generate products anyway
            await GenerateAndSaveProductsAsync();
        }
    }

    private async Task GenerateAndSaveProductsAsync()
    {
        // Get unique document filenames from the chunk collection
        var fileNames = await GetUniqueFileNamesAsync();
        if (fileNames.Count == 0)
        {
            _logger.LogWarning("No documents found in vector store for product generation");
            return;
        }

        var categories = new HashSet<string>();
        var products = new List<ProductVector>();

        // Process each file to create products
        foreach (var fileName in fileNames)
        {
            try
            {
                var productName = Path.GetFileNameWithoutExtension(fileName)
                    .Replace("Example_", "")
                    .Replace("_", " ");

                // Get document content from chunks
                var content = await GetDocumentContentAsync(fileName, productName);
                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("No content found for file: {FileName}", fileName);
                    continue;
                }

                // Use AI to generate product information
                var (description, category) = await AskAIForProductInfoAsync(content, productName);
                
                // Generate embedding for the product (combination of name, description, and content)
                var searchableText = $"{productName} {description} {content}";
                var embedding = await _embeddingGenerator.GenerateAsync(searchableText);

                // Create product vector
                var productVector = new ProductVector
                {
                    Id = Guid.NewGuid(),
                    Name = productName,
                    ShortDescription = description,
                    Category = category,
                    FileName = fileName,
                    FullContent = content,
                    Vector = embedding.Vector
                };

                products.Add(productVector);
                categories.Add(category);

                _logger.LogInformation("Generated product: {ProductName} in category: {Category}", productName, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", fileName);
            }
        }

        // Save all products to vector store
        if (products.Count > 0)
        {
            try
            {
                await _productCollection.UpsertAsync(products);
                _availableCategories.Clear();
                _availableCategories.AddRange(categories.OrderBy(c => c));
                
                _logger.LogInformation("Successfully saved {Count} products to vector store", products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving products to vector store");
            }
        }
    }

    private async Task<List<string>> GetUniqueFileNamesAsync()
    {
        try
        {
            // Get all chunks and extract unique document IDs
            var allChunks = await _chunkCollection.GetAsync(chunk => true, top: 1000).ToListAsync();
            var uniqueFileNames = allChunks
                .Where(chunk => !string.IsNullOrEmpty(chunk.DocumentId))
                .Select(chunk => chunk.DocumentId)
                .Distinct()
                .ToList();

            return uniqueFileNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document filenames from vector store");
            return new List<string>();
        }
    }

    private async Task<string> GetDocumentContentAsync(string fileName, string productName)
    {
        try
        {
            // Get chunks for this specific document
            var chunks = await _chunkCollection.GetAsync(chunk => chunk.DocumentId == fileName, top: 10).ToListAsync();

            var contentBuilder = new StringBuilder();
            foreach (var chunk in chunks)
            {
                contentBuilder.AppendLine(chunk.Text);
            }

            return contentBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting content for file: {FileName}", fileName);
            return string.Empty;
        }
    }

    // Simple record for JSON deserialization
    private record ProductResponse(string Description, string Category);

    private async Task<(string Description, string Category)> AskAIForProductInfoAsync(string content, string productName)
    {
        try
        {
            // Create a prompt for generating product information
            var prompt = $@"Based on this content about '{productName}', provide a JSON object with these properties:
1. description: A concise product description (max 200 characters)
2. category: One of: 'Electronics', 'Safety Equipment', 'GPS', 'Backpack', 'Outdoor Gear', or 'General'

Return ONLY the raw JSON object without any markdown formatting, code blocks, or backticks.

Content: {content}";

            // Get response from the chat client
            var chatResponse = await _chatClient.GetResponseAsync(
                new[] {
                    new ChatMessage(ChatRole.System, "You are a product information assistant. Respond with valid JSON only, no markdown formatting or backticks."),
                    new ChatMessage(ChatRole.User, prompt)
                });

            // Clean and parse the response
            string cleanedResponse = chatResponse.Text
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var responseJson = System.Text.Json.JsonSerializer.Deserialize<ProductResponse>(cleanedResponse, options);

            if (responseJson != null)
            {
                return (responseJson.Description, responseJson.Category);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI processing error for {ProductName}", productName);
        }

        // Fallback response
        return ($"A high-quality {productName.ToLowerInvariant()}", "General");
    }
}
