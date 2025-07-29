# Write a New Products Page

## In this lab

In this lab, you'll enhance your application by creating a Products page that leverages AI to automatically generate product descriptions and categorize items based on their content. This lab demonstrates several key concepts in modern AI application development:

🧩 **What we're building:**

- A product catalog service that uses AI to analyze product documentation
- A vector database system to store both product metadata and searchable content
- A user interface that displays and filters AI-processed products

🔍 **Key technical concepts you'll learn:**

- **AI Service Abstraction**: Work with `IChatClient` interface that allows you to interact with AI models without being tied to a specific provider (like GitHub Models or Azure OpenAI)
  
- **Vector Database as Primary Store**: Learn how vector databases can serve as both storage and search engine, eliminating the need for separate databases

- **Simplified Data Flow**: See how data flows through a streamlined architecture:
  
  ```mermaid
  %%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#f4f4f4', 'primaryTextColor': '#000', 'primaryBorderColor': '#333', 'lineColor': '#333', 'secondaryColor': '#e1f5fe', 'tertiaryColor': '#f3e5f5' }}}%%
  flowchart LR
    PDF[Product PDFs] -->|Ingestion| VDB[(Vector DB - Chunks)]
    VDB -->|Document Analysis| AI{AI Model}
    AI -->|Product Info + Embedding| PDB[(Vector DB - Products)]
    User(User Query) -->|Filter/Search| PDB
    PDB -->|Results| UI[UI Display]
    
    style PDF fill:#f9d5e5
    style VDB fill:#e1f5fe
    style AI fill:#d5e8d4
    style PDB fill:#e8f5e8
  ```

- **Prompt Engineering for Structured Data**: Design AI prompts that return structured JSON responses for reliable data processing

- **Vector Store Collections**: Use Qdrant collections to store different types of data (chunks, documents, and products) in the same database

The Products feature showcases how AI can enhance applications using a simplified, single-database architecture that reduces infrastructure complexity while providing powerful search and storage capabilities.

> [!IMPORTANT]
> **Simplified Architecture Benefits**
> This approach uses **Qdrant as the single data store** for both document chunks and product metadata. This eliminates the complexity of PostgreSQL setup and Entity Framework configuration, reducing setup time to **15-25 minutes**.
>
> **What's Different**: Instead of managing two databases (PostgreSQL + Qdrant), we store everything in Qdrant vector collections, making the solution simpler to deploy and maintain.

## Create the Product Models

First, we need to define the models that will store our AI-generated product information in the vector database.

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

1. In the same folder, create a new file `ProductVector.cs` for storing products in the vector database:

```csharp
using Microsoft.Extensions.VectorData;

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
}
```

## Create the Product Service

Now let's create a service that will use AI to analyze the ingested PDF documents and generate product information, storing it all in Qdrant.

1. In the folder `Services` of the project `src/start/GenAiLab.Web`, create a new file `ProductService.cs` with the following content:

```csharp
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
```

## Create the Products UI

Now let's create the user interface for displaying our AI-generated products.

