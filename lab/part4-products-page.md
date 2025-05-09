# Write a New Products Page

## In this lab

In this lab, you'll enhance your application by creating a Products page that uses AI to generate product descriptions and categories. You'll learn how to implement product models, create a service to interact with AI, generate product information, and build a user interface to display the results.

## Create the Product Models

1. Add a new folder named `Models` to the project `src/start/GenAiLab.Web`, by right-clicking on the project and selecting "Add" > "New Folder".

1. In this new folder, create a new file `ProductInfo.cs` and replace the content with the following code:

```csharp
using System;
using System.Collections.Generic;

namespace GenAiLab.Web.Models;

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
```

1. In the folder `Services` of the project `src/start/GenAiLab.Web`, create a database context for products in `ProductDbContext.cs`, and replace the content with the following code:

```csharp
using Microsoft.EntityFrameworkCore;
using GenAiLab.Web.Models;

namespace GenAiLab.Web.Services;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<ProductInfo> Products { get; set; }
    public DbSet<ProductCategory> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure cascade delete for products and categories
        modelBuilder.Entity<ProductInfo>()
            .HasKey(p => p.Id);
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        dbContext.Database.EnsureCreated();

        // Seed initial categories if none exist
        if (!dbContext.Categories.Any())
        {
            dbContext.Categories.AddRange(
                new ProductCategory { Name = "Electronics" },
                new ProductCategory { Name = "Safety Equipment" },
                new ProductCategory { Name = "Outdoor Gear" },
                new ProductCategory { Name = "General" }
            );
            dbContext.SaveChanges();
        }

        ProductInfo.AvailableCategories = dbContext.Categories
            .Select(c => c.Name)
            .ToList();
    }
}

// New entity for storing categories
public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

## Create the Product Service

In the folder `Services` of the project `src/start/GenAiLab.Web`, create a new file `ProductService.cs` to generate product information using AI. Replace the content with the following code:

```csharp
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
        // Check if we need to generate products first
        if (!await _dbContext.Products.AnyAsync())
        {
            await GenerateAndSaveProductsAsync();
        }

        // Return products, optionally filtered by category
        if (string.IsNullOrEmpty(categoryFilter))
        {
            return await _dbContext.Products.ToListAsync();
        }
        else
        {
            return await _dbContext.Products
                .Where(p => p.Category == categoryFilter)
                .ToListAsync();
        }
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        if (!ProductInfo.AvailableCategories.Any())
        {
            ProductInfo.AvailableCategories = await _dbContext.Categories
                .Select(c => c.Name)
                .ToListAsync();
        }
        
        return ProductInfo.AvailableCategories;
    }
}
```

## Implement Product Generation with AI

Add the following methods to the `ProductService` class:

```csharp
private async Task GenerateAndSaveProductsAsync()
{
    // Get documents from vector store
    var fileNames = await GetUniqueFileNamesAsync();
    
    if (!fileNames.Any())
    {
        _logger.LogWarning("No documents found in vector store. Make sure PDFs are ingested.");
        return;
    }

    var categories = new HashSet<string>();

    // Process each file
    foreach (var fileName in fileNames)
    {
        var productName = Path.GetFileNameWithoutExtension(fileName);
        var documentContent = await GetDocumentContentAsync(fileName, productName);
        
        if (string.IsNullOrWhiteSpace(documentContent))
        {
            continue;
        }

        // Get product description and category from AI
        var (description, category) = await AskAIForProductInfoAsync(documentContent, productName);
        categories.Add(category);

        // Create and save product
        var product = new ProductInfo
        {
            Id = Guid.NewGuid(),
            Name = productName.Replace("_", " "),
            ShortDescription = description,
            Category = category,
            FileName = fileName
        };

        _dbContext.Products.Add(product);
    }

    // Save categories
    foreach (var category in categories)
    {
        if (!await _dbContext.Categories.AnyAsync(c => c.Name == category))
        {
            _dbContext.Categories.Add(new ProductCategory { Name = category });
        }
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
        var allDocuments = await vectorCollection.VectorizedSearchAsync(
            dummyEmbedding,
            new VectorSearchOptions<SemanticSearchRecord> { Top = 1000 }
        );

        var uniqueFileNames = new HashSet<string>();
        await foreach (var item in allDocuments.Results)
        {
            uniqueFileNames.Add(item.Record.FileName);
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
                Filter = new VectorSearchFilter<SemanticSearchRecord>(r => r.FileName == fileName)
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

## Implement AI-Based Product Description Generation

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
1. category: One of: 'Electronics', 'Safety Equipment', 'Outdoor Gear', or 'General'

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

## Register the Services

In the project `src/start/GenAiLab.Web`, update your `Program.cs` file to register the new services. Just before the `var app = builder.Build();` line, add the following code:

```csharp
// Add database support
builder.AddSqliteDbContext<ProductDbContext>("productDb");

// Register product service
builder.Services.AddScoped<ProductService>();
```

And after the `var app = builder.Build();` line, add initialization for the ProductDbContext:

```csharp
IngestionCacheDbContext.Initialize(app.Services);
ProductDbContext.Initialize(app.Services); // Add this line
```

## Create the Products Page

Let's use the new AspNetCore QuickGrid component to display the products. First, we need to add the Nuget package `Microsoft.AspNetCore.Components.QuickGrid`.

There are multiple ways to do this:

- Open the GenAiLab.Web project file and add at the end of the packages `<ItemGroup>`

    ```xml
    <PackageReference Include="Microsoft.AspNetCore.Components.QuickGrid" Version="9.0.4" />
    ```

or

- Type the follinging command in the Package Manager Console:

    ```powershell
    NuGet\Install-Package Microsoft.AspNetCore.Components.QuickGrid -Version 9.0.4
    ```

Create a new file `Components/Pages/Products.razor`:

```csharp
@page "/products"
@using GenAiLab.Web.Models
@using GenAiLab.Web.Services
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

## Update the Navigation

In the GenAiLab.Web project, locate the file `Components/Layout/MainLayout.razor` and update it to include a link to the Products page.

If your project uses a different navigation structure, find the appropriate file (such as `NavMenu.razor` or `ChatHeader.razor`) and add a navigation link to the Products page:

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
        @* 👇👇👇 Add the button here 👇👇👇 *@
        <button class="btn-subtle" onclick="location.href='/products'" style="display: inline-flex; align-items: center;">
            📦 Products
        </button>
        @* 👆👆👆 Add the button here 👆👆👆 *@
    </div>

    <h1 class="page-width">GenAiLab.Web</h1>
</div>
```

## Testing the Products Feature

1. Run your application and navigate to the Products page
1. You should see a list of products with AI-generated descriptions
1. Try filtering products by category using the dropdown

## What You've Learned

- How to create models and database contexts for a new feature
- How to build a service that interacts with AI to generate product information
- How to use vector embeddings to find relevant document content
- How to prompt AI models for structured JSON responses
- How to handle and parse JSON responses from AI models
- How to create a user interface that displays and filters AI-generated content

## Next Steps

Now that you've implemented the Products feature, proceed to [Deploy to Azure](part5-deploy-azure.md) to learn how to prepare your application for production deployment to Azure using the Azure Developer CLI.
