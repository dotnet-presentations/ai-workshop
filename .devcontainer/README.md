# GitHub Codespaces / Dev Container Configuration

This folder contains the development container configuration for running the .NET AI Workshop in GitHub Codespaces or VS Code Dev Containers.

## 🚀 Quick Start with GitHub Codespaces

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/dotnet-presentations/ai-workshop?quickstart=1)

Click the badge above or follow these steps:

1. Navigate to the repository on GitHub
2. Click the **Code** button
3. Select the **Codespaces** tab
4. Click **Create codespace on main**

Your Codespace will be automatically configured with:

- ✅ .NET 9.0 SDK
- ✅ Docker-in-Docker (for Qdrant vector database)
- ✅ Azure CLI and Azure Developer CLI
- ✅ GitHub CLI
- ✅ Microsoft.Extensions.AI.Templates pre-installed
- ✅ VS Code extensions for C# and GitHub Copilot

## 📋 What's Included

### Development Tools

- **.NET 9.0 SDK**: Latest .NET runtime for AI Web Chat applications
- **Docker-in-Docker**: Required for running Qdrant vector database in Aspire
- **Azure CLI**: For deploying applications to Azure
- **Azure Developer CLI (azd)**: For streamlined Azure deployments
- **GitHub CLI**: For working with GitHub Models and repositories

### VS Code Extensions

- **GitHub Copilot**: AI-powered code completion
- **C# Dev Kit**: Full C# development experience
- **Azure GitHub Copilot**: Azure-specific AI assistance

### Starting Solution

The Codespace automatically:

- Restores the **Part 2 - Project Creation** starting solution
- Sets it as the default solution in VS Code
- Configures port forwarding for the Aspire dashboard

## 🔧 Configuration Details

### Port Forwarding

The following ports are automatically forwarded:

- **18888**: Aspire Dashboard (primary port)
- **15000-15004**: Additional Aspire services ports
- Dynamic ports are forwarded as needed for your applications

### Post-Creation Setup

After the Codespace is created, the following commands run automatically:

```bash
dotnet workload update
dotnet new install Microsoft.Extensions.AI.Templates
dotnet dev-certs https --trust
dotnet restore "Part 2 - Project Creation/GenAiLab"
```

## 📚 Next Steps

Once your Codespace is ready:

1. **Configure GitHub Token**: Follow [Part 1 - Setup](../Part%201%20-%20Setup/README.md) to create a GitHub token for GitHub Models
2. **Set User Secrets**: Right-click on `GenAiLab.AppHost` project and select "Manage User Secrets"
3. **Run the Application**: Press F5 or run `dotnet run --project "Part 2 - Project Creation/GenAiLab/GenAiLab.AppHost"`

### 🔐 Managing Secrets in Codespaces

#### GitHub Token for GitHub Models

1. Create a GitHub token with **Models: Read-only** permission at <https://github.com/settings/personal-access-tokens/new>
2. In your Codespace, open the terminal
3. Navigate to the Part 2 solution:

   ```bash
   cd "Part 2 - Project Creation/GenAiLab"
   ```

4. Set the user secret:

   ```bash
   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-GITHUB-TOKEN" --project GenAiLab.AppHost
   ```

#### Using Codespace Secrets (Recommended for Multiple Sessions)

To avoid re-entering your token each time:

1. Go to your GitHub settings → Codespaces → Secrets
2. Add a new secret named `GITHUB_MODEL_TOKEN`
3. Update your `.devcontainer/devcontainer.json` to include:

   ```json
   "containerEnv": {
     "GITHUB_MODEL_TOKEN": "${localEnv:GITHUB_MODEL_TOKEN}"
   }
   ```

4. Access it in your application configuration

#### Azure OpenAI Configuration

If using Azure OpenAI (Part 4+), store your endpoint and key:

```bash
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://YOUR-RESOURCE.openai.azure.com;Key=YOUR-KEY" --project GenAiLab.AppHost
```

## 🌐 Alternative Configurations

For developers who want to use local AI models or work offline, the original development container configurations are still available in `.github/.devcontainer/`:

- **Standard Container**: `.github/.devcontainer/devcontainer.json`
- **Ollama Container**: `.github/.devcontainer/Ollama/devcontainer.json`

See [.github/.devcontainer/README.md](../.github/.devcontainer/README.md) for details.

## 🐛 Troubleshooting

### Docker Issues

If you encounter Docker-related errors when running Aspire applications:

1. Ensure Docker is running: `docker ps`
2. Try restarting the Codespace
3. Check Docker service: `sudo systemctl status docker`

### Port Forwarding Issues

If the Aspire dashboard doesn't open automatically:

1. Check the **Ports** tab in VS Code
2. Manually forward port 18888 or the dynamic port shown in the console
3. Click the globe icon to open the forwarded port in a browser

### Memory Issues

The Codespace requires at least 8GB of RAM. If you experience memory issues:

1. Close unnecessary browser tabs
2. Stop unused Docker containers: `docker stop $(docker ps -q)`
3. Increase Codespace machine size (requires paid plan)

## 💡 Tips

- **Persist your GitHub token**: Store it in Codespace secrets for automatic configuration
- **Save your work frequently**: Codespaces auto-save, but commits ensure persistence
- **Use branches**: Create feature branches for your workshop progress
- **Port visibility**: Set port visibility to "Public" if you need to share your Aspire dashboard

## 📖 Learn More

- [VS Code Dev Containers](https://code.visualstudio.com/docs/devcontainers/containers)
- [GitHub Codespaces](https://docs.github.com/en/codespaces)
- [.NET Aspire Dev Containers](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/dev-containers)