1. Create a new folder called `Components` > `Pages` in the `src/start/GenAiLab.Web` project (if it doesn't already exist).

1. In the `Components/Pages` folder, create a new file called `Products.razor` with the following content:

```razor
@page "/products"
@rendermode InteractiveServer
@inject ProductService ProductService
@inject IJSRuntime JSRuntime

<PageTitle>Products - GenAI Lab</PageTitle>

<div class="container mt-4">
    <div class="row">
        <div class="col-12">
            <h1 class="mb-4">🛍️ Product Catalog</h1>
            <p class="lead text-muted mb-4">
                AI-generated product descriptions and categories based on ingested documentation
            </p>
        </div>
    </div>

    <div class="row mb-4">
        <div class="col-md-4">
            <div class="form-floating">
                <select class="form-select" id="categoryFilter" @bind="selectedCategory" @bind:after="OnCategoryChanged">
                    <option value="">All Categories</option>
                    @foreach (var category in categories)
                    {
                        <option value="@category">@category</option>
                    }
                </select>
                <label for="categoryFilter">Filter by Category</label>
            </div>
        </div>
        <div class="col-md-8 d-flex align-items-end">
            <div class="text-muted">
                Showing @filteredProducts.Count() of @allProducts.Count() products
            </div>
        </div>
    </div>

    @if (isLoading)
    {
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading products...</span>
            </div>
            <p class="mt-3 text-muted">Loading AI-generated products...</p>
        </div>
    }
    else if (!filteredProducts.Any())
    {
        <div class="alert alert-info">
            <h4 class="alert-heading">No Products Found</h4>
            <p>No products match your current filter. Try selecting a different category or clear the filter to see all products.</p>
        </div>
    }
    else
    {
        <div class="row">
            @foreach (var product in filteredProducts)
            {
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h5 class="card-title text-primary">@product.Name</h5>
                                <span class="badge bg-secondary">@product.Category</span>
                            </div>
                            <p class="card-text text-muted">@product.ShortDescription</p>
                        </div>
                        <div class="card-footer bg-transparent">
                            <small class="text-muted">
                                <i class="bi bi-file-earmark-pdf"></i> @product.FileName
                            </small>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
</div>

@code {
    private List<ProductInfo> allProducts = new();
    private List<ProductInfo> filteredProducts = new();
    private List<string> categories = new();
    private string selectedCategory = "";
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            allProducts = (await ProductService.GetProductsAsync()).ToList();
            categories = await ProductService.GetCategoriesAsync();
            filteredProducts = allProducts.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnCategoryChanged()
    {
        filteredProducts = string.IsNullOrEmpty(selectedCategory)
            ? allProducts.ToList()
            : allProducts.Where(p => p.Category == selectedCategory).ToList();

        StateHasChanged();
    }
}
```

## Update Navigation

Let's add a link to our new Products page in the navigation.

1. Open the file `src/start/GenAiLab.Web/Components/Layout/ChatHeader.razor`

1. Look for the existing navigation buttons and add a new Products button. Find this section in the file:

```razor
<div class="nav-buttons">
    <a href="/" class="btn btn-outline-light me-2">
        <i class="bi bi-chat-dots"></i> Chat
    </a>
</div>
```

1. Replace it with this updated version that includes the Products button:

```razor
<div class="nav-buttons">
    <a href="/" class="btn btn-outline-light me-2">
        <i class="bi bi-chat-dots"></i> Chat
    </a>
    <a href="/products" class="btn btn-outline-light">
        <i class="bi bi-grid-3x3-gap"></i> Products
    </a>
</div>
```

## Add Required Packages

We need to add one package for the QuickGrid component used in our UI.

1. Right-click on the `src/start/GenAiLab.Web` project and select "Manage NuGet Packages"

1. Click on the "Browse" tab and search for `Microsoft.AspNetCore.Components.QuickGrid`

1. Install the latest version (should be 9.0.x)

Alternatively, you can run this command in the terminal from the `src/start/GenAiLab.Web` directory:

```bash
dotnet add package Microsoft.AspNetCore.Components.QuickGrid
```

## Update Program.cs for Services

Now we need to register our new services and vector collection in the dependency injection container.

1. Open `src/start/GenAiLab.Web/Program.cs`

1. Add the ProductVector model to the using statements at the top:

```csharp
using GenAiLab.Web.Models;
```

1. Find the section where vector collections are registered (look for the Qdrant collection registrations) and add the ProductVector collection:

```csharp
builder.Services.AddQdrantCollection<Guid, IngestedChunk>("data-genailab-chunks");
builder.Services.AddQdrantCollection<Guid, IngestedDocument>("data-genailab-documents");
builder.Services.AddQdrantCollection<Guid, ProductVector>("data-genailab-products"); // Add this line
```

1. Add the ProductService registration after the other service registrations:

```csharp
// Register product service
builder.Services.AddScoped<ProductService>();
```

Your updated `Program.cs` should look like this:

```csharp
using Microsoft.Extensions.AI;
using GenAiLab.Web.Components;
using GenAiLab.Web.Services;
using GenAiLab.Web.Services.Ingestion;
using GenAiLab.Web.Models;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var openai = builder.AddAzureOpenAIClient("openai");
openai.AddChatClient("gpt-4o-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
openai.AddEmbeddingGenerator("text-embedding-3-small");

builder.AddQdrantClient("vectordb");
builder.Services.AddQdrantCollection<Guid, IngestedChunk>("data-genailab-chunks");
builder.Services.AddQdrantCollection<Guid, IngestedDocument>("data-genailab-documents");
builder.Services.AddQdrantCollection<Guid, ProductVector>("data-genailab-products");
builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();

// Register product service
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.Run();
```

## Build and Test

1. **Build the solution**: Press Ctrl+Shift+B (or use the Build menu) to ensure everything compiles correctly.

1. **Run the application**: Press F5 to start the application with debugging.

1. **Test the Products page**: 
   - Navigate to the Products page using the new navigation button
   - Wait for the AI to analyze the PDF documents and generate product information
   - Try filtering by different categories
   - Verify that products display with AI-generated descriptions

## How It Works

Your Products feature now works entirely with Qdrant as the data store:

1. **Product Generation**: When you visit the Products page for the first time, the service automatically:
   - Scans the vector database for ingested PDF documents
   - Extracts content for each document
   - Uses AI to generate product descriptions and categories
   - Stores the product information as vectors in Qdrant

1. **Vector Storage**: Products are stored with both metadata (name, description, category) and vector embeddings for potential semantic search

1. **Simplified Architecture**: No PostgreSQL setup required - everything uses Qdrant collections

1. **AI Integration**: The service uses your configured AI provider (GitHub Models or Azure OpenAI) to process content and generate structured product information

## Troubleshooting

If you encounter issues:

1. **Build Errors**: Clean and rebuild the solution:
   ```bash
   dotnet clean
   dotnet build
   ```

1. **No Products Shown**: Ensure your PDF files are in the `/wwwroot/Data` directory and that the ingestion process completed successfully

1. **AI Processing Issues**: Check the console logs for AI response parsing errors - the service includes fallback mechanisms for malformed responses

## Summary

You've successfully implemented a Products page that demonstrates:

- ✅ **Simplified vector-first architecture** using only Qdrant for data storage
- ✅ **AI-powered content analysis** to generate product descriptions automatically  
- ✅ **Structured data extraction** from unstructured PDF documents
- ✅ **Category-based filtering** with a clean, responsive UI
- ✅ **Provider-agnostic AI integration** that works with any configured AI service

This approach eliminates the complexity of managing multiple databases while providing the same functionality with better performance and simpler deployment.
