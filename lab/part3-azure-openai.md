# Convert from GitHub Models to Azure OpenAI

## In this lab

In this lab, you will learn how to migrate your application from using GitHub Models during development to Azure OpenAI for production. You'll understand how the common interfaces in Microsoft Extensions for AI make this migration seamless, create an Azure OpenAI resource, deploy models, and update your application's configuration.

## Understand the IChatClient as a common interface across services

Microsoft Extensions for AI provides common interfaces that work across different AI providers:

- `IChatClient` for text generation
- `IEmbeddingGenerator` for creating vector embeddings
- `ITextCompletion` for text completions

These interfaces allow you to switch between AI providers without changing your application code. For example, when using `IChatClient`:

```csharp
public class MyService
{
    private readonly IChatClient _chatClient;

    public MyService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GetResponseAsync(string userMessage)
    {
        var response = await _chatClient.GetResponseAsync(
            new[] {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, userMessage)
            });
        
        return response.Text;
    }
}
```

The same code works whether the `IChatClient` is implemented by GitHub Models, Azure OpenAI, or another provider.

## Create the Azure OpenAI resource

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

## Deploy the `gpt-4o-mini` model for chat completions

After creating your Azure OpenAI resource, you need to deploy the models:

1. Navigate to your Azure OpenAI resource in the Azure portal
1. Select "Model deployments" from the left menu
1. Click "+ Create new deployment"
1. Select the model "gpt-4o-mini" for chat completions
1. Enter a deployment name (e.g., "gpt-4o-mini")
1. Set the token rate limit and other settings as needed
1. Click "Create"

## Deploy the `text-embedding-3-small` model for embeddings

Follow the same process to deploy the embedding model:

1. Navigate to your Azure OpenAI resource in the Azure portal
1. Select "Model deployments" from the left menu
1. Click "+ Create new deployment"
1. Select the model "text-embedding-3-small" for embeddings
1. Enter a deployment name (e.g., "text-embedding-3-small")
1. Set the token rate limit and other settings as needed
1. Click "Create"

## Obtain the Endpoint and API Keys

To connect your application to Azure OpenAI, you need the endpoint and API key:

1. Navigate to your Azure OpenAI resource in the Azure portal
1. Select "Keys and Endpoint" from the left menu
1. Copy the Endpoint URL (it will look like `https://YOUR_RESOURCE_NAME.openai.azure.com/`)
1. Copy one of the Keys (either Key 1 or Key 2)

## Update the connection strings

Now you'll update your application's connection string to use Azure OpenAI instead of GitHub Models:

1. In the Solution Explorer, right-click on the `GenAiLab.AppHost` project and select "Manage User Secrets"

1. In the `secrets.json` file, update the connection string:

   ```json
   {
     "ConnectionStrings:openai": "Endpoint=https://YOUR_RESOURCE_NAME.openai.azure.com/;Key=YOUR_API_KEY"
   }
   ```

   Replace `YOUR_RESOURCE_NAME` with your Azure OpenAI resource name and `YOUR_API_KEY` with the API key you copied.

## Add new Product PDFs for ingestion

To test the new Azure OpenAI integration, let's add some product PDF files for ingestion:

1. In the Solution Explorer, navigate to the `GenAiLab.Web/wwwroot/Data` folder

1. Right-click on the `Data` folder and select "Add" > "Existing Item..."

1. Browse to the location where you have product PDF files (or create some simple PDFs about products)

1. Select the PDF files and click "Add"

For this lab, you can use the following product PDFs:

- Emergency Survival Kit.pdf
- Hiking Backpack.pdf
- Rechargeable Flashlight.pdf
- First Aid Kit.pdf
- Solar Power Bank.pdf

## Run the application

Now let's run the application with Azure OpenAI integration:

1. Make sure the `GenAiLab.AppHost` project is set as the startup project

1. Press F5 or click the "Start Debugging" button in Visual Studio

1. The .NET Aspire dashboard will open in your browser

1. Shortly after, the web application will launch in another browser tab

1. Test the chat functionality with Azure OpenAI by asking questions like:
   - "What products do you have information about?"
   - "Tell me about the emergency survival kit"
   - "What's the most useful feature of the solar power bank?"

1. Notice how the responses now come from Azure OpenAI rather than GitHub Models

## What You've Learned

- How Microsoft Extensions for AI provides common interfaces across AI providers
- How to create an Azure OpenAI resource in the Azure portal
- How to deploy models for chat completions and embeddings
- How to obtain the endpoint and API keys for Azure OpenAI
- How to update your application to use Azure OpenAI instead of GitHub Models
- How to test the application with Azure OpenAI integration

## Next Steps

Now that you've migrated your application to use Azure OpenAI, proceed to [Write a New Products Page](part4-products-page.md) to enhance your application with a Products feature that uses AI to generate product descriptions and categories.
