# .NET AI Workshop

Get up to speed quickly with AI app building in .NET. This workshop covers AI application development (Parts 1-4), MCP server development (Parts 5-7), a short agent bridge module (Part 8), an applied capstone that adds AI to an existing app (Part 9), and deployment preparation and Azure deployment (Parts 10-11). The AI track uses [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) (Azure OpenAI) as the cloud provider, with local-model options covered in the provider module.

## Prerequisites

### AI Web Chat Application Requirements (Parts 1-4, 10, and 11)

- Visual Studio 2026 or VS Code
- .NET AI Web Chatbot template installed (`dotnet new install Microsoft.Extensions.AI.Templates`; walked through in Part 4)
- MCP Server template installed (`dotnet new install Microsoft.McpServer.ProjectTemplates`; walked through in Part 5)
- .NET 10.0 SDK or later
- Docker Desktop or Podman (recommended for the full Qdrant + Aspire path in Part 4)
- Azure subscription with access to [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) (Azure OpenAI), the primary AI provider

### When Docker is required

Docker Desktop or Podman is required when you run the workshop's **Qdrant container locally**. This is the recommended path in Part 4 because it demonstrates a persistent vector database, Aspire orchestration, the Aspire dashboard, and service telemetry.

| Parts | Docker requirement |
| --- | --- |
| Parts 1-3 | Not required. These use console applications and an in-memory vector store. |
| Part 4 | Required for the recommended Qdrant + Aspire path. A supported Docker-free path uses the template's local SQLite vector store without Aspire; see the [Part 4 instructions](Part%2004%20-%20AI%20Web%20Chat%20Template/README.md#alternative-docker-free-path-local-vector-store-without-aspire). |
| Part 5 | Not required for the MCP exercises. |
| Parts 6-7 | Not required. Containerizing an MCP server in Part 7 is optional. |
| Part 8 | Not required. This module includes a standalone `AgentApp` sample and remains Docker-free. |
| Part 9 | Not required. The `eShopLite` sample in this repo is an Aspire solution that deliberately avoids containers — it uses SQLite for both the catalog and the vector store. The companion eShopLite scenarios have their own prerequisites, which usually include a container runtime. |
| Part 10 | Not required. Provider comparisons can use Microsoft Foundry, Foundry Local, or a native Ollama installation. |
| Part 11 | Required to run the supplied Qdrant + Aspire application locally. The Azure deployment itself runs the containers in Azure. |

Without Docker, you can still complete Parts 1-10, including the main chat, RAG, template, MCP, agent, capstone, and provider concepts. In Part 4 you will miss the hands-on Qdrant, Aspire dashboard, service orchestration, and container persistence experience. Part 11 uses the full Qdrant + Aspire application, so local execution of that snapshot requires Docker or Podman.

### Model Context Protocol (Parts 5-7)

- .NET 10.0 SDK - Required for MCP development
- Visual Studio Code
- GitHub Copilot subscription (required for MCP testing)
- `Microsoft.McpServer.ProjectTemplates` package (`dotnet new install Microsoft.McpServer.ProjectTemplates`; walked through in Part 5)

### Optional but Recommended

- Git for version control
- Azure subscription for production deployment

## Lab Overview 🧪

The workshop is split into five sections:

- **AI Application Build Path (Parts 1-4):** Build a minimal chat app, add retrieval and structured output, and inspect the generated template architecture.
- **Model Context Protocol (MCP) Path (Parts 5-7):** Build MCP servers and package/publish them. Only Part 5 is required; Parts 6 and 7 are optional/bonus.
- **Agents (Part 8):** A short bridge module that connects chat, retrieval, and MCP tools into the agent abstraction.
- **Capstone (Part 9):** An applied module on adding targeted AI to an existing application, using eShopLite as the maintained companion implementation.
- **Deployment (Parts 10-11):** Choose your providers and supporting Azure services, then deploy the finished application with the Azure Developer CLI.

### Pacing

The workshop is designed as a **two-part, roughly 8-hour hands-on lab** (8:30-5:30 less an hour for lunch). The core path is Parts 1-5 and 8-11; the sections badged *(Optional / bonus)* exist so groups that move quickly have somewhere to go, and so the schedule keeps slack for everyone else.

| Session | Parts | Budget | Focus |
| --- | --- | --- | --- |
| Morning | Parts 1-4 | ~2h20m-3h30m | Setup, console chat, manual RAG, template reveal |
| Afternoon | Parts 5, 8, 9, 10, 11 | ~2h50m-4h10m | MCP basics, agents, capstone, provider and service choices, deployment |
| Bonus (any time) | Parts 6-7 | ~1h15m-1h45m | Enhanced MCP server and MCP publishing |

> [!IMPORTANT]
> **If you fall behind, cut Parts 10-11 (deployment preparation and deployment) — not Part 9 (the capstone).** Deployment is placed last deliberately: nothing depends on it, and it is the most environment-fragile section (Azure login, subscription and quota checks, provisioning time). Part 10 exists to set up the deployment, so if there is no time to deploy there is little reason to spend time choosing what to deploy to. The capstone is where the whole workshop comes together, so protect that time first. A short `azd up` walkthrough by the instructor is a reasonable substitute for a room that has run out of clock.

<!-- -->

