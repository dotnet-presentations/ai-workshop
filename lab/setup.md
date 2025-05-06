# Setup

## In this lab

In this lab, you will set up your development environment for building AI applications with .NET. You'll install the required tools and configure your environment to work with the lab materials.

## Prerequisites

Before starting the lab, ensure you have:

- Visual Studio 2022 with .NET Aspire workload installed
- .NET AI Web Chatbot template installed
- .NET 9.0 SDK or later
- Azure OpenAI subscription (optional, but recommended for full experience)
- GitHub Copilot subscription (optional, but recommended for full experience)

## Step 1: Install Required Tools

1. **Install Visual Studio 2022:**
   - Download and install Visual Studio 2022 from [https://visualstudio.microsoft.com/downloads/](https://visualstudio.microsoft.com/downloads/)
   - During installation, select the ".NET Aspire workload" in the workloads selection screen
   - Ensure the ".NET 9.0 SDK" is included in your installation

2. **Install .NET AI Web Chatbot template:**
   - Open a terminal or command prompt
   - Run the following command:

     ```powershell
     dotnet new install Microsoft.Extensions.AI.Templates
     ```

3. **Verify installations:**
   - Open a terminal or command prompt
   - Run the following commands to verify the installation:

     ```powershell
     dotnet --list-sdks
     dotnet new --list
     ```

   - You should see .NET 9.0 SDK in the list of SDKs and the AI Web Chatbot template in the list of templates

## Step 2: Clone the Repository

Clone the lab repository to get all the necessary files:

```powershell
git clone https://github.com/dotnet-presentations/build-2025-lab307.git
cd build-2025-lab307
```

## Step 3: Start with a Clean Project

In this lab, you'll start from scratch and build up to the final solution:

1. Create a new AI Web Chat project:

   ```powershell
   mkdir MyGenAiLab
   cd MyGenAiLab
   dotnet new ai-webchatbot
   ```

2. Open the solution in Visual Studio:

   ```powershell
   start MyGenAiLab.sln
   ```

> **Note:** The repository's `src` folder contains the completed solution that you'll build step by step throughout this lab. You can use it as a reference if you get stuck.

## Step 4: Configure GitHub Models

For development, you'll use GitHub Models which are free to use:

1. Configure GitHub Models in your project:
   - Right-click on the `AppHost` project and select "Manage User Secrets"
   - Add the following configuration (no key needed for GitHub Models):

     ```json
     {
       "ConnectionStrings": {
         "openai": "Endpoint=https://models.inference.ai.azure.com"
       }
     }
     ```

2. Build and run the application to verify the setup:
   - Make sure the `AppHost` project is set as the startup project
   - Press F5 or click "Start" to run the application
   - The .NET Aspire dashboard should open, and you should be able to access the web application

## Next Steps

Once your setup is complete, proceed to [Microsoft Extensions for AI (MEAI) fundamentals](part1-meai.md).
