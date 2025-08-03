---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience using the new per-unit directory structure.
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in each Part directory (Parts 1-9), testing both the provided code snapshots and README documentation. For the MCP extensions (parts 7-9), you will test the MCP server creation and integration but skip the publishing sections. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

Before starting the workshop, you will check for and manage required credentials using environment variables. If credentials are not found, you will prompt for them and save them for subsequent runs.

## Purposes of this testing procedure

There are three main objectives:

1. To validate each part of the workshop independently using the provided code snapshots.
2. To identify and document any issues or challenges encountered during the workshop.
3. To validate the new MCP server creation and integration workflow (Parts 7-9).

Each part contains either a complete working code snapshot or comprehensive README documentation for setup/exploration tasks.

## Workshop Structure

The workshop is now organized into 9 parts using a per-unit directory structure:

**Core AI Web Chat Application (Parts 1-6):**
- Part 1 - Setup: Prerequisites and setup
- Part 2 - Project Creation: Create AI Web Chat project using `dotnet new aichatweb`
- Part 3 - Template Exploration: Explore the generated template code (README only)
- Part 4 - Azure OpenAI: Convert from GitHub Models to Azure OpenAI
- Part 5 - Products Page: Implement products page with vector search
- Part 6 - Deployment: Deploy to Azure (includes complete project with .azure directory)

**Model Context Protocol Extension (Parts 7-9):**
- Part 7 - MCP Server Basics: Create Weather MCP Server using `dotnet new mcpserver`
- Part 8 - Enhanced MCP Server: Create Business MCP Server with ContosoOrders tools
- Part 9 - MCP Publishing: Publish and Share MCP Servers (README only)

**Code Snapshots Structure:**
- `Part 2 - Project Creation/GenAiLab/` - Fresh AI Web Chat template (starter code)
- `Part 6 - Deployment/GenAiLab/` - Complete project with deployment configuration
- `Part 7 - MCP Server Basics/WeatherMcpServer/` - Weather MCP server implementation
- `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/` - Business MCP server implementation

Parts 1, 3, 6, and 9 contain only README files as they focus on setup, exploration, deployment, or publishing rather than code creation.

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

### Using Environment Variables in Workshop

When testing parts that require credentials:

**Part 2 - GitHub Models Setup:**
- Use `$env:WORKSHOP_GITHUB_TOKEN` for GitHub token
- Set in appsettings.Development.json or user secrets

**Part 4 - Azure OpenAI Migration:**
- Use `$env:WORKSHOP_AZURE_OPENAI_ENDPOINT` for the endpoint
- Use `$env:WORKSHOP_AZURE_OPENAI_KEY` for the API key
- Set in appsettings.Development.json or user secrets

**Testing Commands:**
```powershell
# Verify environment variables are set
Write-Host "GitHub Token: $($env:WORKSHOP_GITHUB_TOKEN -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Endpoint: $($env:WORKSHOP_AZURE_OPENAI_ENDPOINT -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Key: $($env:WORKSHOP_AZURE_OPENAI_KEY -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Subscription: $($env:WORKSHOP_AZURE_SUBSCRIPTION_ID -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Location: $($env:WORKSHOP_AZURE_LOCATION -ne $null ? 'Set' : 'Not Set')"

# Set user secrets using environment variables (example for Part 2)
cd "Part 2 - Project Creation/GenAiLab"
dotnet user-secrets init
dotnet user-secrets set "GitHubToken" "$env:WORKSHOP_GITHUB_TOKEN"

# Set user secrets for Azure OpenAI (example for Part 4)
dotnet user-secrets set "AzureOpenAI:Endpoint" "$env:WORKSHOP_AZURE_OPENAI_ENDPOINT"
dotnet user-secrets set "AzureOpenAI:Key" "$env:WORKSHOP_AZURE_OPENAI_KEY"
```

## Important Notes for Testing

