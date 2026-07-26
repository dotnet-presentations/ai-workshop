# .NET AI Workshop Repository

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

This repository contains a comprehensive .NET AI workshop with 11 parts covering AI application development (Parts 1-5), Model Context Protocol (MCP) servers (Parts 6-8, where Parts 7-8 are optional/bonus), agents (Part 9), an applied capstone (Part 10), and Azure deployment (Part 11). The workshop teaches building AI-powered applications using .NET 10, Blazor, Microsoft Extensions for AI, Microsoft Foundry (Azure OpenAI), and vector databases.

## Working Effectively

### Prerequisites and Environment Setup
- Install .NET 10.0 SDK for AI Web Chat applications (Parts 1-5 and 11):
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
  export PATH="$HOME/.dotnet:$PATH"
  ```
- Install .NET 10.0 SDK for MCP servers (Parts 6-8)
- Install Docker Desktop or Podman (required for Aspire orchestration and Qdrant vector database)
- Install Microsoft Extensions AI templates:
  ```bash
  dotnet new install Microsoft.Extensions.AI.Templates
  ```

### Build and Restore Commands
- **CRITICAL TIMING**: Set timeout to 300+ seconds for all build commands. NEVER CANCEL builds.
- Bootstrap any solution/project:
  ```bash
  export PATH="$HOME/.dotnet:$PATH"
  dotnet restore [solution-or-project-path]  # Takes 5-20 seconds typically
  dotnet build [solution-or-project-path] --configuration Release  # Takes 2-15 seconds typically
  ```

### Major Project Solutions

1. **Part 2 Chat App (console)**: `Part 2 - Build Chat App/ChatApp/ChatApp.csproj`
   - .NET 10.0 console chat app built by hand

2. **Part 6 MCP Server (Basic)**: `Part 6 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj`
   - .NET 10.0 MCP server with weather tools
   - Build time: ~2 seconds, Restore time: ~5 seconds

3. **Part 7 MCP Server (Enhanced)**: `Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj`
   - .NET 10.0 business MCP server with order management tools
   - Build time: ~4 seconds with 3 warnings (expected)

4. **Part 10 Store App (capstone)**: `Part 10 - Adding AI to an Existing App/StoreApp/StoreApp.csproj`
   - .NET 10.0 console app that adds AI to an existing store application

5. **Part 11 AI Web Chat (Full)**: `Part 11 - Deployment/GenAiLab/GenAiLab.sln`
   - Complete AI Web Chat with deployment configuration

### Running Applications
- **AI Web Chat applications** require Docker for Qdrant vector database:
  ```bash
  # Ensure Docker is running first
  docker --version
  cd "Part 11 - Deployment/GenAiLab" 
  dotnet run --project GenAiLab.AppHost  # Starts Aspire orchestration
  ```
- **MCP servers** run as console applications:
  ```bash
  dotnet run --project "Part 6 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj"
  # Starts stdio server transport, use Ctrl+C to stop
  ```

## Validation and Testing

### Build Validation
- Always run the GitHub Actions workflow commands to ensure CI compatibility:
  ```bash
  # Test all solutions that are in CI
  dotnet restore "Part 2 - Build Chat App/ChatApp/ChatApp.csproj"
  dotnet build "Part 2 - Build Chat App/ChatApp/ChatApp.csproj" --no-restore --configuration Release
  
  dotnet restore "Part 11 - Deployment/GenAiLab/GenAiLab.sln"
  dotnet build "Part 11 - Deployment/GenAiLab/GenAiLab.sln" --no-restore --configuration Release
  
  dotnet restore "Part 6 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj"
  dotnet build "Part 6 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj" --no-restore --configuration Release
  
  dotnet restore "Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj"
  dotnet build "Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj" --no-restore --configuration Release

  dotnet restore "Part 10 - Adding AI to an Existing App/StoreApp/StoreApp.csproj"
  dotnet build "Part 10 - Adding AI to an Existing App/StoreApp/StoreApp.csproj" --no-restore --configuration Release
  ```

### Manual Validation Scenarios
- **ALWAYS run through complete scenarios** after making changes:
  1. **MCP Server Validation**: Run MCP server and verify it starts with proper logging output and responds to Ctrl+C shutdown
  2. **AI Web Chat Validation**: Requires Azure OpenAI credentials for full testing
  3. **Template Validation**: Test creating new projects with templates:
     ```bash
     dotnet new aichatweb --name TestApp --output /tmp/test-app
     dotnet new mcpserver --name TestMcp --output /tmp/test-mcp
     ```

### Expected Build Warnings
- Part 7 Enhanced MCP Server produces 3 expected CS1998 warnings about async methods without await - this is normal

## Template Usage

### AI Chat Web App Template
```bash
dotnet new aichatweb --help  # See all options
# Key options:
# --provider: azureopenai, ollama, openai
# --vector-store: local (default), azureaisearch, qdrant
# --aspire: false (default), true for distributed applications
```

### MCP Server Template
```bash
dotnet new mcpserver --help  # See all options
# Creates basic MCP server with console application structure
```

## Repository Structure

### Workshop Parts (Documentation + Code)
- `Part 1 - Setup/`: Prerequisites and setup (README only)
- `Part 2 - Build Chat App/`: Build the chat app by hand using Microsoft.Extensions.AI
- `Part 3 - Add RAG/`: Add embeddings, retrieval, and prompt augmentation
- `Part 4 - AI Web Chat Template/`: Scaffold and inspect the template-generated web app
- `Part 5 - Providers and Fallbacks/`: Compare provider swap strategies and fallback paths
- `Part 6 - MCP Server Basics/`: Basic MCP server with weather tools
- `Part 7 - Enhanced MCP Server/`: Business MCP server with order tools (optional/bonus)
- `Part 8 - MCP Publishing/`: Publishing and distribution guide, README only (optional/bonus)
- `Part 9 - Agent Framework Basics/`: Agent essentials with a standalone `AgentApp` sample
- `Part 10 - Adding AI to an Existing App/`: Capstone `StoreApp` sample that adds AI to an existing app
- `Part 11 - Deployment/`: Complete project with Azure deployment configuration

### Infrastructure
- `.github/workflows/`: CI/CD workflows for automated building
- `.github/scripts/`: PowerShell credential setup scripts
- `docs/`: Testing procedures and instructor guides
- `manuals/`: Product documentation PDFs for AI chatbot reference

## Common Tasks

### Check Prerequisites
```bash
# Verify .NET SDKs
dotnet --list-sdks

