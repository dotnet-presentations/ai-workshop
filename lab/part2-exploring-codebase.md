# Building the Products Feature

## In this lab

In this lab, you'll extend your AI Web Chat application by adding a Products feature that uses AI to generate product descriptions and categories. You'll learn how to craft effective prompts for AI models and work with JSON responses.

## Overview of the Products Feature

The Products feature allows users to:

- View a list of products with AI-generated descriptions
- Filter products by category
- See products categorized by AI based on their documentation

## Step 1: Create the Product Models

1. Create a new file `Models/ProductInfo.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace MyGenAiLab.Web.Models;

public class ProductInfo
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string Category { get; set; }
    public required string FileName { get; set; }

    // For filtering
    public static List<string> AvailableCategories { get; set; } = new List<string>();
}

public class ProductCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
```

2. Create a database context for products in `Services/ProductDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MyGenAiLab.Web.Models;

namespace MyGenAiLab.Web.Services;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<ProductInfo> Products => Set<ProductInfo>();
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
}
```

## Step 2: Create the Product Service

Create a new file `Services/ProductService.cs` to generate product information using AI:

```csharp
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using MyGenAiLab.Web.Models;
using System.Text;
using OpenAI;
using Microsoft.EntityFrameworkCore;

namespace MyGenAiLab.Web.Services;

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

    // Additional implementation will be added in the next steps
}
```

## Step 3: Implement Product Generation with AI

Add the following methods to the `ProductService` class:

```csharp
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
```

## Step 4: Implement AI-Based Product Description Generation

Add the following methods to use the AI service for generating product descriptions and categories:

```csharp
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
            });

        // Try to parse the JSON response
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
```

## Step 5: Register the Services

Update your `Program.cs` file to register the new services:

```csharp
// Add database support
builder.Services.AddDatabase<ProductDbContext>();

// Register product service
builder.Services.AddScoped<ProductService>();
```

## Step 6: Create the Products Page

Create a new file `Components/Pages/Products.razor`:

```csharp
@page "/products"
@using MyGenAiLab.Web.Models
@using MyGenAiLab.Web.Services
@using Microsoft.AspNetCore.Components.QuickGrid
@using System.Linq.Expressions
@inject ProductService ProductService

<PageTitle>Products - GenAI Lab</PageTitle>

<h1>📦 Our Products</h1>

@if (AllProducts == null)
{
    <div class="message-box">
        <span>🔄 Loading products...</span>
    </div>
}
else if (!FilteredProducts.Any())
{
    <div class="message-box">
        <span>📦 No products found</span>
    </div>
}
else
{
    <div>
        <select @bind="CategoryFilter" @bind:after="StateHasChanged">
            <option value="">✨ All Categories</option>
            @foreach (var category in Categories)
            {
                <option value="@category">📁 @category</option>
            }
        </select>

        <div class="product-table-container">
            <QuickGrid Items="@FilteredProducts">
                <PropertyColumn Property="@(p => p.Name)" Title="📦 Product Name" Sortable="true" />
                <PropertyColumn Property="@(p => p.ShortDescription)" Title="📝 Description" />
                <PropertyColumn Property="@(p => p.Category)" Title="🏷️ Category" Sortable="true" />
            </QuickGrid>
        </div>
    </div>
}

<style>
    h1 {
        margin-bottom: 1.5rem;
    }

    select {
        padding: 0.5rem;
        border: 1px solid #ccc;
        border-radius: 0.25rem;
        margin-bottom: 1rem;
        align-self: flex-end;
    }

    .message-box {
        padding: 1rem;
        margin-bottom: 1.5rem;
        border-left: 4px solid #3a4ed5;
        background-color: #f0f4ff;
        border-radius: 0.25rem;
    }

    .product-table-container {
        margin-bottom: 2rem;
        border: 1px solid #e0e0e0;
        border-radius: 0.25rem;
        overflow: hidden;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    ::deep table {
        width: 100%;
        border-collapse: collapse;
    }

    ::deep th {
        background-color: #f5f5f5;
        font-weight: 600;
        text-align: left;
        padding: 0.75rem 1rem;
        border-bottom: 2px solid #ddd;
    }

    ::deep td {
        padding: 0.75rem 1rem;
        border-bottom: 1px solid #eee;
    }

    ::deep tr:nth-child(even) {
        background-color: #f9f9f9;
    }

    ::deep tr:hover {
        background-color: #f0f4ff;
    }

    ::deep .col-options-button {
        color: #3a4ed5;
    }

    ::deep .col-options-menu {
        padding: 0.75rem;
        border-radius: 0.25rem;
    }
</style>

@code {
    private IQueryable<ProductInfo>? AllProducts;
    private List<string> Categories { get; set; } = new List<string>();
    private string CategoryFilter { get; set; } = string.Empty;

    private IQueryable<ProductInfo> FilteredProducts
    {
        get
        {
            if (AllProducts == null)
                return Enumerable.Empty<ProductInfo>().AsQueryable();

            if (string.IsNullOrEmpty(CategoryFilter))
                return AllProducts;

            return AllProducts.Where(p => p.Category == CategoryFilter);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        Categories = await ProductService.GetCategoriesAsync();
        var products = await ProductService.GetProductsAsync();
        AllProducts = products.AsQueryable();
    }
}
```

## Step 7: Update the Navigation

Add a link to the Products page in `Components/Pages/Chat/ChatHeader.razor`:

```csharp
<div class="chat-header-container main-background-gradient">
    <div class="chat-header-controls page-width" style="display: flex; gap: 8px; align-items: center;">
        <button class="btn-default" @onclick="@OnNewChat">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5"
                stroke="currentColor" class="new-chat-icon">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            New chat
        </button>
        <button class="btn-subtle" onclick="location.href='/products'" style="display: inline-flex; align-items: center;">
            📦 Products
        </button>
    </div>

    <h1 class="page-width">MyGenAiLab.Web</h1>
</div>
```

## Testing the Products Feature

1. Run your application and navigate to the Products page
2. You should see a list of products with AI-generated descriptions
3. Try filtering products by category using the dropdown

## What You've Learned

- How to use Microsoft Extensions for AI to generate product descriptions
- How to prompt AI models for structured JSON responses
- How to handle and parse JSON responses from AI models
- How to use vector search to find relevant content for AI processing
- How to create a user interface that displays and filters AI-generated content

## Key AI Concepts

- **Prompt Engineering**: Crafting effective prompts for AI to generate structured data
- **JSON Response Parsing**: Working with structured responses from AI models
- **Error Handling**: Implementing fallbacks when AI processing fails
- **Vector Search**: Using embeddings to find relevant document content

## Next Steps

Now that you've implemented the Products feature, proceed to [Vector Data and Embeddings](part3-vector-data.md) to learn more about how the semantic search functionality works.
