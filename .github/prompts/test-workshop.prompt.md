````prompt
---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience using the per-unit directory structure.
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in each Part directory (Parts 1-9), testing both the provided code snapshots and README documentation. For the MCP extensions (parts 7-9), you will test the MCP server creation and integration but skip the publishing sections. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

### Test Report Requirements

Create a test report markdown file named `workshop-test-report-[DATE].md` in the `docs/testing/` directory with the following structure:

```markdown
# Workshop Test Report - [Date]

## Test Environment
- .NET SDK Version(s): [version info]
- OS: [operating system]
- Docker Version: [if applicable]
- Test Scope: [Parts 1-6 only / Parts 1-9 full test]

## Part-by-Part Results

### Part 1 - Setup
- Status: ✅ Pass / ❌ Fail / ⚠️ Issues
- Notes: [observations, issues, time taken]
- Documentation Clarity: [rating and comments]

### Part 2 - Project Creation
- Status: ✅ Pass / ❌ Fail / ⚠️ Issues
- Notes: [observations, issues, time taken]
- Documentation Clarity: [rating and comments]
- **Snapshot Comparison**: [differences found between generated code and existing snapshot]
- **Snapshot Updated**: ✅ Yes / ❌ No - [details]

[Repeat snapshot comparison section for Parts 4, 5, 6, 7, 8...]

## Snapshot Reconciliation Summary
- **Part 2 - Project Creation/GenAiLab/**: [differences found and resolved]
- **Part 4 - Azure OpenAI/GenAiLab/**: [differences found and resolved]
- **Part 5 - Products Page/GenAiLab/**: [differences found and resolved]
- **Part 6 - Deployment/GenAiLab/**: [differences found and resolved]

## Issues Encountered
[Detailed list of any problems, with steps to reproduce]

## Recommended Documentation Improvements
[Suggestions for clearer instructions, missing steps, etc.]

## Summary
[Overall assessment and recommendations]
```

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

6. **MCP Prerequisites**: Parts 7-9 require .NET 10 SDK preview and Visual Studio Code with GitHub Copilot extension. Part 7 uses `MyMcpServer` project that includes both template-generated RandomNumberTools and custom WeatherTools. **Note**: A partial test run covering only Parts 1-6 can be performed with .NET 9 SDK alone.

7. **MCP VS Code Integration Testing**: For Parts 7-8, VS Code integration testing involves:
   - Verifying the MCP server starts correctly when run via `dotnet run`
   - Checking that the server responds to stdio communication protocol
   - Confirming proper shutdown with Ctrl+C
   - (Optional) Testing GitHub Copilot integration by configuring the server in VS Code settings and verifying tool availability in chat

8. **Per-Unit Structure**: Each part is self-contained with its own directory. Code parts (2, 6, 7, 8) include working project snapshots, while documentation parts (1, 3, 9) contain comprehensive README files.

## Key Testing Requirements

- **CRITICAL: All projects must be created using `dotnet new` commands** as specified in the README instructions. Never create `.csproj` files directly or manually. The workshop tests the template generation workflow, not manual project creation.
- Always use `--vector-store qdrant` when generating AI Web Chat templates
- Ensure JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) are preserved in all snapshots
- Verify Parts 5-6 have complete implementations, not just documentation
- Test MCP server functionality in Parts 7-8 but skip publishing in Part 9
- Use environment variables for credentials and test with actual services when possible

## Procedure

**Prerequisites: Run the credential setup script before starting the workshop to configure environment variables.**

The workshop uses a per-unit directory structure where each part is self-contained. Follow these guidelines for working with code:

### Working Directory Policy and Snapshot Workflow

- **Part 1** contains no code changes and does not require a working directory.

- **For Part 2**, follow the README instructions to create a new project using `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant` in a test working directory (e.g., `test-workspace/GenAiLab/`). **CRITICAL**: You must use the `dotnet new` command to create the project - never copy snapshots or create `.csproj` files manually.
  - **At the end of Part 2**, compare your working directory with the code snapshot in `Part 2 - Project Creation/GenAiLab/`
  - **After reconciling differences**, replace the contents of `Part 2 - Project Creation/GenAiLab/` with your working directory code as the updated snapshot

