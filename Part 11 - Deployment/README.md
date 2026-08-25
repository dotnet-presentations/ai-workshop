# Part 11: Deploy to Azure

> **⏱️ Estimated Time:** 30-45 minutes (excluding Azure provisioning time, typically 7-12 minutes)

## In this workshop

In this final part, you will deploy the AI Web Chat application you brought back
in [Part 10](../Part%2010%20-%20Choosing%20Providers%20and%20Services/README.md) using the
Azure Developer CLI (`azd`). The recommended path runs the web application in
Azure Container Apps and uses managed Azure AI Search for its vector index.

> [!NOTE]
> This part deliberately returns to the **Part 4 web application** rather than the samples from Parts 5-9. Deployment is a property of a hosted application, and the Aspire-orchestrated web app is the realistic thing to ship — it has a front end, a vector store, and service dependencies that have to exist in Azure. The MCP servers, the agent sample, and the Part 9 capstone are things you run locally; what you learn here about `azd` and Container Apps applies to hosting any of them later.
>
> Docker Desktop or Podman is required only if you run the complete application
> locally, because the markitdown document reader is a container. Azure Container
> Apps runs it after deployment.
>
> [!TIP]
> If you haven't completed the previous steps in the lab or are having trouble with your code, you can use the working code snapshot provided in this `Part 11 - Deployment` folder. The complete code has already been updated with the necessary configuration for external HTTP endpoints and deployment. You can skip directly to the "Set Up the Azure Developer CLI" section and deploy that code instead.

## `GenAiLab/` is the deployment-ready snapshot

[`GenAiLab/`](GenAiLab/) starts from the completed
[Part 4](../Part%2004%20-%20AI%20Web%20Chat%20Template/README.md) application and
includes the production preparation from Part 10:

- current package versions
- `AddConnectionString("openai")` for your existing Azure OpenAI resource
- `gpt-5-mini` as the chat deployment
- Azure AI Search instead of the local Qdrant container
- `WithExternalHttpEndpoints()` for the deployed web application

Use this snapshot if you did not keep your morning project or do not want to make
the Part 10 changes by hand.

Either way, you supply your own credentials. Set `ConnectionStrings:openai` in user
secrets on **`GenAiLab.AppHost`** — in Visual Studio, right-click the project and
choose **Manage User Secrets**:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://YOUR-RESOURCE.openai.azure.com/;Key=YOUR-KEY"
  }
}
```

Or, from the workshop root, run
`.\.github\scripts\setup-workshop-credentials.ps1 -ApplyUserSecrets`.

## Confirm the web application has external access

The Part 11 snapshot already includes `WithExternalHttpEndpoints()`. If you
carried your own project forward from Part 10, confirm that
`GenAiLab.AppHost/AppHost.cs` adds it to `webApp`:

```csharp
var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app")
    .WithExternalHttpEndpoints();
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
> **Vector Database Configuration**: The recommended deployment uses **Azure AI
> Search**. Aspire provisions the Search service, configures access for the
> application, and passes its connection information to the web app. The
> ingestion pipeline creates the vector index, so you do not create one manually.
> Your Azure account must be able to create Azure AI Search resources and role
> assignments.
>
> If you kept Qdrant as the Part 10 fallback, `azd` deploys it as another
> Container App instead.

1. Ensure you are in the root directory which contains the solution file.

1. **Initialize your Azure environment**:

   ```powershell
   # Initialize the application for management with azd
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
   - Azure AI Search service
   - Container apps for your application and the markitdown document reader
   - Log Analytics workspace

> [!NOTE]
> When provisioning resources with `azd`, it will automatically create a resource group with the prefix "rg-" added to your environment name (e.g., "rg-mygenaiapp").
  
1. When prompted to select a location, select a region close to you (e.g., "West US 3" or another nearby Azure datacenter).

1. When prompted to "Enter a value for the 'openai' infrastructure secured parameter", enter your Azure OpenAI connection string in the format:

   ```text
   Endpoint=https://your-resource.openai.azure.com/;Key=your-api-key
   ```

   PowerShell does not expand environment variables pasted as text into an
   interactive prompt. To use the workshop variables, copy the expanded value
   before running `azd provision`, then paste it when prompted:

   ```powershell
   "Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY" |
       Set-Clipboard
   ```

1. Press enter and watch as your resources are provisioned! You can either just follow along in the terminal, or you can click on the link to watch the progress in the Azure portal. Provisioning should take roughly 5 minutes, but may take longer during conference events as multiple concurrent deployments can slow things down.

1. **Deploy your application code**:

   ```powershell
   azd deploy
   ```

   This command:
   - Builds your .NET application
   - Creates container images for the web app and markitdown
   - Pushes them to the Azure Container Registry
   - Deploys them to Azure Container Apps
  
   This should take roughly 2 minutes, but may take longer under busy conditions.

1. **Access your deployed application**:

   After deployment completes, you'll receive a URL to access your application in the terminal output. You can also view it using:

   ```powershell
   azd show
   ```

## Qdrant fallback

If Azure AI Search provisioning is unavailable in your subscription, use the
Qdrant project you completed in Part 4. If you no longer have it, scaffold it
again with the [Part 4 command](../Part%2004%20-%20AI%20Web%20Chat%20Template/README.md#step-1-install-the-template-and-scaffold),
complete Part 4 Steps 2.1-2.4, and confirm the external endpoint as shown above.
Its `AddQdrant("vectordb")` resource tells `azd` to deploy Qdrant as a Container
App alongside the web application. The remaining deployment commands are the
same.

## Manage Your Deployment

Once deployed, you can manage your deployment using various Azure Developer CLI commands:

1. **View deployment information**:

   ```powershell
   azd show
   ```

   This command shows your deployment details, including endpoints and resource information. Launch the link for the **aichatweb-app** service and verify that it is continuing to run as it did locally.

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

## What's next

Your AI application is now running in production, and you have reached the end of the workshop.

Use the end-of-workshop resource handoff page:

- 📚 [End-of-Workshop Resources](../resources.md)

If you skipped the optional MCP track, this is a good moment to come back to it:

- 🏢 [Part 6: Enhanced MCP Server](../Part%2006%20-%20Enhanced%20MCP%20Server/README.md) *(Optional / bonus)*
- 📦 [Part 7: MCP Publishing](../Part%2007%20-%20MCP%20Publishing/README.md) *(Optional / bonus)*

## Conclusion

You have completed the workshop (Parts 1-11). You can now:

1. ✅ Create AI applications using the AI Web Chat template
2. ✅ Understand and customize the template code structure
3. ✅ Configure Microsoft Foundry (Azure OpenAI) for cloud AI workloads
4. ✅ Choose between cloud and local providers, and between vector store services, for real-world deployments
5. ✅ Deploy your application to production environments using Azure

Use this app as a baseline for your own AI application experiments.

---

📖 **Return to**: [Workshop Overview](../README.md) | 🔄 **Previous**: [Part 10: Choosing Providers and Services](../Part%2010%20-%20Choosing%20Providers%20and%20Services/README.md)
