# Welcome to the .NET AI Workshop in GitHub Codespaces! 🚀

You're running this workshop in GitHub Codespaces - no local installation required!

## ✅ What's Already Set Up

Your Codespace includes:
- ✅ .NET 9.0 SDK with Aspire workload
- ✅ Docker-in-Docker (for Qdrant vector database)
- ✅ Azure CLI and Azure Developer CLI
- ✅ GitHub CLI
- ✅ AI Web Chat templates pre-installed
- ✅ VS Code extensions (GitHub Copilot, C# Dev Kit)
- ✅ Starting solution in `src/start/GenAiLab/`

## 🎯 Getting Started

### Step 1: Open the Starting Solution

The starting solution is pre-loaded at `src/start/GenAiLab/`. To open it:

```bash
code src/start/GenAiLab/GenAiLab.sln
```

Or navigate to it in the VS Code file explorer.

### Step 2: Configure AI Provider

You'll need to set up your AI provider (GitHub Models or Azure OpenAI):

#### Option A: GitHub Models (Free)

1. Create a GitHub personal access token:
   - Go to https://github.com/settings/tokens
   - Create a fine-grained token with **Models: Read-only** permission
   
2. Set it as an environment variable:
   ```bash
   export WORKSHOP_GITHUB_TOKEN="your-token-here"
   ```

3. Or set it as a Codespace secret for persistence:
   - Go to your Codespace settings
   - Add a secret named `WORKSHOP_GITHUB_TOKEN`

#### Option B: Azure OpenAI

If you have an Azure OpenAI subscription:

```bash
export WORKSHOP_AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export WORKSHOP_AZURE_OPENAI_KEY="your-key-here"
```

### Step 3: Run the Application

```bash
cd src/start/GenAiLab
dotnet run --project GenAiLab.AppHost
```

The .NET Aspire dashboard will open automatically. You can access it via the **Ports** tab (port 17057).

## 📚 Follow the Workshop

Now continue with the workshop instructions:

1. **Skip** Part 1 (Setup) - already done!
2. **Skip** Part 2's project creation steps - the solution is ready!
3. **Start** with Part 2's authentication configuration section
4. **Continue** with Part 3 onward

👉 [Go to Part 2 - Project Creation](Part%202%20-%20Project%20Creation/README.md#configure-github-models-authentication)

## 💡 Codespaces Tips

### Managing Your Codespace

- **Stop when done**: Settings → Stop Codespace (saves free hours)
- **Rebuild if needed**: `F1` → "Codespaces: Rebuild Container"
- **View ports**: Click the "Ports" tab to see forwarded services

### Port Forwarding

Your Codespace automatically forwards:
- **Port 17057**: .NET Aspire Dashboard
- **Port 11434**: Ollama API (if using Ollama config)

### Committing Changes

Your Codespace has full Git access:

```bash
git add .
git commit -m "Your changes"
git push
```

### Performance

- This Codespace has 4 vCPUs and 16GB RAM
- If you need more power, upgrade in Settings → Change machine type

## 🔄 Switching Configurations

This workshop offers two dev container configurations:

### Standard Configuration (Current)
- Uses GitHub Models or Azure OpenAI
- Best for most users

### Ollama Configuration  
- Includes local AI models (llama3.2, phi4-mini)
- Works completely offline
- Great for privacy or when hitting API limits

To switch:
1. `F1` → "Codespaces: Rebuild Container"
2. Select the "Ollama" configuration

## 🐛 Troubleshooting

### "Port already in use"
```bash
lsof -ti:17057 | xargs kill -9
```

### "Docker not available"
```bash
sudo service docker status
sudo service docker start
```

### "Can't restore packages"
```bash
cd src/start/GenAiLab
dotnet clean
dotnet restore
```

### "Missing workloads"
```bash
sudo dotnet workload update
```

## 📖 More Information

- [Codespaces Documentation](.devcontainer/README.md)
- [Workshop README](README.md)
- [Part 1: Setup](Part%201%20-%20Setup/README.md)
- [GitHub Codespaces Docs](https://docs.github.com/en/codespaces)

## 🎉 You're Ready!

Everything is set up and ready to go. Start building your AI-powered application! 🚀
