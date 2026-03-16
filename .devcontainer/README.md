# Running the .NET AI Workshop in GitHub Codespaces

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/dotnet-presentations/ai-workshop?quickstart=1)

This workshop provides full support for GitHub Codespaces, allowing you to run the entire lab without installing Docker or any other prerequisites on your local machine. This is particularly useful for users who:

- Can't install Docker on their machine
- Have corporate firewall or security restrictions
- Want to get started quickly without local setup
- Prefer a cloud-based development environment

## 🚀 Quick Start

Click the badge above or follow these steps:

1. **Create a Codespace:**
   - Navigate to https://github.com/dotnet-presentations/ai-workshop
   - Click the **Code** button (green button)
   - Select the **Codespaces** tab
   - Click **Create codespace on main**

2. **Choose Configuration:**
   - VS Code will prompt you to select a dev container configuration
   - Choose **".NET AI Workshop"** for the standard configuration
   - Or choose **".NET AI Workshop - Ollama"** if you want to use local AI models

3. **Wait for Setup:**
   - The container will automatically install all dependencies
   - This includes .NET 9.0, Docker-in-Docker, Azure CLI, and the AI templates
   - Setup typically takes 3-5 minutes

4. **Start Building:**
   - Once ready, the solution will open automatically in `src/start/GenAiLab/`
   - Follow the workshop instructions starting from [Part 2](../Part%202%20-%20Project%20Creation/README.md)

## 📋 What's Included

Both dev container configurations include:

### Standard Configuration (`.devcontainer/devcontainer.json`)

**Best for:** Most users following the standard workshop path

**Includes:**
- ✅ .NET 9.0 SDK with Aspire workload
- ✅ Docker-in-Docker (for Qdrant vector database)
- ✅ Azure CLI and Azure Developer CLI
- ✅ GitHub CLI
- ✅ AI Web Chat template pre-installed
- ✅ VS Code extensions: GitHub Copilot, C# Dev Kit, Azure GitHub Copilot

**Use this if you:**
- Have a GitHub account for GitHub Models
- Plan to use Azure OpenAI
- Want to follow the workshop as designed

### Ollama Configuration (`.devcontainer/Ollama/devcontainer.json`)

**Best for:** Developers who want to work offline or use local models

**Includes everything from Standard Configuration, plus:**
- ✅ Ollama for running local LLMs
- ✅ Pre-pulled models: `llama3.2`, `phi4-mini`, `all-minilm`

**Use this if you:**
- Don't have a GitHub account
- Hit GitHub Models token limits
- Want to work completely offline
- Prefer using local models for privacy/security
- Want to experiment with different models

## 🏗️ Starting Solution

The Codespace includes a pre-configured starting solution in `src/start/GenAiLab/`. This is a copy of the Part 2 GenAiLab project that you can use to follow along with the workshop.

The solution includes:
- **GenAiLab.Web**: Blazor web application with AI chat interface
- **GenAiLab.AppHost**: .NET Aspire orchestration for distributed applications
- **GenAiLab.ServiceDefaults**: Shared service defaults for Aspire projects

## 🔧 Working with the Codespace

### Running the Application

To run the AI Web Chat application in Codespaces:

```bash
cd src/start/GenAiLab
dotnet run --project GenAiLab.AppHost
```

The .NET Aspire dashboard will be automatically forwarded and accessible via the **Ports** tab in VS Code.

### Setting Up GitHub Models

1. Create a GitHub personal access token with **Models: Read-only** permission
2. Store it as a Codespace secret named `WORKSHOP_GITHUB_TOKEN`
3. Or set it as an environment variable in your terminal:
   ```bash
   export WORKSHOP_GITHUB_TOKEN="your-token-here"
   ```

### Port Forwarding

Codespaces automatically forwards these ports:
- **17057**: .NET Aspire dashboard (Standard config)
- **17057**: .NET Aspire dashboard (Ollama config)
- **11434**: Ollama API (Ollama config only)

Access them via the **Ports** tab in VS Code.

## 💡 Tips for Codespaces

1. **Save Your Work**: Codespaces automatically saves your files, but make sure to commit and push your changes regularly.

2. **Stop When Done**: Stop your Codespace when not in use to conserve free hours (Settings → Stop Codespace).

3. **Rebuild Container**: If something goes wrong, you can rebuild the container:
   - Press `F1` or `Ctrl+Shift+P`
   - Select "Codespaces: Rebuild Container"

4. **Switch Configurations**: To switch between Standard and Ollama configurations:
   - Press `F1` or `Ctrl+Shift+P`
   - Select "Codespaces: Add Dev Container Configuration Files"
   - Choose the other configuration

5. **Performance**: Codespaces provides 4 vCPUs and 16GB RAM, which is sufficient for this workshop. For better performance, you can upgrade to a larger machine type in your Codespace settings.

## 🆚 Codespaces vs Local Development

| Feature | Codespaces | Local Development |
|---------|-----------|-------------------|
| Setup Time | 3-5 minutes | 20-30 minutes |
| Prerequisites | None (all in cloud) | Docker, .NET 9.0, Visual Studio |
| Access | Any device with browser | Specific machine only |
| Cost | Free tier available | Free (local resources) |
| Performance | Cloud-based | Local machine dependent |

## 🔗 Related Documentation

- [.NET Aspire Dev Containers](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/dev-containers)
- [GitHub Codespaces Documentation](https://docs.github.com/en/codespaces)
- [Workshop Part 1: Setup](../Part%201%20-%20Setup/README.md)
- [Workshop Part 2: Project Creation](../Part%202%20-%20Project%20Creation/README.md)

## 🐛 Troubleshooting

### Container Build Fails

If the container fails to build:
1. Check the build logs in the VS Code output panel
2. Try rebuilding: `F1` → "Codespaces: Rebuild Container"
3. Ensure your GitHub account has access to the repository

### Docker Issues

Docker-in-Docker is pre-configured. If you encounter Docker issues:
```bash
sudo service docker status
sudo service docker start
```

### Ollama Models Not Loading

If Ollama models don't load in the Ollama configuration:
```bash
ollama list  # Check available models
ollama pull llama3.2  # Manually pull a model
```

### Performance Issues

If the Codespace is slow:
1. Check your machine type: Settings → Change machine type
2. Stop and restart the Codespace
3. Close unused browser tabs and applications

## 📞 Getting Help

If you encounter issues:
1. Check the [GitHub Codespaces documentation](https://docs.github.com/en/codespaces)
2. Review the workshop [README](../README.md)
3. Open an issue in the [repository](https://github.com/dotnet-presentations/ai-workshop/issues)
