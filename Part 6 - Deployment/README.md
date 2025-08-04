# Deploy to Azure

> **⏱️ Estimated Time:** 30-45 minutes (excluding Azure provisioning time ~6 minutes)

## In this workshop

In this workshop, you will learn how to deploy your AI application to Azure using the Azure Developer CLI (`azd`). You'll deploy your PostgreSQL-based application to Azure Container Apps for production use.

> [!TIP]
> If you haven't completed the previous steps in the lab or are having trouble with your code, you can use the working code snapshot provided in this `Part 6 - Deployment` folder. The complete code has already been updated with the necessary configuration for external HTTP endpoints and deployment. You can skip directly to the "Set Up the Azure Developer CLI" section and deploy that code instead.

## Configure the web application for external access

  Before the web application is deployed to Azure Container Apps, you will need to configure it so that it is available via web browser. Update **AppHost** `Program.cs` to add the following line just before the call to `builder.Build().Run();` at the end of the file:

  ```csharp
  webApp.WithExternalHttpEndpoints();
  ```

## Set Up the Azure Developer CLI

1. **Install the Azure Developer CLI (azd)**:

   If you don't already have the Azure Developer CLI installed, you can install it with:

   ```powershell
   winget install microsoft.azd
   ```

   Or using PowerShell:

   ```powershell
   irm https://aka.ms/install-azd.ps1 | iex
   ```

1. Close and re-open the terminal to make sure *azd* has been added to the path.

1. **Login to Azure**:

   ```powershell
   azd auth login
   ```

## Deploy to Azure Container Apps

> [!IMPORTANT]
> **Vector Database Configuration**: This deployment uses **Qdrant** as the vector database, which runs as a containerized service in Azure Container Apps. No additional vector database setup is required.
>
> If you prefer to use **Azure AI Search** instead, see the "Alternative: Azure AI Search Deployment" section below.

1. Ensure you are in the root directory which contains the solution file.

1. **Initialize your Azure environment**:

   ```powershell
   # Initialize the application for managment with azd
   azd init
   ```

1. When prompted with "How do you want to initialize your app?", select the default: "Use code in the current directory"

1. After scanning the directory, `azd` prompts you to confirm that it found the correct .NET Aspire *AppHost* project. Select the **Confirm and continue initializing my app** option.

1. When prompted to "Enter a unique environment name", enter "mygenaiapp" or choose something else if you would like.

> [!NOTE]
> If you are using a managed environment, use the environment name provided by your instructor or organization. Otherwise, you can choose any name you prefer.

1. **Provision Azure resources**:

   ```powershell
   azd provision
   ```

   This command creates all the necessary Azure resources, including:
   - Resource group
   - Container registry
   - Container apps environment
   - Container apps for your application and Qdrant vector database
   - Log Analytics workspace

> [!NOTE]
> When provisioning resources with `azd`, it will automatically create a resource group with the prefix "rg-" added to your environment name (e.g., "rg-mygenaiapp").
  
1. When prompted to select a location, select a region close to you (e.g., "West US 3" or another nearby Azure datacenter).

1. When prompted to "Enter a value for the 'openai' infrastructure secured parameter", enter your Azure OpenAI connection string in the format:

   ```text
   Endpoint=https://your-resource.openai.azure.com/;Key=your-api-key
   ```

   Or use your environment variable:

   ```text
   Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY
   ```

1. Press enter and watch as your resources are provisioned! You can either just follow along in the terminal, or you can click on the link to watch the progress in the Azure portal. Provisioning should take roughly 5 minutes, but may take longer during conference events as multiple concurrent deployments can slow things down.

1. **Deploy your application code**:

   ```powershell
   azd deploy
   ```

   This command:
   - Builds your .NET application
   - Creates container images for both the web app and Qdrant
   - Pushes them to the Azure Container Registry
   - Deploys them to Azure Container Apps
  
   This should take roughly 2 minutes, but may take longer under busy conditions.

1. **Access your deployed application**:

   After deployment completes, you'll receive a URL to access your application in the terminal output. You can also view it using:

   ```powershell
   azd show
   ```

## Alternative: Azure AI Search Deployment

If you prefer to use Azure AI Search instead of Qdrant for vector storage, follow these steps:

> [!NOTE]
> Azure AI Search provides a managed vector database service but requires additional Azure resources and configuration.

### Prerequisites for Azure AI Search

1. **Create an Azure AI Search service** in the Azure portal:
   - Go to Azure portal → Create a resource → Azure AI Search
   - Choose a pricing tier (Basic or higher for vector search)
   - Note the endpoint URL and admin key