# Verify Docker
docker --version && docker info | head -5

# Verify templates
dotnet new aichatweb --help
dotnet new mcpserver --help
```

### Credential Management
- Use `.github/scripts/setup-workshop-credentials.ps1` for setting up Azure credentials
- Environment variables: `WORKSHOP_AZURE_OPENAI_ENDPOINT`, `WORKSHOP_AZURE_OPENAI_KEY`

### Timing Expectations
- **NEVER CANCEL**: All builds complete in under 30 seconds. Set timeout to 300+ seconds minimum.
- **Package restore**: 5-20 seconds depending on cache
- **Build**: 2-15 seconds for most projects
- **Application startup**: MCP servers start immediately, AI Web Chat requires Docker Qdrant container

## Key Technologies Used

- **.NET 10**: AI Web Chat applications with Blazor and Aspire
- **.NET 10**: MCP server applications  
- **Microsoft Extensions for AI**: Core AI integration libraries
- **Microsoft Foundry (Azure OpenAI)**: Cloud AI provider used for workshop exercises
- **Azure OpenAI**: Enterprise-grade AI models for production
- **Qdrant**: Vector database for embeddings and semantic search
- **Docker**: Container orchestration for vector databases
- **Aspire**: Cloud-native application orchestration

## Troubleshooting

### Build Issues
- Ensure correct .NET SDK version is installed and in PATH
- For .NET 10 issues, verify SDK installation: `dotnet --list-sdks | grep 10.0`
- For Docker issues with AI Web Chat, ensure Docker Desktop is running

### Template Issues  
- Reinstall templates if missing: `dotnet new install Microsoft.Extensions.AI.Templates`
- Verify template installation: `dotnet new aichatweb --help`

### MCP Server Issues
- MCP servers expect stdio communication protocol - they will start and wait for input
- Use Ctrl+C to gracefully shutdown MCP servers
- Check for proper logging output indicating successful startup