1. **GitHub Token**: Use the `WORKSHOP_GITHUB_TOKEN` environment variable. If not set, the credential setup script will prompt for a classic token (no specific scopes needed) or fine-grained token with `models:read` scope. See [GitHub Models announcement](https://github.blog/changelog/2025-05-15-modelsread-now-required-for-github-models-access/) for details.

2. **Azure OpenAI Credentials**: Use the `WORKSHOP_AZURE_OPENAI_ENDPOINT` and `WORKSHOP_AZURE_OPENAI_KEY` environment variables. Azure OpenAI offers simpler setup and better error handling than GitHub Models if you have Azure access.

3. **AI Provider Options**: The workshop starts with GitHub Models for accessibility (all GitHub users have access), then demonstrates migration to Azure OpenAI. OpenAI direct is also an option with similar capabilities.

4. **Code Inconsistencies**: Some documentation may show code that doesn't match the current template exactly. Focus on what actually gets generated and works, not exact code matches in documentation. Templates evolve over time, and the workshop prioritizes functional correctness over documentation precision.

5. **Template Parameter Critical Requirement**: Always use `--vector-store qdrant` when generating AI Web Chat templates. Missing this parameter causes templates to use SQLite instead of Qdrant, leading to documentation misalignment and broken workshop flow.

6. **JavaScript File Dependencies**: The AI Web Chat template includes essential JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) that provide auto-resize textarea and auto-scroll chat functionality. These files must be preserved in all code snapshots.

7. **Complete Implementation Requirements**: Parts 5-6 require full code implementation, not just documentation. Part 5 must include complete Products page functionality, and Part 6 must contain the complete application from Part 5 plus deployment configuration.

6. **MCP Prerequisites**: Parts 7-9 require .NET 10 SDK preview and Visual Studio Code with GitHub Copilot extension. MCP servers use the new `mcpserver` project template and are created as separate projects.

7. **Per-Unit Structure**: Each part is now self-contained with its own directory. Code parts (2, 6, 7, 8) include working project snapshots, while documentation parts (1, 3, 9) contain comprehensive README files.

8. **Build Issues**: Document any build errors and resolution steps during testing.

9. **MCP Server Testing**: Parts 7-9 focus on creating and integrating MCP servers. Test MCP server creation, tool functionality, and VS Code integration but skip publishing steps.

10. **Deployment Validation**: If possible, test actual Azure deployment using the Part 6 code snapshot. Azure deployment is remarkably smooth (~6 minutes total).

## Known Issues and Solutions (From Previous Testing)

Based on comprehensive testing conducted in August 2025, the following issues have been identified and resolved. Follow these guidelines to avoid repeating them:

### Issue 1: Template Generation Missing Vector Database Parameter
**Problem**: Initial Part 2 snapshot was missing `--vector-store qdrant` parameter, causing template to use SQLite instead of Qdrant.
**Solution**: Always use complete command: `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant`
**Validation**: Check that generated template includes Qdrant packages in both AppHost and Web projects.

### Issue 2: Missing JavaScript Files Causing UI Functionality Loss
**Problem**: Essential JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) were missing from Parts 5-6, breaking auto-resize and auto-scroll functionality.
**Solution**: Ensure all code snapshots include these files from the original template.
**Validation**: Verify both files exist in `GenAiLab.Web/Components/Pages/Chat/` directory.

### Issue 3: Duplicate JavaScript Files Causing Build Errors
**Problem**: Build conflicts from duplicate JavaScript files when copying between parts.
**Solution**: Clean duplicates before copying: `Remove-Item -Path "GenAiLab.Web/Components/Pages/Chat/*.razor.js" -Force`, then copy correct files.
**Validation**: Check for duplicates: `Get-ChildItem "GenAiLab.Web" -Recurse -Name "*.razor.js" | Group-Object | Where-Object { $_.Count -gt 1 }`

