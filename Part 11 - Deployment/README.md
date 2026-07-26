# Part 11: Deploy to Azure

> **⏱️ Estimated Time:** 30-45 minutes (excluding Azure provisioning time ~6 minutes)

## In this workshop

In this final part, you will learn how to deploy the AI Web Chat application you scaffolded in [Part 4](../Part%204%20-%20AI%20Web%20Chat%20Template/README.md) to Azure using the Azure Developer CLI (`azd`). You'll deploy your Qdrant-backed application to Azure Container Apps for production use.

> [!NOTE]
> This part deliberately returns to the **Part 4 web application** rather than the console samples from Parts 6-10. Deployment is a property of a hosted application, and the Aspire-orchestrated web app is the realistic thing to ship — it has a front end, a vector store, and service dependencies that have to exist in Azure. The MCP servers, the agent sample, and the capstone `StoreApp` are console applications you run locally; what you learn here about `azd` and Container Apps applies to hosting any of them later.
> [!TIP]
> If you haven't completed the previous steps in the lab or are having trouble with your code, you can use the working code snapshot provided in this `Part 11 - Deployment` folder. The complete code has already been updated with the necessary configuration for external HTTP endpoints and deployment. You can skip directly to the "Set Up the Azure Developer CLI" section and deploy that code instead.

## Configure the web application for external access

  Before the web application is deployed to Azure Container Apps, you will need to configure it so that it is available via web browser. Update `GenAiLab.AppHost/AppHost.cs` to add the following line just before the call to `builder.Build().Run();` at the end of the file:

  ```csharp
  webApp.WithExternalHttpEndpoints();
  ```

> [!IMPORTANT]
> This is an Aspire solution. Always launch the `GenAiLab.AppHost` project when running locally because AppHost bootstraps the full distributed app (web app + supporting services).

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
> For a note on using a managed vector store in production, see [Optional: using a managed vector store](#optional-using-a-managed-vector-store) at the end of this part.

1. Ensure you are in the root directory which contains the solution file.

1. **Initialize your Azure environment**:

   ```powershell
   # Initialize the application for managment with azd
   azd init
   ```

1. When prompted with "How do you want to initialize your app?", select the default: "Use code in the current directory"

1. After scanning the directory, `azd` prompts you to confirm that it found the correct Aspire *AppHost* project. Select the **Confirm and continue initializing my app** option.

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

## Optional: using a managed vector store

This workshop deploys Qdrant because it needs no setup, behaves the same locally
and in Azure, and costs nothing beyond the container it runs in. A production app
often wants a managed vector store instead, and on Azure that usually means
[Azure AI Search](https://learn.microsoft.com/azure/search/vector-search-overview).

That swap is smaller than you might expect, for the same reason provider swaps
were small in [Part 5](../Part%205%20-%20Providers%20and%20Fallbacks/README.md).
Your search code depends on an abstraction rather than on Qdrant:

```csharp
// GenAiLab.Web/Services/SemanticSearch.cs
public class SemanticSearch(VectorStoreCollection<Guid, IngestedChunk> vectorCollection)
```

`VectorStoreCollection<TKey, TRecord>` comes from `Microsoft.Extensions.VectorData`,
so `SemanticSearch`, `DataIngestor`, and the Blazor components stay as they are.
Only registration changes, in two files: the `AddQdrantClient` and
`AddQdrantCollection<...>` calls in `GenAiLab.Web/Program.cs`, and the
`builder.AddQdrant("vectordb")` resource in `GenAiLab.AppHost/AppHost.cs` that tells
`azd` what to provision.

Rather than editing by hand, scaffold the template a second time with the Azure AI
Search vector store option (the same `dotnet new aichatweb` command from
[Part 4](../Part%204%20-%20AI%20Web%20Chat%20Template/README.md), with a different
`--vector-store` value) and diff the two projects. That shows you the exact
registration and Aspire wiring the template generates.

The trade-off is the usual one. Qdrant is cheaper and portable, and you own the
container and its data volume. Azure AI Search is billed per service hour even
when idle, but it is managed, has an SLA, and adds keyword and hybrid search
alongside vector search.

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

Your AI application is now running in production, and you have reached the end of the workshop.

Use the end-of-workshop resource handoff page:

- 📚 [End-of-Workshop Resources](../resources.md)

If you skipped the optional MCP track, this is a good moment to come back to it:

- 🏢 [Part 7: Enhanced MCP Server](../Part%207%20-%20Enhanced%20MCP%20Server/README.md) *(Optional / bonus)*
- 📦 [Part 8: MCP Publishing](../Part%208%20-%20MCP%20Publishing/README.md) *(Optional / bonus)*

## Conclusion

You have completed the workshop (Parts 1-11). You can now:

1. ✅ Create AI applications using the AI Web Chat template
2. ✅ Understand and customize the template code structure
3. ✅ Configure Microsoft Foundry (Azure OpenAI) for cloud AI workloads
4. ✅ Configure provider and fallback strategies for real-world deployments
5. ✅ Deploy your application to production environments using Azure

Use this app as a baseline for your own AI application experiments.

---

📖 **Return to**: [Workshop Overview](../README.md) | 🔄 **Previous**: [Part 10: Adding AI to an Existing App](../Part%2010%20-%20Adding%20AI%20to%20an%20Existing%20App/README.md)
