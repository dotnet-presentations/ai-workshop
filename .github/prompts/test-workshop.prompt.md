---
mode: agent
description: Testing procedure for workshop completion. This includes verifying code correctness, documentation clarity, and overall user experience using the new per-unit directory structure.
---

You are an intelligent developer who is completing this workshop. You will step through the tasks outlined in each Part directory (Parts 1-9), testing both the provided code snapshots and README documentation. For the MCP extensions (parts 7-9), you will test the MCP server creation and integration but skip the publishing sections. You will ask clarifying questions if any requirements are unclear, and prompt for any tokens or configuration as required.

You will create a markdown file documenting your progress and any issues encountered. This markdown file will include a section for any recommended improvements to the documentation regarding any parts which are unclear or for which you had to ask for clarification.

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

## Important Notes for Testing

1. **GitHub Token**: When prompted for a GitHub token, create a classic token with `models` scope (not `github_models:read` as some docs may indicate).

2. **Azure AI Preferred**: Azure AI setup is significantly simpler than GitHub Models. If Azure AI credentials are available, use them instead of GitHub Models for smoother testing experience.

4. **Code Inconsistencies**: Some documentation may show code that doesn't match the current template. Focus on what actually gets generated and works, not exact code matches in documentation.

6. **MCP Prerequisites**: Parts 7-9 require .NET 10 SDK preview and Visual Studio Code with GitHub Copilot extension. MCP servers use the new `mcpserver` project template and are created as separate projects.

7. **Per-Unit Structure**: Each part is now self-contained with its own directory. Code parts (2, 6, 7, 8) include working project snapshots, while documentation parts (1, 3, 9) contain comprehensive README files.

8. **Build Issues**: Document any build errors and resolution steps during testing.

9. **MCP Server Testing**: Parts 7-9 focus on creating and integrating MCP servers. Test MCP server creation, tool functionality, and VS Code integration but skip publishing steps.

10. **Deployment Validation**: If possible, test actual Azure deployment using the Part 6 code snapshot. Azure deployment is remarkably smooth (~6 minutes total).

## Testing Discoveries and Improvements

Based on comprehensive testing:

### What Works Excellently
- **Azure Deployment**: Remarkably smooth (~6 minutes total) via Azure Developer CLI
- **Azure AI Integration**: Much simpler than GitHub Models setup
- **Aspire Experience**: Excellent development and deployment experience
- **Educational Content**: AI concepts and implementation patterns are solid
- **MCP Integration**: .NET 10 MCP template provides smooth server creation experience
- **VS Code Integration**: MCP servers integrate well with GitHub Copilot in VS Code

### Recommended Improvements
1. **Prioritize Azure AI**: Simpler setup than GitHub Models
2. **Realistic Time Estimates**: Update lab timing expectations based on testing
3. **Troubleshooting Guide**: Include common build issue solutions
4. **MCP Documentation**: Ensure clear prerequisites for .NET 10 preview and VS Code setup

### Updated Testing Guidelines
- Use Azure AI credentials when available (simpler than GitHub Models)
- Document all build errors and resolution steps meticulously
- Test actual deployment when possible for complete validation
- Focus on educational value and learning objectives
- Validate that each part flows logically to the next

### Post-Testing Improvement Process
After completing the workshop testing, follow these phases for repository improvement:

#### Phase 1: Code Validation & Documentation Updates
1. **Test Each Part Independently**: Validate each code snapshot builds and functions correctly
2. **Update Testing Documentation**: Add testing insights and discoveries to this prompt file
3. **Update Lab Documentation**: Fix inconsistencies, add realistic time estimates, include troubleshooting
4. **MCP Validation**: Ensure MCP server projects build correctly and integrate with VS Code

#### Phase 2: Workshop Flow Validation
5. **Test Complete Workshop Flow**: Validate progression through all 9 parts
6. **Cross-Reference Validation**: Ensure all links between parts work correctly
7. **Prerequisites Verification**: Confirm all required tools and SDKs are properly documented
8. **Performance Testing**: Validate time estimates and complexity assessments

#### Repository State Verification
- Test: Each part's code snapshots build successfully and function as expected
- Validate: Documentation clarity and cross-references between parts
- Ensure: MCP server projects integrate properly with VS Code and GitHub Copilot
- Verify: Complete workshop flow from Part 1 through Part 9 works smoothly

## Procedure

The workshop now uses a per-unit directory structure where each part is self-contained:

1. **Part 1 - Setup**: Follow the README.md in `Part 1 - Setup/` for prerequisites and environment setup.