- **For Part 3**, follow README documentation only (no code changes, just exploration of the Part 2 code).

- **For Part 4**, continue from your Part 2 working directory and apply the README instructions. **IMPORTANT**: Follow the README instructions exactly - if any step requires creating a new project or component, use the `dotnet new` commands specified in the documentation, never create `.csproj` files manually.
  - **At the end of Part 4**, compare your working directory with the code snapshot in `Part 4 - Azure OpenAI/GenAiLab/` (if it exists)
  - **After reconciling differences**, replace/create the contents of `Part 4 - Azure OpenAI/GenAiLab/` with your working directory code as the snapshot

- **For Part 5**, continue from your Part 4 working directory and apply the README instructions.
  - **At the end of Part 5**, compare your working directory with the code snapshot in `Part 5 - Products Page/GenAiLab/`
  - **After reconciling differences**, replace the contents of `Part 5 - Products Page/GenAiLab/` with your working directory code as the updated snapshot

- **For Part 6**, test the provided complete code snapshot in `Part 6 - Deployment/GenAiLab/` which includes the `.azure` directory and deployment configuration.
  - **Deployment Testing Decision**: Choose configuration-only testing (recommended) or full Azure deployment test
  - **Configuration-Only**: Verify `azure.yaml` exists, build succeeds in Release mode, document that deployment configuration is correct
  - **Full Deployment (Optional)**: Use interactive `azd up` (not --no-prompt), test deployed app, run `azd down --purge --force` for cleanup (20-25 minutes)
  - **At the end of Part 6**, compare with your Part 5 working directory and verify deployment configuration is added (primarily `WithExternalHttpEndpoints()` in AppHost.cs)
  - **After reconciling differences**, replace the contents of `Part 6 - Deployment/GenAiLab/` with the updated snapshot

- **For Parts 7-9**, follow similar snapshot workflow:
  - Create MCP servers using `dotnet new mcpserver` commands as specified
  - Compare with existing snapshots at the end of each part
  - Update snapshots after reconciling differences

### File Management Rules

- **NEVER copy the `.vs` folder** (IDE state) between parts or into working directories. If a `.vs` directory appears, delete it immediately:
  ```powershell
  Remove-Item -Recurse -Force .\test-workspace\GenAiLab\.vs -ErrorAction SilentlyContinue
  ```
- **Preserve configuration files**: When copying code between parts, ensure that all files in `GenAiLab.Web/Properties/` and `GenAiLab.AppHost/Properties/` (including `launchSettings.json`) are copied exactly to prevent port drift and configuration mismatches.
- **Exclude build artifacts**: Prefer excluding transient build artifacts (`bin/`, `obj/`, `TestResults/`) when copying unless explicitly needed for testing.
- **Clean builds**: At the end of testing each code part, run a clean build to verify success:
  ```powershell
  dotnet clean
  dotnet build
  ```

### Part-by-Part Instructions

1. **Part 1 - Setup**: Follow the README.md in `Part 1 - Setup/` for prerequisites and environment setup.

2. **Part 2 - Project Creation**: 
   - Follow the README.md instructions to create a new project using `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant` in `test-workspace/GenAiLab/`
   - **Immediately**: Copy the generated project to `src/start/` as the initial snapshot
   - Verify it builds and runs with basic chat functionality
   - **End of Part 2**: Compare with `Part 2 - Project Creation/GenAiLab/`, document differences, then replace that snapshot with your working code

3. **Part 3 - Template Exploration**: Follow the README.md in `Part 3 - Template Exploration/` to understand the generated template structure from Part 2.

