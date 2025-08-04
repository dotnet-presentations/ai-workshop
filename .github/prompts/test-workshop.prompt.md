````prompt
---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience using the per-unit directory structure.
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in each Part directory (Parts 1-9), testing both the provided code snapshots and README documentation. For the MCP extensions (parts 7-9), you will test the MCP server creation and integration but skip the publishing sections. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

Before starting the workshop, you will check for and manage required credentials using environment variables. If credentials are not found, you will prompt for them and save them for subsequent runs.

## Purposes of this testing procedure

There are three main objectives:

1. To validate each part of the workshop independently using the provided code snapshots.
2. To identify and document any issues or challenges encountered during the workshop.
3. To validate the MCP server creation and integration workflow (Parts 7-9).

Each part contains either a complete working code snapshot or comprehensive README documentation for setup/exploration tasks.

## Workshop Structure

The workshop is organized into 9 parts using a per-unit directory structure:

**Core AI Web Chat Application (Parts 1-6):**
- Part 1 - Setup: Prerequisites and setup
- Part 2 - Project Creation: Create AI Web Chat project using `dotnet new aichatweb`
- Part 3 - Template Exploration: Explore the generated template code (README only)
- Part 4 - Azure OpenAI: Convert from GitHub Models to Azure OpenAI
- Part 5 - Products Page: Implement products page with vector search
- Part 6 - Deployment: Deploy to Azure (includes complete project with .azure directory)

**Model Context Protocol Extension (Parts 7-9):**
- Part 7 - MCP Server Basics: Create MyMcpServer using `dotnet new mcpserver` with both template RandomNumberTools and custom WeatherTools
- Part 8 - Enhanced MCP Server: Create Business MCP Server with ContosoOrders tools
- Part 9 - MCP Publishing: Publish and Share MCP Servers (README only)

**Code Snapshots Structure:**
- `Part 2 - Project Creation/GenAiLab/` - Fresh AI Web Chat template (starter code)
- `Part 6 - Deployment/GenAiLab/` - Complete project with deployment configuration
- `Part 7 - MCP Server Basics/MyMcpServer/` - Generic MCP server with RandomNumberTools and WeatherTools
- `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/` - Business MCP server implementation

Parts 1, 3, and 9 contain only README files as they focus on setup, exploration, or publishing rather than code creation.

## Environment Variables and Credential Management

Before starting the workshop, the following environment variables should be checked and configured if needed:

- `WORKSHOP_GITHUB_TOKEN` - GitHub personal access token for GitHub Models access
- `WORKSHOP_AZURE_OPENAI_ENDPOINT` - Azure OpenAI service endpoint URL
- `WORKSHOP_AZURE_OPENAI_KEY` - Azure OpenAI service API key
- `WORKSHOP_AZURE_SUBSCRIPTION_ID` - Azure subscription ID for deployment
- `WORKSHOP_AZURE_LOCATION` - Azure location/region for deployment

### Credential Setup Script

A PowerShell script is provided at `.github/scripts/setup-workshop-credentials.ps1` to automatically manage workshop credentials. Run this before starting the workshop:

```powershell
# Navigate to workshop root and run credential setup
.\.github\scripts\setup-workshop-credentials.ps1
```

The script will check for the required environment variables and prompt for any missing credentials, then save them persistently for future testing sessions.

## Important Notes for Testing

1. **GitHub Token**: Use the `WORKSHOP_GITHUB_TOKEN` environment variable. Classic token (no specific scopes needed) or fine-grained token with `models:read` scope.

2. **Azure OpenAI Credentials**: Use the `WORKSHOP_AZURE_OPENAI_ENDPOINT` and `WORKSHOP_AZURE_OPENAI_KEY` environment variables.

3. **Template Parameter Critical Requirement**: Always use `--vector-store qdrant` when generating AI Web Chat templates. Missing this parameter causes templates to use SQLite instead of Qdrant, leading to documentation misalignment.

4. **JavaScript File Dependencies**: The AI Web Chat template includes essential JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) that provide auto-resize textarea and auto-scroll functionality. These files must be preserved in all code snapshots.

5. **Complete Implementation Requirements**: Parts 5-6 require full code implementation, not just documentation. Part 5 must include complete Products page functionality, and Part 6 must contain the complete application from Part 5 plus deployment configuration.

6. **MCP Prerequisites**: Parts 7-9 require .NET 10 SDK preview and Visual Studio Code with GitHub Copilot extension. Part 7 uses `MyMcpServer` project that includes both template-generated RandomNumberTools and custom WeatherTools.

7. **Per-Unit Structure**: Each part is self-contained with its own directory. Code parts (2, 6, 7, 8) include working project snapshots, while documentation parts (1, 3, 9) contain comprehensive README files.

## Key Testing Requirements