### Issue 4: Part 5 Implementation Incomplete
**Problem**: Part 5 had only documentation without actual Products page implementation.
**Solution**: Part 5 must include complete implementation with all models, services, and UI components.
**Validation**: Verify ProductInfo.cs, ProductVector.cs, ProductService.cs, and Products.razor all exist and build successfully.

### Issue 5: Part 6 Missing Complete Application
**Problem**: Part 6 lacked the complete application needed for deployment testing.
**Solution**: Part 6 must contain complete Part 5 implementation plus deployment configuration.
**Validation**: Verify Part 6 has all Products page functionality and AppHost includes `WithExternalHttpEndpoints()`.

### Issue 6: Azure Deployment Directory Navigation
**Problem**: `azd` commands failing due to incorrect working directory.
**Solution**: Always navigate to deployment directory first or use `--cwd` parameter.
**Validation**: Ensure `azure.yaml` exists in current directory before running `azd` commands.

### Issue 7: Incomplete Azure Deployment Parameters
**Problem**: `azd provision` failing without required subscription, location, and OpenAI parameters.
**Solution**: Use full command with all parameters from environment variables.
**Validation**: Verify all environment variables are set before deployment.


Based on comprehensive testing:

### What Works Excellently
- **Azure Deployment**: Remarkably smooth (~6 minutes total) via Azure Developer CLI
- **Azure AI Integration**: Much simpler than GitHub Models setup
- **Aspire Experience**: Excellent development and deployment experience
- **Educational Content**: AI concepts and implementation patterns are solid
- **MCP Integration**: .NET 10 MCP template provides smooth server creation experience
- **VS Code Integration**: MCP servers integrate well with GitHub Copilot in VS Code

### Testing Guidelines
- Use Azure AI credentials when available (simpler than GitHub Models)
- Document all build errors and resolution steps meticulously
- Test actual deployment when possible for complete validation
- Focus on educational value and learning objectives
- Validate that each part flows logically to the next
- Use updated PowerShell commands for Windows environment testing

## Procedure

**Prerequisites: Run the credential setup script before starting the workshop to configure environment variables.**

The workshop now uses a per-unit directory structure where each part is self-contained:

1. **Part 1 - Setup**: Follow the README.md in `Part 1 - Setup/` for prerequisites and environment setup.

2. **Part 2 - Project Creation**: Use the provided code snapshot in `Part 2 - Project Creation/GenAiLab/` as your starting point. This contains a fresh AI Web Chat template created with `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant`.

3. **Part 3 - Template Exploration**: Follow the README.md in `Part 3 - Template Exploration/` to understand the generated template structure.

4. **Part 4 - Azure OpenAI**: Follow the README.md instructions to convert from GitHub Models to Azure OpenAI, building upon the Part 2 code snapshot.

5. **Part 5 - Products Page**: Follow the README.md instructions to implement the products page with vector search functionality.

6. **Part 6 - Deployment**: Use the provided complete code snapshot in `Part 6 - Deployment/GenAiLab/` which includes the `.azure` directory and deployment configuration. Follow the README.md for deployment instructions, stopping before actual Azure deployment for testing purposes.

7. **Part 7 - MCP Server Basics**: Use the provided Weather MCP Server in `Part 7 - MCP Server Basics/WeatherMcpServer/` or create a new one using `dotnet new mcpserver -n WeatherMcpServer`. Test MCP server functionality and VS Code integration.

8. **Part 8 - Enhanced MCP Server**: Use the provided Business MCP Server in `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/` or create a new one using `dotnet new mcpserver -n ContosoOrdersMcpServer`. Test enhanced business tools and multi-server configuration.

9. **Part 9 - MCP Publishing**: Follow the README.md in `Part 9 - MCP Publishing/` for publishing workflow documentation. Skip actual publishing during testing.

### Critical Testing Notes Based on Experience

**Part 1**: Environment setup and prerequisites - ensure all required SDKs and tools are installed.

