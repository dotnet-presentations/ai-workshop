# .NET AI Workshop Repository

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

This repository contains a comprehensive .NET AI workshop with 9 parts covering AI web chat applications and Model Context Protocol (MCP) servers. The workshop teaches building AI-powered applications using .NET 9, Blazor, Microsoft Extensions for AI, GitHub Models, Azure OpenAI, and vector databases.

## Working Effectively

### Prerequisites and Environment Setup
- Install .NET 9.0 SDK for AI Web Chat applications (Parts 1-6):
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
  export PATH="$HOME/.dotnet:$PATH"
  ```
- Install .NET 8.0 SDK for MCP servers (Parts 7-8) - typically already available
- Install Docker Desktop or Podman (required for .NET Aspire orchestration and Qdrant vector database)
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
1. **Part 2 AI Web Chat (Basic)**: `Part 2 - Project Creation/GenAiLab/GenAiLab.sln`
   - .NET 9.0 solution with Blazor web app, service defaults, and AppHost
   - Build time: ~11 seconds, Restore time: ~18 seconds
   
2. **Part 6 AI Web Chat (Full)**: `Part 6 - Deployment/GenAiLab/GenAiLab.sln`
   - Complete AI Web Chat with deployment configuration
   - Same build timing as Part 2
   
3. **Part 7 MCP Server (Basic)**: `Part 7 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj`
   - .NET 8.0 MCP server with weather tools
   - Build time: ~2 seconds, Restore time: ~5 seconds
   
4. **Part 8 MCP Server (Enhanced)**: `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj`
   - .NET 8.0 business MCP server with order management tools
   - Build time: ~4 seconds with 3 warnings (expected)

### Running Applications
- **AI Web Chat applications** require Docker for Qdrant vector database:
  ```bash
  # Ensure Docker is running first
  docker --version
  cd "Part 2 - Project Creation/GenAiLab" 
  dotnet run --project GenAiLab.AppHost  # Starts Aspire orchestration
  ```
- **MCP servers** run as console applications:
  ```bash
  dotnet run --project "Part 7 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj"
  # Starts stdio server transport, use Ctrl+C to stop
  ```

## Validation and Testing

### Build Validation
- Always run the GitHub Actions workflow commands to ensure CI compatibility:
  ```bash
  # Test all solutions that are in CI
  dotnet restore "Part 2 - Project Creation/GenAiLab/GenAiLab.sln"
  dotnet build "Part 2 - Project Creation/GenAiLab/GenAiLab.sln" --no-restore --configuration Release
  
  dotnet restore "Part 6 - Deployment/GenAiLab/GenAiLab.sln"
  dotnet build "Part 6 - Deployment/GenAiLab/GenAiLab.sln" --no-restore --configuration Release
  
  dotnet restore "Part 7 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj"
  dotnet build "Part 7 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj" --no-restore --configuration Release
  
  dotnet restore "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj"
  dotnet build "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj" --no-restore --configuration Release
  ```

### Manual Validation Scenarios
- **ALWAYS run through complete scenarios** after making changes:
  1. **MCP Server Validation**: Run MCP server and verify it starts with proper logging output and responds to Ctrl+C shutdown
  2. **AI Web Chat Validation**: Requires GitHub token or Azure OpenAI credentials for full testing
  3. **Template Validation**: Test creating new projects with templates:
     ```bash
     dotnet new aichatweb --name TestApp --output /tmp/test-app
     dotnet new mcpserver --name TestMcp --output /tmp/test-mcp
     ```

### Expected Build Warnings
- Part 8 Enhanced MCP Server produces 3 expected CS1998 warnings about async methods without await - this is normal

## Template Usage

### AI Chat Web App Template
```bash
dotnet new aichatweb --help  # See all options
# Key options:
# --provider: githubmodels (default), azureopenai, ollama, openai
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
- `Part 2 - Project Creation/`: Fresh AI Web Chat template + documentation
- `Part 3 - Template Exploration/`: Template exploration guide (README only)
- `Part 4 - Azure OpenAI/`: README for switching to Azure OpenAI
- `Part 5 - Products Page/`: Complete project with products page
- `Part 6 - Deployment/`: Complete project with Azure deployment configuration
- `Part 7 - MCP Server Basics/`: Basic MCP server with weather tools
- `Part 8 - Enhanced MCP Server/`: Business MCP server with order tools
- `Part 9 - MCP Publishing/`: Publishing and distribution guide (README only)

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
- Use `.github/scripts/setup-workshop-credentials.ps1` for setting up GitHub and Azure credentials
- Environment variables: `WORKSHOP_GITHUB_TOKEN`, `WORKSHOP_AZURE_OPENAI_ENDPOINT`, `WORKSHOP_AZURE_OPENAI_KEY`

### Timing Expectations
- **NEVER CANCEL**: All builds complete in under 30 seconds. Set timeout to 300+ seconds minimum.
- **Package restore**: 5-20 seconds depending on cache
- **Build**: 2-15 seconds for most projects
- **Application startup**: MCP servers start immediately, AI Web Chat requires Docker Qdrant container

## Key Technologies Used

- **.NET 9**: AI Web Chat applications with Blazor and .NET Aspire
- **.NET 8**: MCP server applications  
- **Microsoft Extensions for AI**: Core AI integration libraries
- **GitHub Models**: Free AI models for development
- **Azure OpenAI**: Enterprise-grade AI models for production
- **Qdrant**: Vector database for embeddings and semantic search
- **Docker**: Container orchestration for vector databases
- **.NET Aspire**: Cloud-native application orchestration

## Troubleshooting

### Build Issues
- Ensure correct .NET SDK version is installed and in PATH
- For .NET 9 issues, verify SDK installation: `dotnet --list-sdks | grep 9.0`
- For Docker issues with AI Web Chat, ensure Docker Desktop is running

### Template Issues  
- Reinstall templates if missing: `dotnet new install Microsoft.Extensions.AI.Templates`
- Verify template installation: `dotnet new aichatweb --help`

### MCP Server Issues
- MCP servers expect stdio communication protocol - they will start and wait for input
- Use Ctrl+C to gracefully shutdown MCP servers
- Check for proper logging output indicating successful startup