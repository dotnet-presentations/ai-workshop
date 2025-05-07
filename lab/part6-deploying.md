# Deploying the Application

## In this lab

In this lab, you will learn how to deploy your AI application to production environments. You'll explore different deployment options and best practices for deploying AI applications built with .NET.

## Deployment Options

There are several options for deploying your AI application:

### 1. Azure Container Apps

Azure Container Apps is an excellent choice for deploying .NET Aspire applications:

- **Fully managed**: No need to manage underlying infrastructure
- **Built-in scaling**: Automatic scaling based on load
- **Container orchestration**: Built on Kubernetes but simplified
- **.NET Aspire integration**: Native support for .NET Aspire applications

### 2. Azure App Service

Azure App Service is suitable for simpler deployments:

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

## Step 2: Containerize Your Application

1. **Ensure Docker is installed and running on your development machine**

2. **Create a Dockerfile**:

   ```dockerfile
   # First stage - Build
   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   WORKDIR /src
   
   # Copy project files
   COPY ["MyGenAiLab.sln", "./"]
   COPY ["MyGenAiLab.Web/MyGenAiLab.Web.csproj", "MyGenAiLab.Web/"]
   COPY ["MyGenAiLab.ServiceDefaults/MyGenAiLab.ServiceDefaults.csproj", "MyGenAiLab.ServiceDefaults/"]
   
   # Restore packages
   RUN dotnet restore
   
   # Copy all source code
   COPY . .
   
   # Build application
   RUN dotnet build "MyGenAiLab.Web/MyGenAiLab.Web.csproj" -c Release -o /app/build
   
   # Publish application
   RUN dotnet publish "MyGenAiLab.Web/MyGenAiLab.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false
   
   # Final stage - Runtime
   FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
   WORKDIR /app
   COPY --from=build /app/publish .
   
   # Add sample PDF files
   COPY ["MyGenAiLab.Web/wwwroot/Data", "wwwroot/Data"]
   
   # Set environment variables
   ENV ASPNETCORE_ENVIRONMENT=Production
   ENV ASPNETCORE_URLS=http://+:8080
   
   EXPOSE 8080
   ENTRYPOINT ["dotnet", "MyGenAiLab.Web.dll"]
   ```

3. **Build the Docker image**:

   ```powershell
   docker build -t mygenailab:latest .
   ```

4. **Test the container locally**:

   ```powershell
   docker run -p 8080:8080 mygenailab:latest
   ```

   Visit <http://localhost:8080> to verify the application is working.

## Step 3: Deploy to Azure Container Apps

1. **Create an Azure Container Registry**:

   ```powershell
   # Login to Azure
   az login
   
   # Create resource group
   az group create --name rg-mygenailab --location eastus
   
   # Create container registry
   az acr create --resource-group rg-mygenailab --name mygenailabregistry --sku Basic
   ```

2. **Push Docker image to ACR**:

   ```powershell
   # Login to ACR
   az acr login --name mygenailabregistry
   
   # Tag the image for ACR
   docker tag mygenailab:latest mygenailabregistry.azurecr.io/mygenailab:latest
   
   # Push the image
   docker push mygenailabregistry.azurecr.io/mygenailab:latest
   ```

3. **Create Azure Container App**:

   ```powershell
   # Create Container Apps Environment
   az containerapp env create --name mygenailab-env --resource-group rg-mygenailab --location eastus
   
   # Create Container App
   az containerapp create `
       --name mygenailab `
       --resource-group rg-mygenailab `
       --environment mygenailab-env `
       --image mygenailabregistry.azurecr.io/mygenailab:latest `
       --target-port 8080 `
       --ingress external `
       --registry-server mygenailabregistry.azurecr.io `
       --min-replicas 1 `
       --max-replicas 5
   ```

4. **Configure secrets and environment variables**:

   ```powershell
   # Set the Azure OpenAI connection string
   az containerapp secret set `
       --name openai-connection `
       --resource-group rg-mygenailab `
       --app mygenailab `
       --value "Endpoint=https://your-resource-name.openai.azure.com;Key=your-key-here"
   
   # Map the secret to an environment variable
   az containerapp update `
       --name mygenailab `
       --resource-group rg-mygenailab `
       --set-env-vars "ConnectionStrings__openai=secretref:openai-connection"
   ```