**Part 2**: Test the provided starter code snapshot builds and runs correctly with basic chat functionality. Template should be generated with `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant` to include Aspire orchestration and Qdrant vector database.

**⚠️ CRITICAL**: Always use the `--vector-store qdrant` parameter when generating templates. Missing this parameter causes template to use SQLite instead of Qdrant, leading to documentation misalignment.

**Part 3**: Verify template exploration documentation is clear and helpful for understanding the codebase.

**Part 4**: Azure OpenAI integration - follow the README instructions carefully. Common issues:
- Build errors from static asset conflicts: `dotnet clean && dotnet build`
- Package restore problems: `dotnet restore && dotnet build`

**Part 5**: Products page implementation - test vector search functionality and integration.

**⚠️ CRITICAL**: Part 5 requires complete implementation, not just documentation. Common issues:
- Missing JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) that provide essential UI functionality
- Duplicate JavaScript files causing build conflicts - clean up duplicates if they exist
- Missing package references for QuickGrid components
- Incomplete service registrations in Program.cs
- Missing navigation links in ChatHeader component

**Build Issue Resolution for Part 5**:
```powershell
# If duplicate JavaScript files cause build errors
dotnet clean
Remove-Item -Path "GenAiLab.Web/Components/Pages/Chat/*.razor.js" -Force -ErrorAction SilentlyContinue
# Then restore the correct JavaScript files from Part 2 template
dotnet build
```

**Part 6**: If possible, test actual Azure deployment using the complete code snapshot. Deployment is remarkably smooth (~6 minutes).

**⚠️ CRITICAL**: Part 6 must contain the complete application from Part 5, not just deployment configuration. Ensure:
- AppHost includes `WithExternalHttpEndpoints()` for Azure deployment
- All Products page functionality is present
- Essential JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) are included
- Use full azd command with all parameters: `azd provision --subscription "$env:WORKSHOP_AZURE_SUBSCRIPTION_ID" --location "$env:WORKSHOP_AZURE_LOCATION" --set openai="Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY"`

**Azure Deployment Directory Navigation**:
```powershell
# Always use explicit working directory for azd commands to avoid navigation issues
cd "Part 6 - Deployment/GenAiLab"
azd provision --subscription "$env:WORKSHOP_AZURE_SUBSCRIPTION_ID" --location "$env:WORKSHOP_AZURE_LOCATION" --set openai="Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY"
# Or use --cwd parameter
azd provision --cwd "C:\path\to\Part 6 - Deployment\GenAiLab" --subscription "$env:WORKSHOP_AZURE_SUBSCRIPTION_ID" --location "$env:WORKSHOP_AZURE_LOCATION" --set openai="Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY"
```

**Part 7**: MCP Weather Server creation and testing:
- Test the provided WeatherMcpServer code snapshot or create new using `dotnet new mcpserver`
- Verify MCP server starts and weather tools are accessible
- Test VS Code integration with GitHub Copilot

**Part 8**: Business MCP Server creation and testing:
- Test the provided ContosoOrdersMcpServer code snapshot
- Verify more complex business scenario tools work correctly
- Test multi-server configuration and tool integration with GitHub Copilot

**Part 9**: MCP Publishing workflow (skip actual publishing during testing):
- Review publishing documentation for completeness
- Verify integration documentation is clear and comprehensive

**Documentation Alignment**: Focus on what actually works vs. exact documentation matches. Some documentation may not perfectly match the current template implementation, which is expected as templates evolve.

## Evaluation

1. At the completion of each part, verify that the provided code snapshots work correctly and the README documentation is clear and actionable.
2. For code parts (2, 6, 7, 8), ensure the projects build successfully and core functionality works as expected.
3. For documentation parts (1, 3, 9), verify the instructions are comprehensive and easy to follow.
4. Document any issues or challenges encountered, including any clarifications needed for the instructions, missing prerequisites, or unclear steps.
5. Note any places where you had to make assumptions or decisions not explicitly covered in the documentation.
6. Test the progression between parts to ensure the workshop flows logically from one part to the next.