- Always use `--vector-store qdrant` when generating AI Web Chat templates
- Ensure JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) are preserved in all snapshots
- Verify Parts 5-6 have complete implementations, not just documentation
- Test MCP server functionality in Parts 7-8 but skip publishing in Part 9
- Use environment variables for credentials and test with actual services when possible

## Procedure

**Prerequisites: Run the credential setup script before starting the workshop to configure environment variables.**

The workshop uses a per-unit directory structure where each part is self-contained:

1. **Part 1 - Setup**: Follow the README.md in `Part 1 - Setup/` for prerequisites and environment setup.

2. **Part 2 - Project Creation**: Use the provided code snapshot in `Part 2 - Project Creation/GenAiLab/` as your starting point. This contains a fresh AI Web Chat template created with `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant`.

3. **Part 3 - Template Exploration**: Follow the README.md in `Part 3 - Template Exploration/` to understand the generated template structure.

4. **Part 4 - Azure OpenAI**: Follow the README.md instructions to convert from GitHub Models to Azure OpenAI, building upon the Part 2 code snapshot.

5. **Part 5 - Products Page**: Follow the README.md instructions to implement the products page with vector search functionality.

6. **Part 6 - Deployment**: Use the provided complete code snapshot in `Part 6 - Deployment/GenAiLab/` which includes the `.azure` directory and deployment configuration. Follow the README.md for deployment instructions, stopping before actual Azure deployment for testing purposes.

7. **Part 7 - MCP Server Basics**: Use the provided MyMcpServer in `Part 7 - MCP Server Basics/MyMcpServer/` which contains both template-generated RandomNumberTools and custom WeatherTools. Test MCP server functionality and VS Code integration.

8. **Part 8 - Enhanced MCP Server**: Use the provided Business MCP Server in `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/` or create a new one using `dotnet new mcpserver -n ContosoOrdersMcpServer`. Test enhanced business tools and multi-server configuration.

9. **Part 9 - MCP Publishing**: Follow the README.md in `Part 9 - MCP Publishing/` for publishing workflow documentation. Skip actual publishing during testing.

### Critical Testing Notes

**Part 1**: Environment setup and prerequisites verification.

**Part 2**: Test provided starter code snapshot builds and runs with basic chat functionality. Use `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant`. Update NuGet packages to latest versions.

**Part 3**: Verify template exploration documentation clarity.

**Part 4**: Test Azure OpenAI integration following README instructions.

**Part 5**: Test Products page implementation and vector search functionality. Ensure complete implementation exists.

**Part 6**: Test Azure deployment using complete code snapshot. Verify all functionality from Part 5 is included.

**Part 7**: Test MyMcpServer with both RandomNumberTools and WeatherTools. Verify VS Code integration with GitHub Copilot.

**Part 8**: Test ContosoOrdersMcpServer business scenarios and multi-server configuration.

**Part 9**: Review MCP publishing documentation (skip actual publishing).

## Evaluation

1. At the completion of each part, verify that the provided code snapshots work correctly and the README documentation is clear and actionable.
2. For code parts (2, 6, 7, 8), ensure the projects build successfully and core functionality works as expected.
3. For documentation parts (1, 3, 9), verify the instructions are comprehensive and easy to follow.
4. Document any issues or challenges encountered, including any clarifications needed for the instructions, missing prerequisites, or unclear steps.
5. Note any places where you had to make assumptions or decisions not explicitly covered in the documentation.
6. Test the progression between parts to ensure the workshop flows logically from one part to the next.

## Essential Testing Commands

### Setup
```powershell
# Run credential setup script (recommended)
.\.github\scripts\setup-workshop-credentials.ps1

# Verify environment variables
Write-Host "GitHub Token: $($env:WORKSHOP_GITHUB_TOKEN -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Endpoint: $($env:WORKSHOP_AZURE_OPENAI_ENDPOINT -ne $null ? 'Set' : 'Not Set')"
```

### Build Testing
```powershell
# Test each code snapshot builds successfully
cd "Part 2 - Project Creation/GenAiLab" && dotnet build --verbosity minimal
cd "Part 6 - Deployment/GenAiLab" && dotnet build --verbosity minimal
cd "Part 7 - MCP Server Basics/MyMcpServer" && dotnet build --verbosity minimal
cd "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer" && dotnet build --verbosity minimal

# Clean build if issues occur
dotnet clean && dotnet build
```

### Template Generation (Part 2)
```powershell
# Create AI Web Chat template with correct parameters
dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant

# Update packages
dotnet tool install --global dotnet-outdated-tool
dotnet outdated --upgrade --pre-release Always
```

### Azure Deployment Testing (Part 6)
```powershell
cd "Part 6 - Deployment/GenAiLab"
azd provision --subscription "$env:WORKSHOP_AZURE_SUBSCRIPTION_ID" --location "$env:WORKSHOP_AZURE_LOCATION" --set openai="Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY"
azd deploy
azd down --purge --force  # Cleanup when done
```
````