2. **Part 2 - Project Creation**: Use the provided code snapshot in `Part 2 - Project Creation/GenAiLab/` as your starting point. This contains a fresh AI Web Chat template created with `dotnet new aichatweb`.

3. **Part 3 - Template Exploration**: Follow the README.md in `Part 3 - Template Exploration/` to understand the generated template structure.

4. **Part 4 - Azure OpenAI**: Follow the README.md instructions to convert from GitHub Models to Azure OpenAI, building upon the Part 2 code snapshot.

5. **Part 5 - Products Page**: Follow the README.md instructions to implement the products page with vector search functionality.

6. **Part 6 - Deployment**: Use the provided complete code snapshot in `Part 6 - Deployment/GenAiLab/` which includes the `.azure` directory and deployment configuration. Follow the README.md for deployment instructions, stopping before actual Azure deployment for testing purposes.

7. **Part 7 - MCP Server Basics**: Use the provided Weather MCP Server in `Part 7 - MCP Server Basics/WeatherMcpServer/` or create a new one using `dotnet new mcpserver -n WeatherMcpServer`. Test MCP server functionality and VS Code integration.

8. **Part 8 - Enhanced MCP Server**: Use the provided Business MCP Server in `Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer/` or create a new one using `dotnet new mcpserver -n ContosoOrdersMcpServer`. Test enhanced business tools and multi-server configuration.

9. **Part 9 - MCP Publishing**: Follow the README.md in `Part 9 - MCP Publishing/` for publishing workflow documentation. Skip actual publishing during testing.

### Critical Testing Notes Based on Experience

**Part 1**: Environment setup and prerequisites - ensure all required SDKs and tools are installed.

**Part 2**: Test the provided starter code snapshot builds and runs correctly with basic chat functionality.

**Part 3**: Verify template exploration documentation is clear and helpful for understanding the codebase.

**Part 4**: Azure OpenAI integration - follow the README instructions carefully. Common issues:
- Build errors from static asset conflicts: `dotnet clean && dotnet build`
- Package restore problems: `dotnet restore && dotnet build`

**Part 5**: Products page implementation - test vector search functionality and integration.

**Part 6**: If possible, test actual Azure deployment using the complete code snapshot. Deployment is remarkably smooth (~6 minutes).

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

### Build Troubleshooting
```bash
# Clean build artifacts if conflicts occur
dotnet clean
dotnet build

# Package restore if needed
dotnet restore
dotnet build
```

### Git-Based Testing Validation
```bash
# Test each code snapshot builds successfully
cd "Part 2 - Project Creation/GenAiLab" && dotnet build --verbosity minimal
cd "Part 6 - Deployment/GenAiLab" && dotnet build --verbosity minimal
cd "Part 7 - MCP Server Basics/WeatherMcpServer" && dotnet build --verbosity minimal
cd "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer" && dotnet build --verbosity minimal

# Verify workshop structure
ls "Part */README.md"  # Should show all 9 README files
ls "Part */*/Program.cs"  # Should show main program files in code parts
```

### Testing Validation Commands
```bash
# Test starter project (Part 2)
cd "Part 2 - Project Creation/GenAiLab" && dotnet build --verbosity minimal

# Test complete project (Part 6)  
cd "Part 6 - Deployment/GenAiLab" && dotnet build --verbosity minimal

# Check workshop structure
ls "Part */README.md"  # Should show README files for all 9 parts

# Test MCP servers (Parts 7-8)
cd "Part 7 - MCP Server Basics/WeatherMcpServer" && dotnet build --verbosity minimal
cd "Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer" && dotnet build --verbosity minimal
```

### MCP Server Testing Commands (Parts 7-9)
```bash
# Create and test Weather MCP Server (Part 7)
dotnet new mcpserver -n WeatherMcpServer
cd WeatherMcpServer && dotnet build --verbosity minimal

# Create and test Business MCP Server (Part 8)  
dotnet new mcpserver -n ContosoOrdersMcpServer
cd ContosoOrdersMcpServer && dotnet build --verbosity minimal

# Verify MCP configuration files are created
ls *.json  # Should see mcp-server-config.json files
```

### Azure Deployment Testing (If Credentials Available)
```bash
# Navigate to complete project for deployment testing
cd "Part 6 - Deployment/GenAiLab"

# Update azd if needed
winget upgrade Microsoft.Azd

# Initialize for deployment (if azure.yaml doesn't exist)
azd init
# Follow prompts: "Use code in current directory" → "Confirm" → "Environment name"

# Provision and deploy (if testing real deployment)
azd provision  # ~4 minutes
azd deploy     # ~2 minutes

# View results
azd show

# Cleanup when done
azd down --purge --force
```