1. **Update your application configuration**:
   - Modify `Program.cs` to use Azure AI Search instead of Qdrant
   - Update connection strings and service registrations
   - Install Azure AI Search NuGet packages

### Azure AI Search Deployment Steps

1. **Add Azure AI Search credentials to the credential setup script**:

   Add to `.github/scripts/setup-workshop-credentials.ps1`:

   ```powershell
   $env:WORKSHOP_AZURE_SEARCH_ENDPOINT = "https://your-search-service.search.windows.net"
   $env:WORKSHOP_AZURE_SEARCH_KEY = "your-admin-key"
   ```

1. **Follow the same azd deployment steps** as above, but when prompted for infrastructure parameters:
   - Provide your Azure AI Search connection string when prompted for 'azureAISearch'
   - Format: `Endpoint=https://your-search-service.search.windows.net;Key=your-admin-key`

### Benefits of Azure AI Search vs Qdrant

**Azure AI Search advantages:**

- Fully managed service (no container management)
- Built-in security and compliance features
- Integration with other Azure AI services
- Advanced filtering and faceting capabilities

**Qdrant advantages:**

- Simpler deployment (runs as container)
- Open source and vendor-neutral
- Lower cost for development scenarios
- Faster setup and testing

## Manage Your Deployment

Once deployed, you can manage your deployment using various Azure Developer CLI commands:

1. **View deployment information**:

   ```powershell
   azd show
   ```

   This command shows your deployment details, including endpoints and resource information. Launch the link for the *aichatweb-app** service and verify that it is continuing to run as it did locally.

1. **Monitor your application**:

   ```powershell
   azd monitor
   ```

   This opens the Application Insights dashboard for your application, where you can view logs, metrics, and performance data.

1. **Update your deployment**:

   After making changes to your application:

   ```powershell
   azd deploy
   ```

1. **Delete your deployment**:

   To completely clean up all resources when you're done:

   ```powershell
   azd down --purge --force
   ```

## Production Considerations

### Security Best Practices

1. **Secure your API keys**:
   - Use Azure Key Vault for storing API keys and secrets
   - Never hardcode keys in your application code
   - Rotate keys periodically

1. **Implement proper authentication and authorization**:
   - Add authentication to your application
   - Protect API endpoints
   - Consider identity providers like Azure AD

1. **Use HTTPS everywhere**:
   - Enable HTTPS for all endpoints
   - Configure proper CORS policies

### Scaling and Performance

1. **Configure scaling rules in Azure Container Apps**:
   - Set minimum and maximum replicas
   - Configure scaling metrics based on load

1. **Implement caching for AI responses**:
   - Use distributed caching (Redis)
   - Cache common AI-generated content

1. **Optimize network communication**:
   - Use gRPC for internal service communication
   - Configure appropriate timeouts

### Cost Management

1. **Monitor AI service usage**:
   - Track token usage with telemetry
   - Set up cost alerts and budgets

1. **Optimize embedding generation**:
   - Only generate embeddings when necessary
   - Cache embedding results

1. **Configure appropriate instance sizes**:
   - Start with smaller instances and scale up as needed
   - Use autoscaling to optimize costs

## What You've Learned

- How to use the Azure Developer CLI (azd) to deploy your AI application
- How to set up and configure Azure Container Apps for production workloads
- How to manage and monitor your deployed application
- Best practices for security, scaling, and cost management in production

## 🎯 Next Steps

Congratulations! Your AI application is now running in production. Ready to explore the cutting edge of AI extensibility?

**Continue to** → [Part 7: MCP Server Basics](../Part%207%20-%20MCP%20Server%20Basics/README.md)

In Part 7, you'll learn about Model Context Protocol (MCP) and how to:

- 🔧 Build custom tools that extend AI agents
- 🌤️ Create your first MCP server with weather tools
- 🤖 Integrate with GitHub Copilot and VS Code
- ⚡ Enable AI agents to interact with external systems

## Conclusion

Congratulations! You've completed the AI Web Chat application series (Parts 1-6). You now have the knowledge to:

1. ✅ Create AI applications using the AI Web Chat template
2. ✅ Understand and customize the template code structure
3. ✅ Migrate from GitHub Models to Azure OpenAI
4. ✅ Implement AI-powered features like the Products page
5. ✅ Deploy your application to production environments using Azure

Continue exploring the possibilities of AI with .NET and build amazing AI-powered applications!
