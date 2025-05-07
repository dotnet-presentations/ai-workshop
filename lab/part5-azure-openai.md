# Using Azure OpenAI

## In this lab

In this lab, you will learn how to migrate your application from using GitHub Models during development to Azure OpenAI for production. Azure OpenAI provides enterprise-grade AI capabilities with global scale, enhanced security, and dedicated support.

## Azure OpenAI Overview

Azure OpenAI is Microsoft's cloud offering for OpenAI's models. It provides:

- **Enterprise-grade security**: Compliance with Microsoft's Azure security standards
- **Scalability**: Ability to handle high volumes of requests
- **Service SLAs**: Guaranteed uptime and performance
- **Regional availability**: Deployment in multiple Azure regions
- **Model management**: Advanced features for model deployment and versioning
- **Monitoring and logging**: Built-in monitoring and logging capabilities

## Step 1: Setting Up Azure OpenAI

To use Azure OpenAI, you need to set up resources in Azure:

1. **Create an Azure OpenAI resource**:
   - Navigate to the Azure portal (<https://portal.azure.com>)
   - Click "Create a resource" and search for "Azure OpenAI"
   - Fill in the required details:
     - Subscription
     - Resource group
     - Region
     - Name
     - Pricing tier
   - Click "Review + create" and then "Create"

2. **Deploy models**:
   - Navigate to your Azure OpenAI resource
   - Select "Model deployments" from the left menu
   - Deploy the following models:
     - `gpt-35-turbo` for chat completions
     - `text-embedding-ada-002` for embeddings

## Step 2: Obtain API Keys and Endpoints

1. **Get the API key**:
   - Navigate to your Azure OpenAI resource
   - Select "Keys and Endpoint" from the left menu
   - Copy one of the keys and the endpoint URL

2. **Update your connection string**:

Change from GitHub Models connection string:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://models.inference.ai.azure.com"
  }
}
```

To Azure OpenAI connection string:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://YOUR_RESOURCE_NAME.openai.azure.com;Key=YOUR_API_KEY"
  }
}
```

## Step 3: Configure Azure OpenAI in Your Application

Update your `Program.cs` file to use Azure OpenAI instead of GitHub Models:

```csharp
// Comment out GitHub Models
// builder.AddGitHubModels();

// Add Azure OpenAI
builder.AddAzureOpenAI(configureSettings: settings =>
{
    // Optional: Configure model deployments if they don't match the default names
    settings.TextCompletionModelDeploymentName = "gpt-35-turbo";
    settings.EmbeddingModelDeploymentName = "text-embedding-ada-002";
});
```

The beauty of Microsoft Extensions for AI is that the rest of your code remains unchanged. The same interfaces (`IChatClient` and `IEmbeddingGenerator`) work with both GitHub Models and Azure OpenAI.

## Step 4: Handling Connection Strings in Different Environments

For a smooth development workflow, you can use different connection strings in different environments:

1. Update `appsettings.Development.json` to use GitHub Models:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://models.inference.ai.azure.com"
  }
}
```

1. Update `appsettings.Production.json` to use Azure OpenAI:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://YOUR_RESOURCE_NAME.openai.azure.com;Key=YOUR_API_KEY"
  }
}
```

