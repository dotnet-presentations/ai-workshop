````prompt
---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience using the per-unit directory structure.
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in each Part directory (Parts 1-11), testing both the provided code snapshots and README documentation. For the MCP extensions (parts 6-8), you will test the MCP server creation and integration but skip the publishing sections. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

### Test Report Requirements

Create a test report markdown file named `workshop-test-report-[DATE].md` in the `docs/testing/` directory with the following structure:

```markdown
# Workshop Test Report - [Date]

## Test Environment
- .NET SDK Version(s): [version info]
- OS: [operating system]
- Docker Version: [if applicable]
- Test Scope: [Parts 1-5 only / Parts 1-11 full test]

## Part-by-Part Results

### Part 1 - Setup
- Status: ✅ Pass / ❌ Fail / ⚠️ Issues
- Notes: [observations, issues, time taken]
- Documentation Clarity: [rating and comments]

### Part 2 - Build Chat App
- Status: ✅ Pass / ❌ Fail / ⚠️ Issues
- Notes: [observations, issues, time taken]
- Documentation Clarity: [rating and comments]
- **Snapshot Comparison**: [differences found between generated code and existing snapshot]
- **Snapshot Updated**: ✅ Yes / ❌ No - [details]

[Repeat snapshot comparison section for Parts 3, 6, 7, 9, 10, 11...]

## Snapshot Reconciliation Summary
- **Part 2 - Build Chat App/ChatApp/**: [differences found and resolved]
- **Part 3 - Add RAG/RagChatApp/**: [differences found and resolved]
- **Part 6 - MCP Server Basics/MyMcpServer/**: [differences found and resolved]
- **Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/**: [differences found and resolved]
- **Part 11 - Deployment/GenAiLab/**: [differences found and resolved]

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
3. To validate the MCP server creation and integration workflow (Parts 6-8).

Each part contains either a complete working code snapshot or comprehensive README documentation for setup/exploration tasks.

## Workshop Structure

The workshop is organized into 11 parts using a per-unit directory structure:

**Core AI application development (Parts 1-5):**
- Part 1 - Setup: Prerequisites and setup
- Part 2 - Build Chat App: Build a console chat app by hand with `dotnet new console`, including structured output
- Part 3 - Add RAG: Add retrieval-augmented generation with embeddings and semantic search
- Part 4 - AI Web Chat Template: Scaffold and inspect the generated template architecture with `dotnet new aichatweb`
- Part 5 - Providers and Fallbacks: Configure provider options and fallback paths

**Model Context Protocol Extension (Parts 6-8):**
- Part 6 - MCP Server Basics: Create MyMcpServer using `dotnet new mcpserver` with both template RandomNumberTools and custom WeatherTools
- Part 7 - Enhanced MCP Server *(optional / bonus)*: Create Business MCP Server with ContosoOrders tools
- Part 8 - MCP Publishing *(optional / bonus)*: Publish and Share MCP Servers (README only)

**Agents, capstone, and deployment (Parts 9-11):**
- Part 9 - Agent Framework Essentials: Agent basics with the `AgentApp` sample
- Part 10 - Adding AI to an Existing App: Capstone `StoreApp` sample
- Part 11 - Deployment: Deploy to Azure (includes complete project with .azure directory)

**Code Snapshots Structure:**
- `Part 2 - Build Chat App/ChatApp/` - Hand-built console chat app
- `Part 3 - Add RAG/RagChatApp/` - Console chat app with retrieval added by hand
- `Part 6 - MCP Server Basics/MyMcpServer/` - Generic MCP server with RandomNumberTools and WeatherTools
- `Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/` - Business MCP server implementation
- `Part 9 - Agent Framework Basics/AgentApp/` - Agent sample
- `Part 10 - Adding AI to an Existing App/StoreApp/` - Existing app with AI added
- `Part 11 - Deployment/GenAiLab/` - Complete project with deployment configuration

Parts 1, 4, 5, and 8 contain only README files as they focus on setup, exploration, comparison, or publishing rather than code creation.

## Environment Variables and Credential Management

Before starting the workshop, the following environment variables should be checked and configured if needed:

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

1. **Azure OpenAI Credentials**: Use the `WORKSHOP_AZURE_OPENAI_ENDPOINT` and `WORKSHOP_AZURE_OPENAI_KEY` environment variables.

2. **Template Parameter Critical Requirement**: Always use `--vector-store qdrant` when generating AI Web Chat templates. Missing this parameter causes templates to use SQLite instead of Qdrant, leading to documentation misalignment.

3. **JavaScript File Dependencies**: The AI Web Chat template includes essential JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) that provide auto-resize textarea and auto-scroll functionality. These files must be preserved in all code snapshots.

4. **Complete Implementation Requirements**: Part 11 must contain the complete AI Web Chat application plus deployment configuration.

5. **MCP Prerequisites**: Parts 6-8 require .NET 10 SDK and Visual Studio Code with GitHub Copilot extension. Part 6 uses `MyMcpServer` project that includes both template-generated RandomNumberTools and custom WeatherTools.

6. **MCP VS Code Integration Testing**: For Parts 6-7, VS Code integration testing involves:
   - Verifying the MCP server starts correctly when run via `dotnet run`
   - Checking that the server responds to stdio communication protocol
   - Confirming proper shutdown with Ctrl+C
   - (Optional) Testing GitHub Copilot integration by configuring the server in VS Code settings and verifying tool availability in chat

8. **Per-Unit Structure**: Each part is self-contained with its own directory. Code parts (2, 3, 6, 7, 9, 10, 11) include working project snapshots, while documentation parts (1, 4, 5, 8) contain comprehensive README files.

## Key Testing Requirements

- **CRITICAL: All projects must be created using `dotnet new` commands** as specified in the README instructions. Never create `.csproj` files directly or manually. The workshop tests the template generation workflow, not manual project creation.
- Always use `--vector-store qdrant` when generating AI Web Chat templates
- Ensure JavaScript files (ChatInput.razor.js, ChatMessageList.razor.js) are preserved in all snapshots
- Verify Part 11 has a complete implementation, not just documentation
- Test MCP server functionality in Parts 6-7 but skip publishing in Part 8
- Use environment variables for credentials and test with actual services when possible

## Procedure

**Prerequisites: Run the credential setup script before starting the workshop to configure environment variables.**

The workshop uses a per-unit directory structure where each part is self-contained. Follow these guidelines for working with code:

### Working Directory Policy and Snapshot Workflow

- **Part 1** contains no code changes and does not require a working directory.

- **For Part 2**, follow the README instructions to create a new project using `dotnet new console -n ChatApp` in a test working directory (e.g., `test-workspace/ChatApp/`). **CRITICAL**: You must use the `dotnet new` command to create the project - never copy snapshots or create `.csproj` files manually.
  - **At the end of Part 2**, compare your working directory with the code snapshot in `Part 2 - Build Chat App/ChatApp/`
  - **After reconciling differences**, replace the contents of `Part 2 - Build Chat App/ChatApp/` with your working directory code as the updated snapshot

- **For Part 3**, continue from your Part 2 working directory and add retrieval by hand as described in the README.
  - **At the end of Part 3**, compare your working directory with the code snapshot in `Part 3 - Add RAG/RagChatApp/`
  - **After reconciling differences**, replace the contents of `Part 3 - Add RAG/RagChatApp/` with your working directory code as the updated snapshot

- **For Part 4**, scaffold the template app with `dotnet new aichatweb -n GenAiLab --provider azureopenai --aspire --vector-store qdrant` in a separate working directory and follow the README instructions. **IMPORTANT**: Follow the README instructions exactly - never create `.csproj` files manually.
  - **At the end of Part 4**, compare the generated app with the code snapshot in `Part 11 - Deployment/GenAiLab/` (which is the same app plus deployment configuration)

- **For Part 5**, follow the README documentation only (provider comparison, no new project).

- **For Part 11**, test the provided complete code snapshot in `Part 11 - Deployment/GenAiLab/` which includes the `.azure` directory and deployment configuration.
  - **Deployment Testing Decision**: Choose configuration-only testing (recommended) or full Azure deployment test
  - **Configuration-Only**: Verify `azure.yaml` exists, build succeeds in Release mode, document that deployment configuration is correct
  - **Full Deployment (Optional)**: Use interactive `azd up` (not --no-prompt), test deployed app, run `azd down --purge --force` for cleanup (20-25 minutes)
  - **At the end of Part 11**, compare with your Part 5 working directory and verify deployment configuration is added (primarily `WithExternalHttpEndpoints()` in AppHost.cs)
  - **After reconciling differences**, replace the contents of `Part 11 - Deployment/GenAiLab/` with the updated snapshot

- **For Parts 6-8**, follow similar snapshot workflow:
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

2. **Part 2 - Build Chat App**:
   - Follow the README.md instructions to create a new project using `dotnet new console -n ChatApp` in `test-workspace/ChatApp/`
   - Verify it builds and runs with basic chat, streaming, and structured output functionality
   - **End of Part 2**: Compare with `Part 2 - Build Chat App/ChatApp/`, document differences, then replace that snapshot with your working code

3. **Part 3 - Add RAG**: Follow the README.md in `Part 3 - Add RAG/` to build retrieval-augmented generation on top of Part 2.
   - **End of Part 3**: Compare with `Part 3 - Add RAG/RagChatApp/`, document differences, then replace that snapshot with your working code

4. **Part 4 - AI Web Chat Template**:
   - Follow the README.md instructions to scaffold and explore the template app with `dotnet new aichatweb`
   - **CRITICAL**: If the README instructs you to create any new projects or components, use the `dotnet new` commands exactly as specified - never create `.csproj` files manually
   - **End of Part 4**: Compare the generated app with `Part 11 - Deployment/GenAiLab/` and document differences

5. **Part 5 - Providers and Fallbacks**:
   - Follow the README.md instructions to review provider options and fallback strategy (documentation only)

6. **Part 11 - Deployment**: Test the provided complete code snapshot in `Part 11 - Deployment/GenAiLab/`. Follow the README.md for deployment instructions. **For testing purposes**: Verify the `azure.yaml` configuration and `.azure/` directory structure exist, but skip actual `azd provision` and `azd deploy` commands unless you intend to deploy to Azure (followed by immediate cleanup with `azd down --purge --force`).

7. **Part 6 - MCP Server Basics**: Follow the README.md instructions to create MyMcpServer using `dotnet new mcpserver -n MyMcpServer`. **CRITICAL**: You must use the `dotnet new` command - never create the `.csproj` manually. Test MCP server functionality and VS Code integration. Compare with the reference code snapshot in `Part 6 - MCP Server Basics/MyMcpServer/`.

8. **Part 7 - Enhanced MCP Server**: Follow the README.md instructions to create ContosoOrdersMcpServer. **CRITICAL**: If the README instructs you to create a new MCP server project, use `dotnet new mcpserver -n ContosoOrdersMcpServer` exactly as specified - never create the `.csproj` manually. Test enhanced business tools and multi-server configuration. Compare with the reference code snapshot in `Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/`.

9. **Part 8 - MCP Publishing**: Follow the README.md in `Part 8 - MCP Publishing/` for publishing workflow documentation. Skip actual publishing during testing.

### Critical Testing Notes

**Part 1**: Environment setup and prerequisites verification.

**Part 2**: Create a new project using `dotnet new console -n ChatApp` as instructed in the README. Test that the project builds and runs with chat, streaming, and structured output. At end of part, compare with `Part 2 - Build Chat App/ChatApp/`, document differences, then replace that snapshot. **CRITICAL**: You must create the project using `dotnet new` - never copy existing snapshots or create `.csproj` files manually.

**Part 3**: Add retrieval by hand on top of the Part 2 project. At end of part, compare with `Part 3 - Add RAG/RagChatApp/`, document differences, then replace that snapshot.

**Part 4**: Scaffold and explore the template app with `dotnet new aichatweb`. At end of part, compare with `Part 11 - Deployment/GenAiLab/` and document differences. **CRITICAL**: Use the `dotnet new` commands exactly as specified - never create `.csproj` files manually.

**Part 5**: Review the provider and fallback documentation (no new project).

**Part 11**: Test the complete code snapshot which includes all functionality from Parts 2-5 plus deployment configuration. Verify the `.azure/` directory and `azure.yaml` exist. 

**Deployment Testing Options**:
- **Recommended (Configuration-Only)**: Verify `azure.yaml` exists, build succeeds in Release mode, document deployment configuration is present
- **Full Deployment Test (Optional)**: If testing actual Azure deployment:
  - **MUST use interactive mode** - `azd up --no-prompt` fails with secure parameters
  - **Time requirement**: 7-12 minutes for deployment, 20-25 minutes for cleanup
  - **Cost consideration**: Incurs Azure charges during test
  - **Cleanup mandatory**: Always run `azd down --purge --force` immediately after testing
  - **Directory context**: Use `Push-Location` with absolute path to Part 11 directory
  - **Authentication**: Use `azd auth login --use-device-code` before deployment
  - **Environment name**: Use unique name (e.g., `workshoptest-[timestamp]`) to avoid conflicts

**Part 6**: Create MyMcpServer using `dotnet new mcpserver -n MyMcpServer` as instructed in the README. **CRITICAL**: You must use the `dotnet new` command - never create the `.csproj` manually. 

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

Compare with reference code snapshot in `Part 6 - MCP Server Basics/MyMcpServer/`. Document any template differences (especially namespace usage).

**Part 7**: Test the existing ContosoOrdersMcpServer snapshot (Part 7 is exploration-focused, not template creation). 

**Important Considerations**:
- **Approach**: Build and run existing snapshot rather than creating new project
- **Target Framework**: .NET 10.0 (aligned with Part 6)
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

Compare with reference code snapshot in `Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/`.

**Part 8**: Review MCP publishing documentation for clarity and completeness (skip actual publishing).

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
- ❌ **NEVER use the provided code snapshots as your working directory** for Parts 2, 3, 6, or 7 - they are reference implementations only

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
   - End of Part 2: Update `Part 2 - Build Chat App/ChatApp/`
   - End of Part 3: Update `Part 3 - Add RAG/RagChatApp/`
   - End of Part 6: Update `Part 6 - MCP Server Basics/MyMcpServer/`
   - End of Part 7: Update `Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/`
   - End of Part 11: Update `Part 11 - Deployment/GenAiLab/`
5. For code parts (2, 3, 6, 7, 9, 10, 11), ensure the projects build successfully and core functionality works as expected.
6. For documentation parts (1, 4, 5, 8), verify the instructions are comprehensive and easy to follow.
7. Document any issues or challenges encountered, including any clarifications needed for the instructions, missing prerequisites, or unclear steps.
8. Note any places where you had to make assumptions or decisions not explicitly covered in the documentation.
9. Test the progression between parts to ensure the workshop flows logically from one part to the next.

## Known Issues and Workarounds

### MCP Server Template Evolution (Parts 6-7)

**Issue**: .NET 10 RC `dotnet new mcpserver` template generates code without namespaces, but reference snapshots use namespace pattern
- **Template Generated** (v10 RC1): No namespace declarations in tool files
- **Reference Snapshot**: Uses `namespace MyMcpServer.Tools;` pattern
- **Impact**: Generated code may not match reference snapshots exactly
- **Resolution**: Both patterns are valid - template evolution is expected between preview/RC versions
- **Recommendation**: Update generated code to match reference snapshot pattern for consistency, or update snapshots to match current template output

**Issue**: MCP servers target different .NET framework versions
- **Part 6 MyMcpServer**: Targets .NET 10.0
- **Part 7 ContosoOrdersMcpServer**: Targets .NET 10.0
- **Impact**: Build requires appropriate SDK version available
- **Resolution**: Verify SDK version with `dotnet --list-sdks` before building

**Issue**: Expected async warnings in MCP tool implementations
- **Warning**: CS1998 - "This async method lacks 'await' operators and will run synchronously"
- **Location**: Tool methods that return synchronous data but use async signature
- **Impact**: Build succeeds with warnings (not errors)
- **Resolution**: These warnings are expected and acceptable - methods use async signature for API consistency even when implementation is synchronous

### Azure Deployment with azd (Part 11)

**Issue**: `azd up --no-prompt` panics when encountering secure parameters (OpenAI connection string)
- **Symptom**: Error message about prompting in non-interactive mode
- **Root Cause**: azd cannot prompt for secure bicep parameters when --no-prompt is specified
- **Workaround**: Use interactive mode (`azd up` without --no-prompt) and provide values when prompted
- **Alternative**: Use `azd env set` before `azd up`, but interactive mode is more reliable

**Issue**: Terminal commands lose directory context between invocations
- **Symptom**: `azd` commands run in repo root instead of project directory
- **Workaround**: Use `Push-Location` with absolute path before azd commands, `Pop-Location` after
- **Example**: `Push-Location "d:\Users\Jon\Documents\GitHub\ai-workshop\Part 11 - Deployment\GenAiLab"; azd up; Pop-Location`

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
Write-Host "Azure Endpoint: $($env:WORKSHOP_AZURE_OPENAI_ENDPOINT -ne $null ? 'Set' : 'Not Set')"
```