4. **Part 4 - Azure OpenAI**: 
   - Continue from your Part 2 working directory and follow the README.md instructions to convert from GitHub Models to Azure OpenAI
   - **CRITICAL**: If the README instructs you to create any new projects or components, use the `dotnet new` commands exactly as specified - never create `.csproj` files manually
   - **End of Part 4**: Compare with `Part 4 - Azure OpenAI/GenAiLab/` (if exists), document differences, then replace/create that snapshot with your working code

5. **Part 5 - Products Page**: 
   - Continue from your Part 4 working directory and follow the README.md instructions to implement the products page
   - **CRITICAL**: Follow all `dotnet new` commands in the README exactly - never create `.csproj` files manually
   - **End of Part 5**: 
     - Compare with `Part 5 - Products Page/GenAiLab/`, document differences, then replace that snapshot
     - **Then compare with `src/complete/`**, document any unexpected differences
     - Replace `src/complete/` with your working code as the master reference snapshot

6. **Part 6 - Deployment**: Test the provided complete code snapshot in `Part 6 - Deployment/GenAiLab/`. Follow the README.md for deployment instructions. **For testing purposes**: Verify the `azure.yaml` configuration and `.azure/` directory structure exist, but skip actual `azd provision` and `azd deploy` commands unless you intend to deploy to Azure (followed by immediate cleanup with `azd down --purge --force`).

7. **Part 7 - MCP Server Basics**: Follow the README.md instructions to create MyMcpServer using `dotnet new mcpserver -n MyMcpServer`. **CRITICAL**: You must use the `dotnet new` command - never create the `.csproj` manually. Test MCP server functionality and VS Code integration. Compare with the reference code snapshot in `Part 7 - MCP Server Basics/MyMcpServer/`.

8. **Part 8 - Enhanced MCP Server**: Follow the README.md instructions to create ContosoOrdersMcpServer. **CRITICAL**: If the README instructs you to create a new MCP server project, use `dotnet new mcpserver -n ContosoOrdersMcpServer` exactly as specified - never create the `.csproj` manually. Test enhanced business tools and multi-server configuration. Compare with the reference code snapshot in `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/`.

9. **Part 9 - MCP Publishing**: Follow the README.md in `Part 9 - MCP Publishing/` for publishing workflow documentation. Skip actual publishing during testing.

### Critical Testing Notes

**Part 1**: Environment setup and prerequisites verification.

**Part 2**: Create a new project using `dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant` as instructed in the README. Test that the generated project builds and runs with basic chat functionality. At end of part, compare with `Part 2 - Project Creation/GenAiLab/`, document differences, then replace that snapshot. **CRITICAL**: You must create the project using `dotnet new` - never copy existing snapshots or create `.csproj` files manually.

**Part 3**: Verify template exploration documentation clarity using the project you created in Part 2.

**Part 4**: Continue from your Part 2 working directory and apply Azure OpenAI conversion instructions. At end of part, compare with `Part 4 - Azure OpenAI/GenAiLab/`, document differences, then replace/create that snapshot. **CRITICAL**: If the README instructions involve creating any new projects or components, you must use the `dotnet new` commands exactly as specified - never create `.csproj` files manually.

**Part 5**: Continue from Part 4 working directory and apply Products page implementation instructions. At end of part, compare with `Part 5 - Products Page/GenAiLab/`, document differences, then replace that snapshot. **CRITICAL**: Follow all `dotnet new` commands in the README exactly - never create `.csproj` files manually.

**Part 6**: Test the complete code snapshot which includes all functionality from Parts 2-5 plus deployment configuration. Verify the `.azure/` directory and `azure.yaml` exist. 

**Deployment Testing Options**:
- **Recommended (Configuration-Only)**: Verify `azure.yaml` exists, build succeeds in Release mode, document deployment configuration is present
- **Full Deployment Test (Optional)**: If testing actual Azure deployment:
  - **MUST use interactive mode** - `azd up --no-prompt` fails with secure parameters
  - **Time requirement**: 7-12 minutes for deployment, 20-25 minutes for cleanup
  - **Cost consideration**: Incurs Azure charges during test
  - **Cleanup mandatory**: Always run `azd down --purge --force` immediately after testing
  - **Directory context**: Use `Push-Location` with absolute path to Part 6 directory
  - **Authentication**: Use `azd auth login --use-device-code` before deployment
  - **Environment name**: Use unique name (e.g., `workshoptest-[timestamp]`) to avoid conflicts