> [!TIP]
> The morning is the tighter half. Ask attendees to complete [Part 1 - Setup](Part%2001%20-%20Setup/README.md) **before** the session — it is prerequisite installation rather than lab content, and moving it to pre-work buys back 15-20 minutes at exactly the point in the day where the schedule has the least slack.

Detailed architecture and sequence diagrams are documented in the individual module READMEs where each concept is implemented.

## Getting Started

Follow the [setup instructions](Part%2001%20-%20Setup/README.md) to get started with the lab.

## Lab Modules 📚

The lab is divided into eleven modules.

The primary sequence below reflects the current converged workshop flow and keeps advanced modules explicitly optional for pacing.

### AI Application Build Path (Parts 1-4)

This sequence starts with a minimal console chat app, then layers in RAG and template architecture.

1. 🏗️ [**Setup**](Part%2001%20-%20Setup/README.md): Configure prerequisites and development environment for the AI workshop.

2. 💬 [**Build Chat App**](Part%2002%20-%20Build%20Chat%20App/README.md): Build and run a minimal console-based AI chat app.

3. 🧩 [**Add RAG**](Part%2003%20-%20Add%20RAG/README.md): Add retrieval-augmented generation with embeddings and semantic search.

4. 🔍 [**AI Web Chat Template**](Part%2004%20-%20AI%20Web%20Chat%20Template/README.md): Compare your manual implementation with the generated template architecture.

### Model Context Protocol (MCP) Servers (Parts 5-7)

Part 5 is required: its weather tool is the tool you hand to an agent in Part 8. Parts 6 and 7 are optional/bonus material.

1. 🔧 [**MCP Server Basics**](Part%2005%20-%20MCP%20Server%20Basics/README.md): Create your first MCP server with weather tools that extend AI agents like GitHub Copilot.

2. 🏢 [**Enhanced MCP Server**](Part%2006%20-%20Enhanced%20MCP%20Server/README.md) *(Optional / bonus)*: Build sophisticated business tools for order management, inventory, and customer service scenarios.

3. 📦 [**MCP Publishing**](Part%2007%20-%20MCP%20Publishing/README.md) *(Optional / bonus)*: Package, publish, and distribute your MCP servers through NuGet for professional deployment.

### Agents (Part 8)

1. 🤖 [**Agent Framework Essentials**](Part%2008%20-%20Agent%20Framework%20Basics/README.md): A short bridge module covering how chat, tools, and memory become an agent with Microsoft Agent Framework, and how MCP tools fit in.

### Capstone (Part 9)

1. 🛒 [**Adding AI to an Existing App with eShopLite**](Part%2009%20-%20Adding%20AI%20to%20an%20Existing%20App/README.md): Apply everything you have learned to an application that already exists. Start from a cut-down eShopLite with no AI in it, then add semantic search to the catalog API, grounded product discovery to the storefront, and an operations assistant running on a local model — before exploring the maintained eShopLite scenarios.

### Deployment (Parts 10-11)

1. 🔁 [**Choosing Providers and Services**](Part%2010%20-%20Choosing%20Providers%20and%20Services/README.md): Decide which model provider and supporting Azure services your application will run against ([Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) primary, with local options).

2. 🚀 [**Deployment**](Part%2011%20-%20Deployment/README.md): Deploy your application to Azure using the Azure Developer CLI.

## Lab Structure 📁

The repository is structured as follows:

- 📖 `Part 01 - Setup` through `Part 11 - Deployment`: Contains all the lab instructions, documentation, and working code snapshots
- 📄 `manuals/`: Product documentation PDFs for the AI chatbot exercises
- 🧪 `docs/`: Instructor guides, testing procedures, and archived reports

Most parts keep their completed code beside their own README. The one exception is
**Part 4**, whose finished solution lives in `Part 11 - Deployment/GenAiLab/` because
Part 11 deploys that same app. It is a correctly completed Part 4 plus the single
`WithExternalHttpEndpoints()` line that Part 11 adds.

## Session Resources 📚

For the end-of-workshop handoff links, see [End-of-Workshop Resources](resources.md).

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

The script prompts for your Microsoft Foundry (Azure OpenAI) endpoint and key, the chat and embedding deployment names, and — optionally — an Azure subscription and region for Part 11 and a local model endpoint for Part 9.

Values are saved as `WORKSHOP_*` environment variables so they persist across testing sessions. Useful switches:

- `-ApplyUserSecrets` writes the Foundry endpoint and key into the user secrets of Parts 2, 3, 8, and 9 (as `AzureOpenAI:Endpoint` / `AzureOpenAI:Key`), the local model settings into Part 9 if configured, and the composed `ConnectionStrings:openai` into the Part 11 AppHost.
- `-Force` re-prompts for values that are already set, which is how you replace a rotated key.
- `-SkipOptional` skips the optional local model prompts.

> [!WARNING]
> API keys are stored in plaintext in User-scoped environment variables. Don't run this on a shared machine.

### Testing Procedure

The complete testing procedure is in the [workshop-testing skill](.github/skills/workshop-testing/SKILL.md). This includes:

- Part-by-part testing instructions for all 11 workshop parts
- Snapshot reconciliation guidance
- Report template (`.github/skills/workshop-testing/report-template.md`)
- Common troubleshooting scenarios

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
