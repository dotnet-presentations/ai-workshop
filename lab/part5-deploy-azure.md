# Deploy to Azure

## In this lab

In this lab, you will learn how to deploy your AI application to Azure using the Azure Developer CLI (`azd`). You'll prepare your application for production, migrate from SQLite to PostgreSQL for better scalability, and deploy your application to Azure Container Apps.

## Prepare for Production: Migrate from SQLite to PostgreSQL

The first step in preparing for production is to upgrade our data storage from SQLite (which is great for development) to PostgreSQL (which is better suited for production workloads).

1. **Add PostgreSQL NuGet packages**:

   **Using Visual Studio's .NET Aspire tooling**:

   For the `GenAiLab.AppHost` project:
   * Right-click on the `GenAiLab.AppHost` project in Solution Explorer
   * Select "Add" > ".NET Aspire package..."
   * In the package manager that opens (with pre-filtered .NET Aspire packages), search for "Aspire.Hosting.PostgreSQL"
   * Select the package and click "Install"

   For the `GenAiLab.Web` project:
   * Right-click on the `GenAiLab.Web` project in Solution Explorer
   * Select "Manage NuGet Packages..."
   * Click on the "Browse" tab
   * Search for "Npgsql.EntityFrameworkCore.PostgreSQL"
   * Select the package and click "Install"

   **Using Terminal**:

   To open the terminal in Visual Studio:
   * Go to "View" menu
   * Select "Terminal" (or press Ctrl+`)

   Then run these commands:

   ```powershell
   dotnet add GenAiLab.AppHost/GenAiLab.AppHost.csproj package Aspire.Hosting.PostgreSQL
   dotnet add GenAiLab.Web/GenAiLab.Web.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

1. **Update AppHost Program.cs**:

   Change the SQLite database references to PostgreSQL in `GenAiLab.AppHost/Program.cs`:

   ```csharp
   // Replace these lines:
   var ingestionCache = builder.AddSqlite("ingestionCache");
   var productDb = builder.AddSqlite("productDb");

   // With:
   var ingestionCache = builder.AddPostgres("ingestionCache")
       .WithDataVolume()
       .WithLifetime(ContainerLifetime.Persistent);
   var productDb = builder.AddPostgres("productDb")
       .WithDataVolume()
       .WithLifetime(ContainerLifetime.Persistent);
   ```

1. **Update Web Project Database Context**:

   In the `GenAiLab.Web/Program.cs` file, update the database context registration:

   ```csharp
   // Replace these lines:
   builder.AddSqliteDbContext<IngestionCacheDbContext>("ingestionCache");
   builder.AddSqliteDbContext<ProductDbContext>("productDb");

   // With:
   builder.AddNpgsqlDbContext<IngestionCacheDbContext>("ingestionCache");
   builder.AddNpgsqlDbContext<ProductDbContext>("productDb");
   ```

1. **Test locally before deployment**:

   Run the application locally to ensure the PostgreSQL migration works:

   ```powershell
   dotnet run --project GenAiLab.AppHost/GenAiLab.AppHost.csproj
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

1. **Login to Azure**:

   ```powershell
   azd auth login
   ```

## Deploy to Azure Container Apps

1. **Initialize your Azure environment**:

   ```powershell
   # Create a new environment with a specific location
   azd init --environment mygenailab
   ```

   When prompted, select an Azure location like East US.

1. **Provision Azure resources**:

   ```powershell
   azd provision
   ```

   This command creates all the necessary Azure resources, including:
   * Resource group
   * Container registry
   * Container apps environment
   * Container apps for your application
   * Log Analytics workspace

1. **Deploy your application code**:

   ```powershell
   azd deploy
   ```

   This command:
   * Builds your .NET application
   * Creates container images
   * Pushes them to the Azure Container Registry
   * Deploys them to Azure Container Apps

1. **Access your deployed application**:

   After deployment completes, you'll receive a URL to access your application in the terminal output. You can also view it using:

   ```powershell
   azd show
   ```

## Manage Your Deployment

Once deployed, you can manage your deployment using various Azure Developer CLI commands:

1. **View deployment information**:

   ```powershell
   azd show
   ```

   This command shows your deployment details, including endpoints and resource information.

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
   * Use Azure Key Vault for storing API keys and secrets
   * Never hardcode keys in your application code
   * Rotate keys periodically

1. **Implement proper authentication and authorization**:
   * Add authentication to your application
   * Protect API endpoints
   * Consider identity providers like Azure AD

1. **Use HTTPS everywhere**:
   * Enable HTTPS for all endpoints
   * Configure proper CORS policies

### Scaling and Performance

1. **Configure scaling rules in Azure Container Apps**:
   * Set minimum and maximum replicas
   * Configure scaling metrics based on load

1. **Implement caching for AI responses**:
   * Use distributed caching (Redis)
   * Cache common AI-generated content

1. **Optimize network communication**:
   * Use gRPC for internal service communication
   * Configure appropriate timeouts

### Cost Management

1. **Monitor AI service usage**:
   * Track token usage with telemetry
   * Set up cost alerts and budgets

1. **Optimize embedding generation**:
   * Only generate embeddings when necessary
   * Cache embedding results

1. **Configure appropriate instance sizes**:
   * Start with smaller instances and scale up as needed
   * Use autoscaling to optimize costs

## What You've Learned

* How to migrate from SQLite to PostgreSQL for production readiness
* How to use the Azure Developer CLI (azd) to deploy your AI application
* How to set up and configure Azure Container Apps
* How to manage and monitor your deployed application
* Best practices for security, scaling, and cost management in production

## Conclusion

Congratulations! You've completed all parts of the AI Web Chat template lab. You now have the knowledge to:

1. Create AI applications using the AI Web Chat template
1. Understand and customize the template code structure
1. Migrate from GitHub Models to Azure OpenAI
1. Implement AI-powered features like the Products page
1. Deploy your application to production environments using Azure

Continue exploring the possibilities of AI with .NET and build amazing AI-powered applications!