**Part 7**: Create MyMcpServer using `dotnet new mcpserver -n MyMcpServer` as instructed in the README. **CRITICAL**: You must use the `dotnet new` command - never create the `.csproj` manually. 

**Important Considerations**:
- **SDK Version**: Requires .NET 10.0 SDK (preview 6 or higher)
- **Template Output**: Current template may generate code without namespaces - this differs from reference snapshot
- **Namespace Pattern**: Reference snapshot uses `namespace MyMcpServer.Tools;` - update generated code to match if needed
- **Two Tool Classes**: Keep both RandomNumberTools (from template) and add WeatherTools (per instructions)
- **Verification Steps**:
  1. Build succeeds: `dotnet build`
  2. Server starts: `dotnet run` shows "Server (stream) (MyMcpServer) transport reading messages"
  3. Server responds to termination: Ctrl+C or process kill stops cleanly
- **VS Code Integration**: Optional - can test with .vscode/mcp.json configuration and GitHub Copilot
- **Time Estimate**: 20-30 minutes including template generation, code additions, and testing

Compare with reference code snapshot in `Part 7 - MCP Server Basics/MyMcpServer/`. Document any template differences (especially namespace usage).

**Part 8**: Test the existing ContosoOrdersMcpServer snapshot (Part 8 is exploration-focused, not template creation). 