## Reproducible Commands for Common Issues

### Environment Variable Setup
```powershell
# Run credential setup script (recommended)
.\.github\scripts\setup-workshop-credentials.ps1

# Quick check of environment variables
Write-Host "GitHub Token: $($env:WORKSHOP_GITHUB_TOKEN -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Endpoint: $($env:WORKSHOP_AZURE_OPENAI_ENDPOINT -ne $null ? 'Set' : 'Not Set')"  
Write-Host "Azure Key: $($env:WORKSHOP_AZURE_OPENAI_KEY -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Subscription: $($env:WORKSHOP_AZURE_SUBSCRIPTION_ID -ne $null ? 'Set' : 'Not Set')"
Write-Host "Azure Location: $($env:WORKSHOP_AZURE_LOCATION -ne $null ? 'Set' : 'Not Set')"
```

### Build Troubleshooting
```powershell
# Clean build artifacts if conflicts occur
dotnet clean
dotnet build

# Package restore if needed
dotnet restore
dotnet build

# Part 5 specific: Remove duplicate JavaScript files that cause build conflicts
# Only run this if build errors mention duplicate JavaScript files
Remove-Item -Path "GenAiLab.Web/Components/Pages/Chat/*.razor.js" -Force -ErrorAction SilentlyContinue
# Then restore correct JavaScript files from template or Part 2 snapshot
Copy-Item "Part 2 - Project Creation/GenAiLab/GenAiLab.Web/Components/Pages/Chat/ChatInput.razor.js" "GenAiLab.Web/Components/Pages/Chat/"
Copy-Item "Part 2 - Project Creation/GenAiLab/GenAiLab.Web/Components/Pages/Chat/ChatMessageList.razor.js" "GenAiLab.Web/Components/Pages/Chat/"
dotnet build
```

### Git-Based Testing Validation
```powershell
# Test each code snapshot builds successfully
cd "Part 2 - Project Creation/GenAiLab" && dotnet build --verbosity minimal
cd "Part 6 - Deployment/GenAiLab" && dotnet build --verbosity minimal
cd "Part 7 - MCP Server Basics/WeatherMcpServer" && dotnet build --verbosity minimal
cd "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer" && dotnet build --verbosity minimal

# Verify workshop structure
Get-ChildItem "Part */README.md"  # Should show all 9 README files
Get-ChildItem "Part */*/Program.cs"  # Should show main program files in code parts
```

### Testing Validation Commands
```powershell
# Setup credentials first (if not already done)
.\.github\scripts\setup-workshop-credentials.ps1

# Test starter project (Part 2) with GitHub token
cd "Part 2 - Project Creation/GenAiLab"
dotnet user-secrets init
dotnet user-secrets set "GitHubToken" "$env:WORKSHOP_GITHUB_TOKEN"
dotnet build --verbosity minimal

# Test complete project (Part 6) with Azure OpenAI credentials  
cd "Part 6 - Deployment/GenAiLab"
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "$env:WORKSHOP_AZURE_OPENAI_ENDPOINT"
dotnet user-secrets set "AzureOpenAI:Key" "$env:WORKSHOP_AZURE_OPENAI_KEY"
dotnet build --verbosity minimal

# Check workshop structure
Get-ChildItem "Part */README.md"  # Should show README files for all 9 parts

# Test MCP servers (Parts 7-8)
cd "Part 7 - MCP Server Basics/WeatherMcpServer" && dotnet build --verbosity minimal
cd "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer" && dotnet build --verbosity minimal
```

### MCP Server Testing Commands (Parts 7-9)
```powershell
# Create and test Weather MCP Server (Part 7)
dotnet new mcpserver -n WeatherMcpServer
cd WeatherMcpServer && dotnet build --verbosity minimal

# Create and test Business MCP Server (Part 8)  
dotnet new mcpserver -n ContosoOrdersMcpServer
cd ContosoOrdersMcpServer && dotnet build --verbosity minimal

# Verify MCP configuration files are created
Get-ChildItem *.json  # Should see mcp-server-config.json files
```

