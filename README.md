# .NET AI Workshop

Get up to speed quickly with AI app building in .NET. This workshop covers two tracks: AI application development (Parts 1-6) and MCP server development (Parts 7-9). The AI track uses [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) (Azure OpenAI) as the cloud provider, with local-model options covered in the provider module.

## Prerequisites

### AI Web Chat Application Requirements (Parts 1-6)

- Visual Studio 2026 or VS Code
- .NET AI Web Chatbot template installed (instructions in Part 1 - Setup)
- .NET 10.0 SDK or later
- Docker Desktop or Podman (required for .NET Aspire orchestration)
- Azure subscription with access to [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) (Azure OpenAI) — the primary AI provider

### Model Context Protocol (Parts 7-9)

- .NET 10.0 SDK - Required for MCP development
- Visual Studio Code
- GitHub Copilot subscription (required for MCP testing)
- `Microsoft.Extensions.AI.Templates` package

### Optional but Recommended

- Git for version control
- Azure subscription for production deployment

## Lab Overview 🧪

The workshop is split into two tracks:

- **AI Application Build Path (Parts 1-6):** Build a minimal chat app, add retrieval, inspect the generated template architecture, configure provider strategy, and deploy.
- **Model Context Protocol (MCP) Path (Parts 7-9):** Build MCP servers and package/publish them.

Detailed architecture and sequence diagrams are documented in the individual module READMEs where each concept is implemented.

## Getting Started

Follow the [setup instructions](Part%201%20-%20Setup/README.md) to get started with the lab.

## Lab Modules 📚

The lab is divided into nine modules.

The primary sequence below reflects the current converged workshop flow and keeps advanced modules explicitly optional for pacing.

### AI Application Build Path (Parts 1-6)

This sequence starts with a minimal console chat app, then layers in RAG, template architecture, provider strategy, and deployment.

1. 🏗️ [**Setup**](Part%201%20-%20Setup/README.md): Configure prerequisites and development environment for the AI workshop.

2. 💬 [**Build Chat App**](Part%202%20-%20Build%20Chat%20App/README.md): Build and run a minimal console-based AI chat app.

3. 🧩 [**Add RAG**](Part%203%20-%20Add%20RAG/README.md): Add retrieval-augmented generation with embeddings and semantic search.

4. 🔍 [**AI Web Chat Template**](Part%204%20-%20AI%20Web%20Chat%20Template/README.md): Compare your manual implementation with the generated template architecture.

5. 🔁 [**Providers and Fallbacks**](Part%205%20-%20Providers%20and%20Fallbacks/README.md): Configure provider options and fallback paths ([Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) primary).

6. 🚀 [**Deployment**](Part%206%20-%20Deployment/README.md): Deploy your application to Azure using the Azure Developer CLI.

### Model Context Protocol (MCP) Servers (Parts 7-9)

1. 🔧 [**MCP Server Basics**](Part%207%20-%20MCP%20Server%20Basics/README.md): Create your first MCP server with weather tools that extend AI agents like GitHub Copilot.

2. 🏢 [**Enhanced MCP Server**](Part%208%20-%20Enhanced%20MCP%20Server/README.md) *(Optional / bonus)*: Build sophisticated business tools for order management, inventory, and customer service scenarios.

3. 📦 [**MCP Publishing**](Part%209%20-%20MCP%20Publishing/README.md) *(Optional / bonus)*: Package, publish, and distribute your MCP servers through NuGet for professional deployment.

## Lab Structure 📁

The repository is structured as follows:

- 📖 `Part 1 - Setup` through `Part 9 - MCP Publishing`: Contains all the lab instructions, documentation, and working code snapshots
- 📄 `manuals/`: Product documentation PDFs for the AI chatbot exercises
- 🧪 `docs/testing/`: Testing procedures and validation reports

## Session Resources 📚

|Resources|Links|Description|
|:-------------------|:----------------------------------|:-------------------|
|Microsoft Learn|<https://aka.ms/build25/plan/ADAI_DevStartPlan>|AI developer resources|
|Microsoft Learn|<https://learn.microsoft.com/en-us/dotnet/machine-learning/ai-overview>|.NET AI Documentation|
|Microsoft Learn|<https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview>|.NET Aspire Documentation|
|Microsoft Learn|<https://learn.microsoft.com/en-us/dotnet/machine-learning/extensions-ai/>|Extensions for AI Documentation|
|Microsoft Learn|<https://learn.microsoft.com/en-us/azure/ai-services/openai/>|Azure OpenAI Documentation|

## Testing the Workshop 🧪

For workshop instructors and contributors who want to validate the workshop content, a comprehensive testing procedure is available:

### Automated Credential Setup

Before testing the workshop, run the credential setup script to configure required API keys and endpoints:

```powershell
# Navigate to the workshop root directory
cd ai-workshop

# Run the credential setup script
.\.github\scripts\setup-workshop-credentials.ps1
```

This script will prompt you for:

- **Azure OpenAI Endpoint**: Your Azure OpenAI service endpoint URL
- **Azure OpenAI Key**: Your Azure OpenAI service API key

The credentials are saved as environment variables (`WORKSHOP_AZURE_OPENAI_ENDPOINT`, `WORKSHOP_AZURE_OPENAI_KEY`) and will be available for subsequent testing sessions.

### Testing Procedure

The complete testing procedure and validation scripts are available in `.github/prompts/test-workshop.prompt.md`. This includes:

- Step-by-step testing instructions for all 9 workshop parts
- Automated build validation commands
- Common troubleshooting scenarios
- Documentation improvement tracking

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