### Build Testing
```powershell
# Test each code snapshot builds successfully
cd "Part 2 - Build Chat App/ChatApp"
dotnet build --verbosity minimal

cd "Part 11 - Deployment/GenAiLab"
dotnet build --verbosity minimal

cd "Part 6 - MCP Server Basics/MyMcpServer"
dotnet build --verbosity minimal

cd "Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer"
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
# Part 2: Create the console chat app using dotnet new
New-Item -ItemType Directory -Path "test-workspace" -Force
cd test-workspace
dotnet new console -n ChatApp

# At end of Part 2, compare and update snapshot
code --diff "ChatApp" "../Part 2 - Build Chat App/ChatApp"
# After reviewing differences:
Copy-Item -Path "ChatApp/*" -Destination "../Part 2 - Build Chat App/ChatApp/" -Recurse -Force

# Part 6: Create MCP Server using dotnet new
dotnet new mcpserver -n MyMcpServer
# Compare with snapshot, then update
Copy-Item -Path "MyMcpServer/*" -Destination "../Part 6 - MCP Server Basics/MyMcpServer/" -Recurse -Force

# Part 7: Create Business MCP Server using dotnet new
dotnet new mcpserver -n ContosoOrdersMcpServer
# Compare with snapshot, then update
Copy-Item -Path "ContosoOrdersMcpServer/*" -Destination "../Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/" -Recurse -Force

# Always remove IDE state folder and build artifacts before copying
Remove-Item -Recurse -Force .vs,bin,obj,TestResults -ErrorAction SilentlyContinue
```

### Azure Deployment Testing (Part 11)
```powershell
# IMPORTANT: Azure deployment is time-intensive and incurs costs
# Provisioning: ~5-10 minutes, Cleanup: ~20-25 minutes
# Recommended: Test code changes only, skip actual deployment unless specifically required

# If performing actual deployment test:
Push-Location "d:\Users\Jon\Documents\GitHub\ai-workshop\Part 11 - Deployment\GenAiLab"

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
Push-Location "d:\Users\Jon\Documents\GitHub\ai-workshop\Part 11 - Deployment\GenAiLab"

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
