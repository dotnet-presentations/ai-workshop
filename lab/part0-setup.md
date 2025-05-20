# Part 0: Setup

## In this lab

In this lab, you will set up your development environment for building AI applications with .NET. You'll install the required tools and configure your environment to work with the lab materials.

## Prerequisites

Before starting the lab, ensure you have:

- Visual Studio 2022 with the Web & Cloud workload installed
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or [Podman](https://podman.io/)
- .NET AI Web Chatbot template installed
- .NET 9.0 SDK or later
- GitHub account (required for GitHub Models access)
- Azure subscription (optional, but recommended for full experience)
- GitHub Copilot subscription (optional, but recommended for full experience)

## Clone the Repository

First, clone the lab repository to get all the necessary files:

```powershell
git clone https://github.com/dotnet-presentations/build-2025-lab307.git
cd build-2025-lab307
```

> [!WARNING]
> For the Build 2025 lab, all Azure resources MUST be created in the "rg-mygenaiapp" resource group. This resource group has already been created in the subscription. When creating new resources via the portal, select this resource group rather than creating a new one. Permissions are restricted to only allow creating resources in this resource group, so missing this will cause resource creation to fail.

## Step 1: Install Required Tools

> [!IMPORTANT]
> If you are completing this in the Build lab session, these prerequisites should already be installed on your virtual machine.

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

## Next Steps

Once your setup is complete, proceed to [Part 1 - Create a Project with AI Web Chat Template](part1-create-project.md)
