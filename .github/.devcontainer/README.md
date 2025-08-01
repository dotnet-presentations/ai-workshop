# Development Container Options

[![Open in Dev Containers](https://img.shields.io/static/v1?label=Dev%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/dotnet-presentations/ai-workshop)

This workshop provides two development container configurations to suit different needs:

## 🌐 **Standard Container** (`devcontainer.json`)

**Best for:** Most users following the standard workshop path

**Includes:**

- .NET 9.0 with Aspire workload
- Docker-in-Docker (for Qdrant vector database)
- Azure CLI and Azure Developer CLI
- GitHub CLI
- AI Web Chat template pre-installed

**Use this if you:**

- Have a GitHub account for GitHub Models
- Plan to use Azure OpenAI
- Want to follow the workshop as designed

## 🤖 **Ollama Container** (`Ollama/devcontainer.json`)

**Best for:** Developers who want to work offline or use local models

**Includes everything from Standard Container, plus:**

- Ollama for running local LLMs
- Pre-pulled models: `llama3.2`, `phi4-mini`, `all-minilm`

**Use this if you:**

- Don't have a GitHub account
- Hit GitHub Models token limits
- Want to work completely offline
- Prefer using local models for privacy/security
- Want to experiment with different models

## 🚀 **Getting Started**

[![Open in Dev Containers](https://img.shields.io/static/v1?label=Dev%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/dotnet-presentations/ai-workshop)

1. **Open in Dev Container:** Use the "Dev Containers: Reopen in Container" command in VS Code
2. **Choose your option:** VS Code will ask which devcontainer to use
3. **Wait for setup:** The container will install all dependencies automatically
4. **Start building:** Follow the [workshop instructions](../../lab/part0-setup.md)!

## 💡 **Switching Between Options**

You can easily switch between containers by:

1. Reopening the folder in VS Code
2. Selecting "Dev Containers: Reopen in Container"
3. Choosing the other devcontainer option

The workshop code works with both configurations - just update your AI provider settings as needed!