### AI Web Chat Template Testing Commands (Part 2)
```powershell
# Create fresh AI Web Chat template with correct parameters
# ⚠️ CRITICAL: Always include --vector-store qdrant parameter
dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant

# Build and test the template
cd GenAiLab && dotnet build --verbosity minimal

# Set up GitHub token for testing
dotnet user-secrets set "GitHubToken" "$env:WORKSHOP_GITHUB_TOKEN" --project GenAiLab.Web

# Verify Qdrant configuration is present (should show Qdrant-related packages)
Get-Content "GenAiLab.AppHost/GenAiLab.AppHost.csproj" | Select-String "Qdrant"
Get-Content "GenAiLab.Web/GenAiLab.Web.csproj" | Select-String "Qdrant"
```

### Part 5 Implementation Validation Commands
```powershell
# Check that Part 5 has complete implementation, not just documentation
cd "Part 5 - Products Page/GenAiLab"

# Verify essential components exist
Test-Path "GenAiLab.Web/Models/ProductInfo.cs"  # Should be True
Test-Path "GenAiLab.Web/Models/ProductVector.cs"  # Should be True  
Test-Path "GenAiLab.Web/Services/ProductService.cs"  # Should be True
Test-Path "GenAiLab.Web/Components/Pages/Products.razor"  # Should be True

# Verify essential JavaScript files are present
Test-Path "GenAiLab.Web/Components/Pages/Chat/ChatInput.razor.js"  # Should be True
Test-Path "GenAiLab.Web/Components/Pages/Chat/ChatMessageList.razor.js"  # Should be True

# Check for duplicate JavaScript files that cause build issues
Get-ChildItem "GenAiLab.Web" -Recurse -Name "*.razor.js" | Group-Object | Where-Object { $_.Count -gt 1 }

# Clean build if issues found
if ($LASTEXITCODE -ne 0) {
    dotnet clean
    dotnet build --verbosity minimal
}
```

### Part 6 Deployment Validation Commands  
```powershell
# Verify Part 6 has complete application from Part 5
cd "Part 6 - Deployment/GenAiLab"

# Check AppHost is configured for external HTTP endpoints
Get-Content "GenAiLab.AppHost/Program.cs" | Select-String "WithExternalHttpEndpoints"

# Verify complete application is present (should have Products page functionality)
Test-Path "GenAiLab.Web/Components/Pages/Products.razor"  # Should be True
Test-Path "GenAiLab.Web/Services/ProductService.cs"  # Should be True

# Verify essential JavaScript files are present  
Test-Path "GenAiLab.Web/Components/Pages/Chat/ChatInput.razor.js"  # Should be True
Test-Path "GenAiLab.Web/Components/Pages/Chat/ChatMessageList.razor.js"  # Should be True

# Test build before deployment
dotnet build --verbosity minimal
```

### Azure Deployment Testing (If Credentials Available)
```powershell
# Navigate to complete project for deployment testing
cd "Part 6 - Deployment/GenAiLab"

# Update azd if needed
winget upgrade Microsoft.Azd

# Initialize for deployment (if azure.yaml doesn't exist)
azd init
# Follow prompts: "Use code in current directory" → "Confirm" → "Environment name"

# Provision with full parameters
azd provision --subscription "$env:WORKSHOP_AZURE_SUBSCRIPTION_ID" --location "$env:WORKSHOP_AZURE_LOCATION" --set openai="Endpoint=$env:WORKSHOP_AZURE_OPENAI_ENDPOINT;Key=$env:WORKSHOP_AZURE_OPENAI_KEY"

# Deploy the application
azd deploy

# View results
azd show

# Cleanup when done
azd down --purge --force
```