**Important Considerations**:
- **Approach**: Build and run existing snapshot rather than creating new project
- **Target Framework**: .NET 8.0 (different from Part 7's .NET 9/10)
- **Expected Warnings**: 3 CS1998 warnings about async methods without await - these are acceptable
  - ContosoOrdersTools.cs lines 13, 61, 93
  - Methods return synchronous data but use async signature for API consistency
- **Business Tools**: Three tools simulating e-commerce system (GetOrderDetails, SearchOrdersByCustomer, GetProductInventory)
- **Verification Steps**:
  1. Build succeeds with expected warnings: `dotnet build`
  2. Server starts with business context: `dotnet run` shows ContosoOrdersMcpServer transport messages
  3. Application logs show proper lifecycle messages
- **Documentation Review**: Focus on business integration patterns, security, error handling, performance
- **VS Code Integration**: Optional - can configure multi-server setup with both MyMcpServer and ContosoOrdersMcpServer
- **Time Estimate**: 10-15 minutes for build, run test, and documentation review

Compare with reference code snapshot in `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/`.

**Part 9**: Review MCP publishing documentation for clarity and completeness (skip actual publishing).

**Important Considerations**:
- **Scope**: Documentation-only part - no code creation or actual NuGet publishing
- **Publishing Strategies**: Four main approaches covered (NuGet, source code, containers, enterprise internal)
- **Documentation Review Focus**:
  - NuGet package metadata configuration (PackageId, Version, Authors, etc.)
  - .mcp/server.json configuration for package managers
  - Professional README and documentation templates
  - Versioning and update strategies (semantic versioning)
  - Security best practices (input validation, error handling)
  - Distribution channels (NuGet.org, private feeds, GitHub packages, Azure Artifacts)
- **Verification**: Read through README to ensure publishing guidance is clear and actionable
- **No Actions Required**: Do not actually publish packages during testing
- **Time Estimate**: 10-15 minutes for thorough documentation review

Document any unclear sections or missing information for documentation improvement recommendations.

### Template Creation Rule (CRITICAL)

Throughout all testing, if any README instructs you to create a new project:
- ✅ **ALWAYS use the `dotnet new` command** exactly as shown in the documentation
- ❌ **NEVER create `.csproj` files manually** or directly
- ❌ **NEVER copy and modify `.csproj` files** from other projects
- ❌ **NEVER use the provided code snapshots as your working directory** for Parts 2, 7, or 8 - they are reference implementations only

This workshop specifically tests the template generation workflow. Manual project file creation or copying snapshots defeats the purpose of the workshop and will result in invalid test results.

## Evaluation

1. At the completion of each part, verify that your generated code works correctly and the README documentation is clear and actionable.
2. **Compare your working code with existing snapshots** at the end of each part, documenting any differences found.
3. **Reconcile differences**: Determine if differences are due to:
   - Template updates (expected - update snapshots)
   - Documentation issues (document for improvement)
   - Errors in following instructions (fix and retry)
   - Missing steps in documentation (document for improvement)
4. **Update snapshots** after reconciliation:
   - End of Part 2: Update `Part 2 - Project Creation/GenAiLab/`
   - End of Part 4: Update `Part 4 - Azure OpenAI/GenAiLab/`
   - End of Part 5: Update `Part 5 - Products Page/GenAiLab/`
   - End of Part 6: Update `Part 6 - Deployment/GenAiLab/`
   - End of Part 7: Update `Part 7 - MCP Server Basics/MyMcpServer/`
   - End of Part 8: Update `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/`
5. For code parts (2, 6, 7, 8), ensure the projects build successfully and core functionality works as expected.
6. For documentation parts (1, 3, 9), verify the instructions are comprehensive and easy to follow.
7. Document any issues or challenges encountered, including any clarifications needed for the instructions, missing prerequisites, or unclear steps.
8. Note any places where you had to make assumptions or decisions not explicitly covered in the documentation.
9. Test the progression between parts to ensure the workshop flows logically from one part to the next.

## Known Issues and Workarounds

### MCP Server Template Evolution (Parts 7-8)

**Issue**: .NET 10 RC `dotnet new mcpserver` template generates code without namespaces, but reference snapshots use namespace pattern
- **Template Generated** (v10 RC1): No namespace declarations in tool files
- **Reference Snapshot**: Uses `namespace MyMcpServer.Tools;` pattern
- **Impact**: Generated code may not match reference snapshots exactly
- **Resolution**: Both patterns are valid - template evolution is expected between preview/RC versions
- **Recommendation**: Update generated code to match reference snapshot pattern for consistency, or update snapshots to match current template output

**Issue**: MCP servers target different .NET framework versions
- **Part 7 MyMcpServer**: Template generates for .NET 9.0 (using .NET 10 SDK)
- **Part 8 ContosoOrdersMcpServer**: Targets .NET 8.0
- **Impact**: Build requires appropriate SDK version available
- **Resolution**: Verify SDK version with `dotnet --list-sdks` before building

**Issue**: Expected async warnings in MCP tool implementations
- **Warning**: CS1998 - "This async method lacks 'await' operators and will run synchronously"
- **Location**: Tool methods that return synchronous data but use async signature
- **Impact**: Build succeeds with warnings (not errors)
- **Resolution**: These warnings are expected and acceptable - methods use async signature for API consistency even when implementation is synchronous

### Azure Deployment with azd (Part 6)

**Issue**: `azd up --no-prompt` panics when encountering secure parameters (OpenAI connection string)
- **Symptom**: Error message about prompting in non-interactive mode
- **Root Cause**: azd cannot prompt for secure bicep parameters when --no-prompt is specified
- **Workaround**: Use interactive mode (`azd up` without --no-prompt) and provide values when prompted
- **Alternative**: Use `azd env set` before `azd up`, but interactive mode is more reliable

**Issue**: Terminal commands lose directory context between invocations
- **Symptom**: `azd` commands run in repo root instead of project directory
- **Workaround**: Use `Push-Location` with absolute path before azd commands, `Pop-Location` after
- **Example**: `Push-Location "d:\Users\Jon\Documents\GitHub\ai-workshop\Part 6 - Deployment\GenAiLab"; azd up; Pop-Location`

**Issue**: Azure resource cleanup takes significant time
- **Duration**: 20-25 minutes for `azd down --purge --force` to complete
- **Impact**: Must factor cleanup time into testing schedule
- **Recommendation**: Consider configuration-only testing to avoid cleanup overhead

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
cd "Part 2 - Project Creation/GenAiLab"
dotnet build --verbosity minimal

cd "Part 6 - Deployment/GenAiLab"
dotnet build --verbosity minimal

cd "Part 7 - MCP Server Basics/MyMcpServer"
dotnet build --verbosity minimal

cd "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer"
dotnet build --verbosity minimal

# Clean build if issues occur
dotnet clean
dotnet build

# At the end of each part's testing, verify clean build
dotnet clean
dotnet build --configuration Release
```

### Project Creation and Snapshot Management
```powershell
# Part 2: Create AI Web Chat project using dotnet new
New-Item -ItemType Directory -Path "test-workspace" -Force
cd test-workspace
dotnet new aichatweb -n GenAiLab --provider githubmodels --aspire --vector-store qdrant

# At end of Part 2, compare and update snapshot
code --diff "GenAiLab" "../Part 2 - Project Creation/GenAiLab"
# After reviewing differences:
Copy-Item -Path "GenAiLab/*" -Destination "../Part 2 - Project Creation/GenAiLab/" -Recurse -Force

# Part 7: Create MCP Server using dotnet new
dotnet new mcpserver -n MyMcpServer
# Compare with snapshot, then update
Copy-Item -Path "MyMcpServer/*" -Destination "../Part 7 - MCP Server Basics/MyMcpServer/" -Recurse -Force

# Part 8: Create Business MCP Server using dotnet new
dotnet new mcpserver -n ContosoOrdersMcpServer
# Compare with snapshot, then update
Copy-Item -Path "ContosoOrdersMcpServer/*" -Destination "../Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/" -Recurse -Force

# Always remove IDE state folder and build artifacts before copying
Remove-Item -Recurse -Force .vs,bin,obj,TestResults -ErrorAction SilentlyContinue
```

### Azure Deployment Testing (Part 6)
```powershell
# IMPORTANT: Azure deployment is time-intensive and incurs costs
# Provisioning: ~5-10 minutes, Cleanup: ~20-25 minutes
# Recommended: Test code changes only, skip actual deployment unless specifically required

# If performing actual deployment test:
Push-Location "d:\Users\Jon\Documents\GitHub\ai-workshop\Part 6 - Deployment\GenAiLab"

# Authenticate with Azure (if not already done)
azd auth login --use-device-code

# Initialize environment (interactive mode recommended due to azd --no-prompt limitations)
azd init
# Choose: Use code in current directory
# Environment name: [unique-name] (e.g., workshoptest-[timestamp])

# CRITICAL: azd up in interactive mode (--no-prompt has issues with secure parameters)
# You will be prompted for:
# - Subscription
# - Location (use: westus2 or value from WORKSHOP_AZURE_LOCATION)
# - OpenAI connection string (format: Endpoint=https://...;Key=...)
azd up
# Estimated time: 7-12 minutes total

# Test the deployed application
# URL will be shown in azd up output

# MANDATORY CLEANUP: Always run after deployment testing
azd down --purge --force
# Estimated time: 20-25 minutes
# Verify in Azure Portal that resource group is deleted

Pop-Location
```

**Alternative: Configuration-Only Testing (Recommended)**
```powershell
# Verify deployment configuration without actual Azure deployment
Push-Location "d:\Users\Jon\Documents\GitHub\ai-workshop\Part 6 - Deployment\GenAiLab"

# Verify azure.yaml exists and is properly configured
Test-Path azure.yaml
Get-Content azure.yaml

# Verify .azure directory structure (if using existing snapshot)
Test-Path .azure

# Build in Release mode (same as azd uses)
dotnet build --configuration Release

# Document that deployment configuration is present and builds successfully
Pop-Location
```
````
