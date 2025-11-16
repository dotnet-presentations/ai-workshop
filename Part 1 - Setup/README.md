# Part 1: Setup

> **⏱️ Estimated Time:** 15-20 minutes (or 3-5 minutes with Codespaces)

## In this workshop

In this workshop, you will set up your development environment for building AI applications with .NET. You'll install the required tools and configure your environment to work with the workshop materials.

## 🚀 Quick Start: Use GitHub Codespaces

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/dotnet-presentations/ai-workshop?quickstart=1)

**No installation required!** Skip the rest of this setup page and start the workshop immediately using GitHub Codespaces. All prerequisites are pre-configured.

- ✅ No Docker installation needed
- ✅ No .NET SDK installation needed  
- ✅ Works with corporate firewalls
- ✅ Starting solution pre-loaded in `src/start/`

👉 [Learn more about Codespaces setup](../.devcontainer/README.md)

After your Codespace loads, continue to [Part 2: Project Creation](../Part%202%20-%20Project%20Creation/README.md).

---

## Local Development Setup

If you prefer to run the workshop on your local machine, follow these steps:

### Prerequisites

Before starting, ensure you have:

- Visual Studio 2022 with the Web & Cloud workload installed
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Podman](https://podman.io/)
- .NET AI Web Chatbot template installed
- .NET 9.0 SDK or later
- GitHub account (required for GitHub Models access)
- Azure subscription (optional, but recommended for full experience)
- GitHub Copilot subscription (optional, but recommended for full experience)

> [!TIP]
> **Alternative: Local AI with Ollama**: If you don't have a GitHub account or prefer to work offline, this workshop includes an Ollama-based development container option that runs AI models locally. See the [Development Container Options](../.devcontainer/README.md) for details.

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

1. **Install Visual Studio 2022:**
   - Download and install Visual Studio 2022 from [https://visualstudio.microsoft.com/downloads/](https://visualstudio.microsoft.com/downloads/)
   - During installation, select the "Web & Cloud" in the workloads selection screen
   - Ensure the ".NET 9.0 SDK" is included in your installation

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

- 🚀 Create a new AI Web Chat project using Visual Studio
- ⚙️ Configure GitHub Models for AI services
- 🔧 Set up the development environment
- 🧪 Test your first AI-powered application
