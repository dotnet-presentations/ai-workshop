# Vector Data and Embeddings

## In this lab

In this lab, you will learn about vector embeddings and how to use Microsoft Extensions for AI to implement semantic search with a vector database. You'll understand how text is converted into numerical vectors that enable efficient similarity matching.

## Understanding Vector Embeddings

Vector embeddings are numerical representations of text that capture semantic meaning. They allow us to:

- Represent words, sentences, or documents as points in high-dimensional space
- Calculate similarity between texts based on their semantic meaning
- Search for content based on meaning rather than just keywords
- Cluster similar content together

## Step 1: Understanding the Semantic Search Service

Examine the `SemanticSearch.cs` file to understand how semantic search is implemented:

```csharp
namespace GenAiLab.Web.Services;

public class SemanticSearch(
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    IVectorStore vectorStore,
    ILogger<SemanticSearch> logger)
{
    private const string CollectionName = "data-genailab-ingested";
    
    public async Task<SearchResults> Search(string query)
    {
        try
        {
            // Generate an embedding vector for the query
            var queryEmbedding = await embedder.GenerateEmbeddingVectorAsync(query);
            
            // Search the vector database for similar document chunks
            var collection = vectorStore.GetCollection<Guid, SemanticSearchRecord>(CollectionName);
            var searchResults = await collection.VectorizedSearchAsync(
                queryEmbedding,
                new VectorSearchOptions<SemanticSearchRecord> { Top = 5 }
            );
            
            // Process and return results
            var results = new List<DocumentResult>();
            await foreach (var match in searchResults.Results)
            {
                results.Add(new DocumentResult
                {
                    FileName = match.Record.FileName,
                    Text = match.Record.Text,
                    Score = match.Score
                });
            }
            
            return new SearchResults(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error performing semantic search");
            return new SearchResults(new List<DocumentResult>());
        }
    }
}

public record DocumentResult
{
    public required string FileName { get; init; }
    public required string Text { get; init; }
    public float Score { get; init; }
}

public record SearchResults(IReadOnlyList<DocumentResult> Results);
```

## Step 2: Understanding the Vector Data Structure

Examine the `SemanticSearchRecord.cs` file:

```csharp
namespace GenAiLab.Web.Services;

public class SemanticSearchRecord
{
    public required string FileName { get; set; }
    public required string Text { get; set; }
}
```

This simple class represents the data stored in the vector database:

- `FileName`: The source document's name
- `Text`: A chunk of text from the document
- Each record is stored with its corresponding embedding vector

## Step 3: Exploring Document Ingestion

Examine how documents are processed and stored in the vector database by looking at the `DataIngestor.cs` file:

```csharp
public class DataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    IVectorStore vectorStore,
    IServiceProvider serviceProvider,
    ILogger<DataIngestor> logger)
{
    private const string CollectionName = "data-genailab-ingested";
    
    public async Task IngestAllSourcesAsync()
    {
        // Get ingestion sources
        var sources = serviceProvider.GetServices<IIngestionSource>();
        
        foreach (var source in sources)
        {
            await IngestFromSourceAsync(source);
        }
    }
    
    private async Task IngestFromSourceAsync(IIngestionSource source)
    {
        // Create or get the vector collection
        var collection = vectorStore.GetCollection<Guid, SemanticSearchRecord>(CollectionName);
        
        // Get documents from the source
        var documents = await source.GetDocumentsAsync();
        
        foreach (var doc in documents)
        {
            // Split the document into chunks
            var chunks = SplitTextIntoChunks(doc.Content, chunkSize: 1000);
            
            foreach (var chunk in chunks)
            {
                // Generate embedding for each chunk
                var embedding = await embedder.GenerateEmbeddingVectorAsync(chunk);
                
                // Store the chunk and its embedding in the vector database
                await collection.StoreAsync(
                    Guid.NewGuid(),
                    new SemanticSearchRecord
                    {
                        FileName = doc.FileName,
                        Text = chunk
                    },
                    embedding
                );
            }
        }
    }
    
    private List<string> SplitTextIntoChunks(string text, int chunkSize)
    {
        // Implementation of text chunking
    }
}
```

Key steps in document ingestion:

1. Documents are obtained from ingestion sources (like PDF files)
2. Each document is split into smaller chunks
3. Embeddings are generated for each chunk
4. Each chunk and its embedding are stored in the vector database

## Step 4: Using the PDF Directory Source

Examine how PDF documents are read from a directory:

```csharp
public class PDFDirectorySource(ILogger<PDFDirectorySource> logger) : IIngestionSource
{
    public async Task<IEnumerable<Document>> GetDocumentsAsync()
    {
        var documents = new List<Document>();
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Data");
        
        if (!Directory.Exists(dataDirectory))
        {
            logger.LogWarning("Data directory not found: {Directory}", dataDirectory);
            return documents;
        }
        
        // Get all PDF files
        var pdfFiles = Directory.GetFiles(dataDirectory, "*.pdf");
        
        foreach (var pdfFile in pdfFiles)
        {
            try
            {
                // Extract text from PDF
                string text = await ExtractTextFromPdfAsync(pdfFile);
                
                documents.Add(new Document
                {
                    FileName = Path.GetFileName(pdfFile),
                    Content = text
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extracting text from {FileName}", pdfFile);
            }
        }
        
        return documents;
    }
    
    private async Task<string> ExtractTextFromPdfAsync(string pdfPath)
    {
        // PDF text extraction implementation
    }
}
```

## Step 5: Understanding Qdrant Vector Database

Qdrant is the vector database used in this application. It provides:

- High-performance vector similarity search
- Filtering capabilities
- Support for multiple collections
- REST API for vector operations

The application uses the Qdrant container managed by .NET Aspire:

```csharp
// In AppHost's Program.cs
var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
```

## Step 6: Embedding Generation with GitHub Models

The application uses GitHub Models to generate embeddings:

```csharp
// In Web's Program.cs
builder.AddGitHubModels();
```

This registers:

- `IEmbeddingGenerator<string, Embedding<float>>` for text-to-vector conversion
- GitHub's embedding model that produces 1536-dimension vectors

## Exercise: Implement Semantic Search in the Products Feature

Enhance the Products page to include a search function:

1. Add a search box to the Products page:

```html
<div class="search-container">
    <input @bind-value="SearchQuery" @bind-value:event="oninput" placeholder="Search products..." />
    <button class="search-button" @onclick="SearchProducts">Search</button>
</div>
```

1. Add the search functionality to the code-behind:

```csharp
private string SearchQuery { get; set; } = string.Empty;

private async Task SearchProducts()
{
    if (string.IsNullOrWhiteSpace(SearchQuery))
    {
        // Reset to all products
        await LoadData();
        return;
    }

    // Generate embedding for the search query
    var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync(SearchQuery);
    
    // Search for products with similar descriptions
    var allProducts = await ProductService.GetProductsAsync();
    
    // Sort products by relevance to the query
    // This is a simplified approach - in a real app you would use the vector database
    AllProducts = allProducts.OrderByDescending(p => 
        p.ShortDescription.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
        .AsQueryable();
}
```

## What You've Learned

- How vector embeddings represent text in a high-dimensional space
- How to use Qdrant vector database for semantic search
- How document ingestion works with chunking and embedding generation
- How to implement semantic search using Microsoft Extensions for AI

## Next Steps

Now that you understand vector data and embeddings, proceed to [Using GitHub Models](part4-github-models.md) to learn more about GitHub's AI models.
