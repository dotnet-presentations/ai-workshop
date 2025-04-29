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
        // Get products from database
        await EnsureProductsExistAsync();

        // Get from database with optional filtering
        IQueryable<ProductInfo> query = _dbContext.Products;

        // Apply category filter if provided
        if (!string.IsNullOrEmpty(categoryFilter))
        {
            query = query.Where(p => p.Category == categoryFilter);
        }

        return await query.ToListAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        // Ensure products are initialized
        await EnsureProductsExistAsync();

        // Get categories from database
        var categories = await _dbContext.Categories
            .Select(c => c.Name)
            .OrderBy(c => c)
            .ToListAsync();

        return categories;
    }

    private async Task EnsureProductsExistAsync()
    {
        // Check if we already have products in the database
        if (!await _dbContext.Products.AnyAsync())
        {
            // If not, generate them
            var products = await GenerateProductInfoAsync();

            // Update ProductInfo.AvailableCategories (used for UI)
            var categories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            ProductInfo.AvailableCategories = categories;

            // Store the categories in the database
            foreach (var categoryName in categories)
            {
                _dbContext.Categories.Add(new ProductCategory { Name = categoryName });
            }

            await _dbContext.SaveChangesAsync();
        }
        else
        {
            // Set available categories for UI components
            var categories = await _dbContext.Categories
                .Select(c => c.Name)
                .OrderBy(c => c)
                .ToListAsync();

            ProductInfo.AvailableCategories = categories;
        }
    }
    private async Task<List<ProductInfo>> GenerateProductInfoAsync()
    {
        var products = new List<ProductInfo>();
        var vectorCollection = _vectorStore.GetCollection<Guid, SemanticSearchRecord>("data-genailab-ingested");

        try
        {
            // Simple approach to get all unique filenames
            var dummyEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync("all documents");
            var searchResults = await vectorCollection.VectorizedSearchAsync(
                dummyEmbedding,
                new VectorSearchOptions<SemanticSearchRecord> { Top = 1000 });

            var allRecords = new List<SemanticSearchRecord>();
            await foreach (var result in searchResults.Results)
            {
                allRecords.Add(result.Record);
            }

            var uniqueFileNames = allRecords.Select(r => r.FileName).Distinct().ToList();

            foreach (var fileName in uniqueFileNames)
            {
                // Extract product name from filename
                var productName = GetProductNameFromFileName(fileName);

                // Get content for this product
                var contentQuery = $"Information about {productName}";
                var contentEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync(contentQuery);
                var contentResults = await vectorCollection.VectorizedSearchAsync(contentEmbedding,
                    new VectorSearchOptions<SemanticSearchRecord>
                    {
                        Top = 5,
                        Filter = record => record.FileName == fileName
                    });

                // Collect content
                var contentBuilder = new StringBuilder();
                await foreach (var item in contentResults.Results)
                {
                    contentBuilder.AppendLine(item.Record.Text);
                }

                // Get description and category from AI
                var content = contentBuilder.ToString();
                var (shortDescription, category) = await GenerateDescriptionAndCategory(content, productName, fileName);

                // Create product and add to DB context
                var product = new ProductInfo
                {
                    Name = productName,
                    ShortDescription = shortDescription,
                    Category = category,
                    FileName = fileName
                };

                _dbContext.Products.Add(product);
                products.Add(product);
            }

            // Save all products to the database
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during product info generation");
        }

        return products;
    }
    private string GetProductNameFromFileName(string fileName)
    {
        // Simple name extraction from filename
        return Path.GetFileNameWithoutExtension(fileName)
            .Replace("Example_", "")
            .Replace("_", " ");
    }
    private async Task<(string Description, string Category)> GenerateDescriptionAndCategory(string content, string productName, string fileName)
    {
        try
        {
            // Create a simple prompt for both description and category
            var prompt = $@"Based on this content about '{productName}', provide:
1. A concise product description (max 200 characters)
2. A category from: Electronics, Safety Equipment, Outdoor Gear, or General

Format as:
DESCRIPTION: [description]
CATEGORY: [category]

Content: {content}";

            // Get response from the chat client
            var chatResponse = await _chatClient.GetResponseAsync(
                new[] { new ChatMessage(ChatRole.System, "You are a product information assistant."),
                        new ChatMessage(ChatRole.User, prompt) });

            string responseText = chatResponse.Text ?? string.Empty;

            // Set default values
            string description = $"A high-quality {productName.ToLower()}";
            string category = "General";

            // Simple parsing 
            int descIndex = responseText.IndexOf("DESCRIPTION:", StringComparison.OrdinalIgnoreCase);
            int catIndex = responseText.IndexOf("CATEGORY:", StringComparison.OrdinalIgnoreCase);

            if (descIndex >= 0 && catIndex > descIndex)
            {
                // Extract description and category
                description = responseText.Substring(descIndex + "DESCRIPTION:".Length, catIndex - descIndex - "DESCRIPTION:".Length).Trim();
                category = responseText.Substring(catIndex + "CATEGORY:".Length).Trim();

                // Simple category normalization
                if (category.Contains("Electronic", StringComparison.OrdinalIgnoreCase))
                    category = "Electronics";
                else if (category.Contains("Safety", StringComparison.OrdinalIgnoreCase))
                    category = "Safety Equipment";
                else if (category.Contains("Outdoor", StringComparison.OrdinalIgnoreCase))
                    category = "Outdoor Gear";
                else
                    category = "General";

                // Trim description if needed
                if (description.Length > 200)
                {
                    description = description.Substring(0, 197) + "...";
                }
            }
            else
            {
                // If format wasn't followed, use simple defaults
                _logger.LogInformation("AI response format not as expected for {ProductName}, using defaults", productName);
            }

            return (description, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product info for {ProductName}", productName);
            // Simple fallback with generic values
            return ($"A high-quality {productName.ToLower()}", "General");
        }
    }
}