1. Use environment-specific configuration in `Program.cs`:

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.AddGitHubModels();
}
else
{
    builder.AddAzureOpenAI();
}
```

## Step 5: Advanced Azure OpenAI Features

Azure OpenAI provides additional features not available in GitHub Models:

### Content Filtering

Azure OpenAI includes content filtering to ensure appropriate AI responses:

```csharp
builder.AddAzureOpenAI(configureSettings: settings =>
{
    settings.TextCompletionModelDeploymentName = "gpt-35-turbo";
    settings.EmbeddingModelDeploymentName = "text-embedding-ada-002";
    
    // Configure content filtering
    settings.ContentFiltering = new ContentFilterSettings
    {
        FilterSeverity = ContentFilterSeverity.Medium
    };
});
```

### Advanced Model Options

Azure OpenAI allows for more advanced model configuration:

```csharp
var response = await _chatClient.GetResponseAsync(
    new[]
    {
        new ChatMessage(ChatRole.System, "You are a helpful assistant."),
        new ChatMessage(ChatRole.User, userMessage)
    },
    new ChatCompletionOptions
    {
        Temperature = 0.7f,
        MaxTokens = 800,
        TopP = 0.95f,
        FrequencyPenalty = 0.0f,
        PresencePenalty = 0.0f,
        ResponseFormat = ChatResponseFormat.Text
    });
```

## Step 6: Implementing Azure OpenAI-Specific Features

Let's add a feature that takes advantage of Azure OpenAI's capabilities. We'll add a method to the ProductService that generates multiple product category suggestions with different temperatures:

```csharp
public async Task<List<string>> GenerateAlternativeCategoriesAsync(string productName, string description)
{
    var categories = new List<string>();
    
    try
    {
        var prompt = $@"Product: {productName}
Description: {description}

Please suggest three alternative product categories for this item. 
Each category should be a single word or short phrase.
Format the response as a simple comma-separated list.";

        // First call with low temperature (more predictable)
        var lowTempResponse = await _chatClient.GetResponseAsync(
            new[]
            {
                new ChatMessage(ChatRole.System, "You are a product categorization specialist."),
                new ChatMessage(ChatRole.User, prompt)
            },
            new ChatCompletionOptions { Temperature = 0.3f });
            
        // Second call with high temperature (more creative)
        var highTempResponse = await _chatClient.GetResponseAsync(
            new[]
            {
                new ChatMessage(ChatRole.System, "You are a creative product marketing specialist."),
                new ChatMessage(ChatRole.User, prompt)
            },
            new ChatCompletionOptions { Temperature = 0.9f });
            
        // Parse and combine responses
        categories.AddRange(ParseCategories(lowTempResponse.Text));
        categories.AddRange(ParseCategories(highTempResponse.Text));
        
        // Remove duplicates and return
        return categories.Distinct().ToList();
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Error generating alternative categories for {ProductName}", productName);
        return new List<string> { "Alternative", "Option", "Miscellaneous" };
    }
}

private List<string> ParseCategories(string? text)
{
    if (string.IsNullOrEmpty(text))
        return new List<string>();
        
    return text.Split(',')
        .Select(c => c.Trim())
        .Where(c => !string.IsNullOrEmpty(c))
        .ToList();
}
```

## Step 7: Load Testing with Azure OpenAI

When deploying to production, it's important to understand the performance characteristics of Azure OpenAI:

1. **Tokens per minute (TPM) limits**: Azure OpenAI resources have TPM limits based on your pricing tier
2. **Concurrent requests**: Plan for simultaneous user requests
3. **Response latency**: Understand the typical response times for your usage pattern

Consider implementing:

- Request queuing for high-load scenarios
- Caching frequently requested AI responses
- Rate limiting to prevent exceeding quota

## Migrating from GitHub Models to Azure OpenAI

When you're ready to migrate to production with Azure OpenAI, follow these steps:

1. **Provision Azure OpenAI resources** as described in Step 1
2. **Update connection strings** in production environments
3. **Test thoroughly** with Azure OpenAI before full deployment
4. **Monitor usage and costs** to optimize your implementation
5. **Implement caching** for frequently used AI responses to reduce costs

## What You've Learned

- How to set up and configure Azure OpenAI resources
- How to migrate from GitHub Models to Azure OpenAI
- How to use advanced Azure OpenAI features
- Best practices for production deployments with Azure OpenAI

## Next Steps

Now that you know how to use Azure OpenAI in your application, proceed to [Deploying the Application](part6-deploying.md) to learn how to deploy your AI application to production.
