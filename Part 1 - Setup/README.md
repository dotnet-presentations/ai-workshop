# Part 1: Setup

> **⏱️ Estimated Time:** 15-20 minutes

## In this workshop

In this workshop, you will set up your development environment for building AI applications with .NET. You'll install the required tools and configure your environment to work with the workshop materials.

> [!IMPORTANT]
> **GitHub Models is [retiring on July 30, 2026](https://github.blog/changelog/2026-07-01-github-models-is-being-fully-retired-on-july-30-2026/)** (brownouts July 16 & 23). Set up **Azure AI Foundry (Azure OpenAI)** as your primary provider; the GitHub account below is only for the legacy fallback.

## Prerequisites

Before starting, ensure you have:

- **.NET 10 SDK** or later
- **Visual Studio 2026** (with the Web & Cloud workload) **or** Visual Studio Code with the **C# Dev Kit** extension
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Podman](https://podman.io/)
- **An Azure subscription with access to Azure AI Foundry (Azure OpenAI)** — the primary AI provider for this workshop
- GitHub Copilot subscription (recommended; used later for the MCP and GitHub Copilot SDK units)
- (Optional) A GitHub account — GitHub Models is available only as a legacy fallback that is [retiring July 30, 2026](https://github.blog/changelog/2026-07-01-github-models-is-being-fully-retired-on-july-30-2026/)

> [!TIP]
> **No Azure access? Local fallback:** you can run local models with **Foundry Local** or **Ollama** for the chat exercises. See the [Development Container Options](./../.github/.devcontainer/README.md). Note that the full retrieval-augmented generation (RAG) exercise also needs an embedding model.

## Before the workshop: provision Azure AI Foundry ⏳

> [!IMPORTANT]
> Complete this **before** the workshop. Creating the resource and deploying models takes a little time, and model quota sometimes requires a request that isn't instant. Arriving with this done means you can start building right away.

You'll need an **Azure AI Foundry (Azure OpenAI)** resource with **two** models deployed:

1. **Create the resource**
   - Go to the [Azure AI Foundry portal](https://ai.azure.com) and sign in with your Azure account.
   - Create (or select) a project. If prompted, create a new resource group such as `rg-aiworkshop`.

1. **Deploy the chat model**
   - Open **Deployments** → **+ Deploy model** → **Deploy base model**.
   - Deploy **`gpt-4.1-mini`** (leave the default deployment name).

1. **Deploy the embedding model** (required for the RAG exercises)
   - Deploy **`text-embedding-3-small`** (leave the default deployment name).

1. **Confirm quota**
   - If a deployment is blocked by quota, request quota for that model/region. This can take time, so do it early.

1. **Capture your endpoint and key**
   - From a deployment, copy the resource **endpoint** (it looks like `https://YOUR_RESOURCE_NAME.openai.azure.com/`) and an **API key**.
   - Keep these handy for Part 2. If your instructor provided a pre-provisioned resource, use the endpoint and key they gave you.

> [!CAUTION]
> **Never hardcode your key or connection string in source code.** In Part 2 you'll store it with .NET **user secrets** *before* wiring the AI provider. Treat the key like a password — don't paste it into files you might commit.

### Verify your setup

Confirm the SDK and template tooling are installed:

```powershell
dotnet --version          # should report 10.x
dotnet new aichatweb --help
```

## Clone the Repository

First, clone the workshop repository to get all the necessary files:

```powershell
git clone https://github.com/dotnet-presentations/ai-workshop.git
cd ai-workshop
```

> [!NOTE]
> When deploying to Azure, you may need to create resources in a specific resource group (e.g., "rg-mygenaiapp"). If you are following this workshop in a managed environment, use the resource group provided by your instructor or organization. Otherwise, you can create your own resource group as needed.

## Step 1: Install Required Tools

> [!IMPORTANT]
> If you are using a managed or pre-configured environment, some prerequisites may already be installed for you.

1. **Install Visual Studio 2026 or VS Code:**
   - Visual Studio 2026: download from [https://visualstudio.microsoft.com/downloads/](https://visualstudio.microsoft.com/downloads/) and select the "Web & Cloud" workload during installation.
   - Or Visual Studio Code with the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension.
   - Ensure the **.NET 10 SDK** is installed (`dotnet --version` reports `10.x`).

1. **Install .NET AI Web Chatbot template:**
   - Open a terminal or command prompt
   - Run the following command:

     ```powershell
     dotnet new install Microsoft.Extensions.AI.Templates
     ```

1. **Verify installations:**
   - Open a terminal or command prompt
   - Run the following commands to verify the installation:

     ```powershell
     dotnet new aichatweb --help
     ```

   - You should see the help information for the *AI Chat Web App (C#)* displayed.

## Step 2: Create a GitHub Account (if needed)

If you don't already have a GitHub account, follow these steps to create one:

1. **Create a GitHub account:**
   - Go to [https://github.com/signup](https://github.com/signup)
   - Enter your email address and create a password
   - Choose a username
   - Complete the account creation process by following the on-screen instructions
   - Verify your email address (GitHub will send you a verification email)

## 🎯 Next Steps

Ready to build your first AI application? Let's get started!

**Continue to** → [Part 2: Create a Project with AI Web Chat Template](../Part%202%20-%20Project%20Creation/README.md)

In Part 2, you'll learn how to:

- 🚀 Create a new AI Web Chat project using the AI Web Chat template
- ⚙️ Configure Azure AI Foundry (Azure OpenAI) for AI services
- 🔧 Set up the development environment
- 🧪 Test your first AI-powered application