5. **Set up deployment for Qdrant vector database**:

   ```powershell
   # Create Container App for Qdrant
   az containerapp create `
       --name mygenailab-qdrant `
       --resource-group rg-mygenailab `
       --environment mygenailab-env `
       --image qdrant/qdrant:latest `
       --target-port 6333 `
       --ingress internal `
       --min-replicas 1 `
       --max-replicas 1
   
   # Configure storage for Qdrant
   az containerapp update `
       --name mygenailab-qdrant `
       --resource-group rg-mygenailab `
       --set-env-vars "QDRANT_STORAGE_PATH=/qdrant/storage"
   
   # Update main app with Qdrant connection
   az containerapp update `
       --name mygenailab `
       --resource-group rg-mygenailab `
       --set-env-vars "VectorStore__Url=https://mygenailab-qdrant.internal.mygenailab-env.eastus.azurecontainerapps.io:6333"
   ```

## Step 4: Setting Up CI/CD with GitHub Actions

1. **Create a GitHub workflow file** in `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure Container Apps

on:
  push:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
        
    - name: Build and publish
      run: |
        dotnet restore
        dotnet build --configuration Release
        dotnet publish MyGenAiLab.Web/MyGenAiLab.Web.csproj -c Release -o publish
      
    - name: Log in to Azure
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
        
    - name: Log in to ACR
      uses: azure/docker-login@v1
      with:
        login-server: ${{ secrets.ACR_LOGIN_SERVER }}
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
        
    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        file: ./Dockerfile
        push: true
        tags: ${{ secrets.ACR_LOGIN_SERVER }}/mygenailab:${{ github.sha }}
        
    - name: Deploy to Azure Container Apps
      uses: azure/container-apps-deploy-action@v1
      with:
        resourceGroup: rg-mygenailab
        containerAppName: mygenailab
        imageToDeploy: ${{ secrets.ACR_LOGIN_SERVER }}/mygenailab:${{ github.sha }}
```

1. **Add GitHub secrets** for the workflow:
   - `AZURE_CREDENTIALS`: JSON credentials for Azure
   - `ACR_LOGIN_SERVER`: ACR login server URL
   - `ACR_USERNAME`: ACR username
   - `ACR_PASSWORD`: ACR password

## Step 5: Monitoring and Logging

1. **Set up Application Insights**:

```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add custom telemetry
builder.Services.AddSingleton<ITelemetryInitializer, AITelemetryInitializer>();
```

1. **Create a custom telemetry initializer**:

```csharp
public class AITelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "MyGenAiLab.Web";
        
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            // Track AI model usage
            if (requestTelemetry.Url.AbsolutePath.Contains("/chat"))
            {
                requestTelemetry.Properties["AIFeature"] = "ChatCompletion";
            }
            else if (requestTelemetry.Url.AbsolutePath.Contains("/products"))
            {
                requestTelemetry.Properties["AIFeature"] = "ProductDescription";
            }
        }
    }
}
```

## Step 6: Production Considerations

### Security Best Practices

1. **Secure your API keys**:
   - Use Key Vault for storing API keys and secrets
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

1. **Configure scaling rules**:
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

- How to containerize your AI application for deployment
- How to deploy to Azure Container Apps
- How to set up CI/CD for automated deployments
- Best practices for security, scaling, and monitoring in production
- How to manage costs for AI applications

## Conclusion

Congratulations! You've completed all labs in the Building GenAI Apps in C# tutorial. You now have the knowledge to:

1. Develop AI applications using Microsoft Extensions for AI
2. Implement semantic search using vector embeddings
3. Use GitHub Models for development and Azure OpenAI for production
4. Deploy your AI application to production environments

Continue exploring the possibilities of AI with .NET!
