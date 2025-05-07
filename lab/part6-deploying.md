# Deploying the Application

## In this lab

In this lab, you will learn how to deploy your AI application to production environments. You'll explore different deployment options and best practices for deploying AI applications built with .NET. We'll focus on using the Azure Developer CLI (`azd`) for a streamlined deployment experience.

## Deployment Options

There are several options for deploying your .NET Aspire applications:

### 1. Azure Container Apps (Recommended)

Azure Container Apps is the recommended choice for deploying .NET Aspire applications:

- **Fully managed**: No need to manage underlying infrastructure
- **Built-in scaling**: Automatic scaling based on load
- **Container orchestration**: Built on Kubernetes but simplified
- **.NET Aspire integration**: Native support for .NET Aspire applications
- **Azure Developer CLI integration**: Simplified deployment process

### 2. Azure App Service

Azure App Service is suitable for simpler, non-containerized deployments:

- **Familiar deployment model**: Easy to use with existing knowledge
- **Integrated with Visual Studio**: Simple deployment from IDE
- **Multiple languages**: Supports .NET, Node.js, Python, etc.
- **DevOps integration**: Easy CI/CD pipeline setup

### 3. Azure Kubernetes Service (AKS)

For more complex, scalable deployments:

- **Full Kubernetes**: Complete control over orchestration
- **Advanced scaling**: Complex scaling scenarios
- **Multi-container**: Management of complex microservices
- **Global distribution**: Deploy across regions

## Step 1: Prepare Your Application for Deployment

1. **Configure settings for production**:
   - Update `appsettings.Production.json`:

   ```json
   {
     "ConnectionStrings": {
       "openai": "Endpoint=https://your-resource-name.openai.azure.com;Key=your-key-here"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     }
   }
   ```

2. **Set up application insights**:
   - Add Application Insights to your project:

   ```csharp
   // In Program.cs
   builder.Services.AddApplicationInsightsTelemetry();
   ```

3. **Configure CORS if needed**:

   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("ProductionPolicy", policy =>
       {
           policy.WithOrigins("https://your-app-domain.com")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   ```

## Step 2: Set Up Azure Development Environment

1. **Install the Azure Developer CLI (azd)**:

   If you don't already have the Azure Developer CLI installed, you can install it with:

   ```powershell
   winget install microsoft.azd
   ```

   Or using PowerShell:

   ```powershell
   irm https://aka.ms/install-azd.ps1 | iex
   ```

2. **Login to Azure**:

   ```powershell
   azd auth login
   ```

3. **Create your Azure environment configuration**:

   Create an `azure.yaml` file in the root of your project:

   ```yaml
   name: mygenailab
   services:
     app:
       project: GenAiLab.AppHost/GenAiLab.AppHost.csproj
       host: containerapp
       language: dotnet
   ```

   This configuration tells `azd` that we're deploying a .NET project to Azure Container Apps.

## Step 3: Deploy to Azure Container Apps

1. **Initialize your Azure environment**:

   ```powershell
   # Create a new environment with a specific location
   azd init --environment mygenailab
   ```

   When prompted, select an Azure location like East US.

1. **Provision Azure resources and deploy your application**:

   ```powershell
   azd provision
   ```

   This command creates all the necessary Azure resources, including:
   - Resource group
   - Container registry
   - Container apps environment
   - Container apps for your application
   - Log Analytics workspace

1. **Deploy your application code**:

   ```powershell
   azd deploy
   ```

   This command builds your .NET application, creates container images, pushes them to the Azure Container Registry, and deploys them to Azure Container Apps.

1. **Configure secrets and environment variables**:

   ```powershell
   # Set the Azure OpenAI connection string as a secret
   azd set config openai.connectionString "Endpoint=https://your-resource-name.openai.azure.com;Key=your-key-here"
   
   # Deploy the updated configuration
   azd deploy
   ```

1. **Application Insights for monitoring**:

   .NET Aspire includes built-in support for Application Insights. To configure it, add Application Insights to your `azure.yaml`:

   ```yaml
   resources:
     appinsights:
       type: azure.monitor.applicationinsights
   ```

1. **Re-provision resources and deploy**:

   ```powershell
   azd provision
   azd deploy
   ```

## Step 6: Managing Your Deployment

Once deployed, you can manage your deployment using various tools:

1. **View deployment information**:

   ```powershell
   azd show
   ```

   This command shows your deployment details, including endpoints and resource information.

2. **Monitor your application**:

   ```powershell
   azd monitor
   ```

   This opens the Application Insights dashboard for your application.

3. **Update your deployment**:

   After making changes to your application:

   ```powershell
   azd deploy
   ```

4. **Delete your deployment**:

   To clean up all resources:

   ```powershell
   azd down
   ```

## Step 7: Production Considerations

### Security Best Practices

1. **Secure your API keys**:
   - Use Azure Key Vault for storing API keys and secrets
   - Never hardcode keys in your application code
   - Rotate keys periodically

2. **Implement proper authentication and authorization**:
   - Add authentication to your application
   - Protect API endpoints
   - Consider identity providers like Azure AD

3. **Use HTTPS everywhere**:
   - Enable HTTPS for all endpoints
   - Configure proper CORS policies

### Scaling and Performance

1. **Configure scaling rules in Azure Container Apps**:
   - Set minimum and maximum replicas
   - Configure scaling metrics based on load

2. **Implement caching for AI responses**:
   - Use distributed caching (Redis)
   - Cache common AI-generated content

3. **Optimize network communication**:
   - Use gRPC for internal service communication
   - Configure appropriate timeouts

### Cost Management

1. **Monitor AI service usage**:
   - Track token usage with telemetry
   - Set up cost alerts and budgets

2. **Optimize embedding generation**:
   - Only generate embeddings when necessary
   - Cache embedding results

3. **Configure appropriate instance sizes**:
   - Start with smaller instances and scale up as needed
   - Use autoscaling to optimize costs

## What You've Learned

- How to use the Azure Developer CLI (azd) to deploy your AI application
- How to set up and configure Azure Container Apps resources
- How to automate deployments with GitHub Actions
- Best practices for security, scaling, and monitoring in production
- How to manage costs for AI applications

## Conclusion

Congratulations! You've completed all labs in the Building GenAI Apps with .NET tutorial. You now have the knowledge to:

1. Develop AI applications using Microsoft Extensions for AI
2. Implement semantic search using vector embeddings
3. Use GitHub Models for development and Azure OpenAI for production
4. Deploy your AI application to production environments using Azure Developer CLI

Continue exploring the possibilities of AI with .NET!
