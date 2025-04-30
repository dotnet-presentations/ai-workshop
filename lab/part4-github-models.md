# Using GitHub Models

## In this lab

In this lab, you will learn how to integrate GitHub Models into your .NET application. GitHub Models provides free access to a variety of AI models that can be used during development, making it an excellent choice for building and testing AI applications.

## GitHub Models Overview

GitHub Models is a service that provides free access to AI models for developers. Key benefits include:

- **Free to use**: No cost for development scenarios
- **Ease of use**: Simple integration with Microsoft Extensions for AI
- **No authentication required**: Uses anonymous access for development
- **Multiple model types**: Supports text generation and embeddings

GitHub Models provides access to:

- Text generation models for chat and completions
- Embedding models for vector representations
- Advanced capabilities like function calling and JSON mode

## Step 1: Configuring GitHub Models

GitHub Models integration is already set up in the template project. Let's examine how it's configured:

1. **In Program.cs file**:

```csharp
// Add GitHub Models for development
builder.AddGitHubModels();
```

This simple call does several things:

- Registers GitHub Models as the default AI provider
- Sets up IChatClient for text generation
- Sets up IEmbeddingGenerator for creating embeddings
- Configures client settings for the GitHub endpoints

2. **Setting the connection string**:

For GitHub Models, the connection string is very simple:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://models.inference.ai.azure.com"
  }
}
```

This connects to the GitHub Models endpoint at models.inference.ai.azure.com without requiring an API key.

## Step 2: Understanding GitHub Models Capabilities

GitHub Models supports several capabilities:

### Text Generation

GitHub Models provides a chat completion model similar to GPT models:

```csharp
public class MyService(IChatClient chatClient)
{
    public async Task<string> GetChatResponseAsync(string userMessage)
    {
        var response = await chatClient.GetResponseAsync(new[]
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant."),
            new ChatMessage(ChatRole.User, userMessage)
        });
        
        return response.Text;
    }
}
```

### Embeddings

GitHub Models also provides an embedding model:

```csharp
public class MyService(IEmbeddingGenerator<string, Embedding<float>> embedder)
{
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var embedding = await embedder.GenerateEmbeddingVectorAsync(text);
        return embedding.Values;
    }
}
```

### Structured Output with JSON Mode

GitHub Models supports generating structured JSON output:

```csharp
public async Task<ProductInfo> GenerateProductInfoAsync(string description)
{
    var response = await _chatClient.GetResponseAsync(
        new[]
        {
            new ChatMessage(ChatRole.System, "You are a product information assistant. Respond with valid JSON only."),
            new ChatMessage(ChatRole.User, $"Generate product information for: {description}")
        },
        new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.Json }
    );
    
    return JsonSerializer.Deserialize<ProductInfo>(response.Text);
}
```

## Step 3: Implementing Enhanced Product Descriptions

Let's add a feature to generate enhanced product descriptions using GitHub Models.

1. Add a new method to the `ProductService` class:

```csharp
public async Task<string> GenerateEnhancedDescriptionAsync(string productName, string shortDescription)
{
    try
    {
        var prompt = $@"Product name: {productName}
Short description: {shortDescription}

Write a marketing-focused enhanced description for this product. 
The description should be 2-3 sentences highlighting key features and benefits.
Keep the tone professional but engaging.";

        var response = await _chatClient.GetResponseAsync(
            new[]
            {
                new ChatMessage(ChatRole.System, "You are a product marketing specialist."),
                new ChatMessage(ChatRole.User, prompt)
            });

        return response.Text?.Trim() ?? 
            $"Discover the incredible {productName}. This high-quality product delivers outstanding performance and reliability.";
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error generating enhanced description for {ProductName}", productName);
        return $"Discover the incredible {productName}. This high-quality product delivers outstanding performance and reliability.";
    }
}
```

2. Add a method to get the enhanced description for a specific product:

```csharp
public async Task<string> GetEnhancedDescriptionAsync(Guid productId)
{
    var product = await _dbContext.Products.FindAsync(productId);
    if (product == null)
    {
        return "Product not found";
    }

    return await GenerateEnhancedDescriptionAsync(product.Name, product.ShortDescription);
}
```

3. Create a new component `Components/ProductDetail.razor`:

```csharp
@using MyGenAiLab.Web.Models
@using MyGenAiLab.Web.Services
@inject ProductService ProductService

<div class="product-detail">
    @if (IsLoading)
    {
        <div class="loading">Loading enhanced description...</div>
    }
    else if (!string.IsNullOrEmpty(EnhancedDescription))
    {
        <div class="enhanced-description">
            <h3>About this product</h3>
            <p>@EnhancedDescription</p>
        </div>
    }
</div>

<style>
    .product-detail {
        margin-top: 1rem;
    }

    .enhanced-description {
        background-color: #f9f9ff;
        padding: 1rem;
        border-radius: 0.25rem;
        border-left: 4px solid #3a4ed5;
    }

    .loading {
        color: #666;
        font-style: italic;
    }
</style>

@code {
    [Parameter]
    public ProductInfo Product { get; set; } = null!;

    private bool IsLoading { get; set; } = false;
    private string EnhancedDescription { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (Product != null)
        {
            IsLoading = true;
            EnhancedDescription = await ProductService.GetEnhancedDescriptionAsync(Product.Id);
            IsLoading = false;
        }
    }
}
```

4. Update the Products.razor page to include the detail component:

```csharp
<PropertyColumn Property="@(p => p.Name)" Title="📦 Product Name" Sortable="true">
    <ColumnOptions>
        <button @onclick="() => ShowProductDetail(p)">Details</button>
    </ColumnOptions>
</PropertyColumn>

@if (SelectedProduct != null)
{
    <div class="detail-panel">
        <h2>@SelectedProduct.Name</h2>
        <p>@SelectedProduct.ShortDescription</p>
        <p><strong>Category:</strong> @SelectedProduct.Category</p>
        
        <ProductDetail Product="@SelectedProduct" />
        
        <button class="btn-default" @onclick="CloseDetail">Close</button>
    </div>
}

@code {
    private ProductInfo? SelectedProduct { get; set; }

    private void ShowProductDetail(ProductInfo product)
    {
        SelectedProduct = product;
    }

    private void CloseDetail()
    {
        SelectedProduct = null;
    }
}
```

## Step 4: Understanding GitHub Models Limitations

When using GitHub Models, keep these limitations in mind:

1. **Rate Limits**: GitHub Models has usage limits for free access
2. **Performance**: Response times may be slower than paid services
3. **Model Selection**: Limited model options compared to Azure OpenAI
4. **Development Only**: Not intended for production workloads

## Best Practices for GitHub Models

1. **Error Handling**: Always implement robust error handling for AI calls
2. **Fallback Strategies**: Provide fallbacks for when AI generation fails
3. **Content Filtering**: Be aware that GitHub Models has content filtering
4. **Prompt Engineering**: Carefully design prompts for best results
5. **Local Development**: Use GitHub Models for local development before moving to Azure OpenAI for production

## What You've Learned

- How to configure and use GitHub Models with .NET
- How to generate text using the GitHub Models endpoint
- How to implement robust AI features with proper error handling
- Best practices for working with AI models in development

## Next Steps

Now that you understand how to use GitHub Models for development, proceed to [Azure OpenAI Integration](part5-azure-openai.md) to learn how to migrate your application to use Azure OpenAI for production scenarios.
