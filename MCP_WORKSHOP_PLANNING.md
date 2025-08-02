# MCP Workshop Implementation Plan

## Overview

This document outlines the implementation plan for adding Model Context Pro5. **Update Lab Structure section**:

   ```markdown
   ## Lab Structure 📁
   
   The repository is structured as follows:
   
   - 📖 `/lab`: Contains all the lab instructions and documentation
   - 🚀 `/src/Part 1 - Create AI Project`: Create AI Web Chat project (Part 1)
   - 🔍 `/src/Part 2 - Explore Template`: Template exploration (Part 2) - README only
   - ☁️ `/src/Part 3 - Azure OpenAI`: Azure OpenAI integration (Part 3)
   - 🛍️ `/src/Part 4 - Products Page`: Products page implementation (Part 4)
   - 🚀 `/src/Part 5 - Deploy Azure`: Azure deployment (Part 5) - README only
   - 🔧 `/src/Part 6 - MCP Server`: Basic MCP server (Part 6)
   - 🏢 `/src/Part 7 - MCP Business Tools`: Enhanced MCP server with business tools (Part 7)
   - 📦 `/src/Part 8 - MCP Publishing`: MCP publishing (Part 8) - README only
   ```

   **Note**: Some parts (2, 5, 8) contain only README files as they focus on exploration, deployment, or publishing rather than code creation.units to the existing .NET AI Workshop. The MCP units will complement the existing AI Web Chat application by teaching users how to build and publish custom MCP servers that can be consumed by AI agents like GitHub Copilot.

## Workshop Structure

### Complete Lab Modules (8 Parts Total)

1. **Part 1**: Create a Project with AI Web Chat Template
2. **Part 2**: Explore the Template Code  
3. **Part 3**: Convert from GitHub Models to Azure OpenAI
4. **Part 4**: Write a Products Page
5. **Part 5**: Deploy to Azure
6. **Part 6**: Create an MCP Server Project (NEW)
7. **Part 7**: Enhance MCP Server with Business Data (NEW)
8. **Part 8**: Publish and Consume MCP Server (NEW)

### Repository Structure - Per-Unit Approach

The workshop will use a per-unit directory structure where each part has its own folder containing the complete working state at that point in the lab.

**Source Directory Structure:**

```text
/src/
  Part 1 - Create AI Project/
    README.md (create project, test basic functionality)
    GenAiLab/ (fresh AI Web Chat template)
  Part 2 - Explore Template/
    README.md (exploration only - no code changes needed)
  Part 3 - Azure OpenAI/
    README.md (Azure OpenAI migration)
    GenAiLab/ (with Azure OpenAI configured)
  Part 4 - Products Page/
    README.md (Products page implementation)
    GenAiLab/ (with Products page added)
  Part 5 - Deploy Azure/
    README.md (deployment guide - uses Part 4 code)
  Part 6 - MCP Server/
    README.md (MCP introduction and basic server)
    SampleMcpServer/ (basic MCP template working)
  Part 7 - MCP Business Tools/
    README.md (enhanced MCP with business tools)
    SampleMcpServer/ (with ContosoOrders tools)
  Part 8 - MCP Publishing/
    README.md (publishing guide - uses Part 7 code)
```

**Benefits of This Approach:**

- **Error Recovery**: Students can start fresh from any unit if they encounter issues
- **Modular Teaching**: Instructors can teach units independently or skip units as needed
- **Clear Snapshots**: Each unit shows exactly what the code should look like at that stage
- **Self-Contained**: Each unit has its own README with specific objectives and setup
- **Technology Separation**: AI Web Chat (Parts 1-5) and MCP (Parts 6-8) are clearly separated
- **Workshop Flexibility**: Students can jump to any unit during time-constrained workshops

## New MCP Lab Modules

### Part 6: Create an MCP Server Project

- **Objective**: Introduce MCP concepts and create a basic MCP server
- **Content**:
  - Introduction to Model Context Protocol
  - Installing the MCP Server template
  - Creating a new MCP server project using `dotnet new mcpserver`
  - Understanding the project structure
  - Configuring VS Code with the MCP server
  - Testing the default `get_random_number` tool

### Part 7: Enhance MCP Server with Business Data

- **Objective**: Add custom tools with real business value
- **Content**:
  - Adding a `ContosoOrders` tool that reads business data
  - Implementing configuration via environment variables
  - Adding more sophisticated tools (e.g., order lookup, customer info)
  - Testing enhanced functionality
  - Understanding tool descriptions and parameters

### Part 8: Publish and Consume MCP Server (Optional Advanced)

- **Objective**: Package and distribute the MCP server
- **Content**:
  - Configuring `server.json` for NuGet packaging
  - Publishing to NuGet (or test environment)
  - Consuming published MCP server in VS Code
  - Best practices for production MCP servers

## Required Updates

### 1. Main README.md Updates

1. **Update Lab Overview section** to include MCP modules:

   ```markdown
   The lab consists of a series of hands-on exercises where you'll build:
   - 🤖 **AI-powered web application** using the new .NET AI project templates
   - 🔧 **Custom MCP server** that extends AI agents with business-specific tools
   ```

2. **Add MCP Architecture Diagram**:

   ```mermaid
   flowchart TD
       User([User]) <--> VSCode[VS Code + GitHub Copilot]
       VSCode <--> MCPServer[Custom MCP Server]
       MCPServer <--> BusinessData[(Business Data<br>ContosoOrders)]
       
       subgraph "Part 1-5: AI Web Chat"
           WebApp[Blazor Web App] <--> AIService[AI Chat Service]
           AIService <--> VectorDB[(Vector Database)]
       end
       
       subgraph "Part 6-8: MCP Server"
           MCPServer
           BusinessData
       end
   ```

3. **Update Lab Modules section** to include Parts 6-8

4. **Update Prerequisites** to include:
   - .NET 10.0 SDK (preview 6 or higher) for MCP development
   - GitHub Copilot extension for VS Code

5. **Update Lab Structure section**:

   ```markdown
   ## Lab Structure 📁
   
   The repository is structured as follows:
   
   - 📖 `/lab`: Contains all the lab instructions and documentation
   - 🚀 `/src/part1`: Create AI Web Chat project (Part 1)
   - 🔍 `/src/part2`: Template exploration (Part 2) - README only
   - ☁️ `/src/part3`: Azure OpenAI integration (Part 3)
   - �️ `/src/part4`: Products page implementation (Part 4)
   - 🚀 `/src/part5`: Azure deployment (Part 5) - README only
   - 🔧 `/src/part6`: Basic MCP server (Part 6)
   - 🏢 `/src/part7`: Enhanced MCP server with business tools (Part 7)
   - 📦 `/src/part8`: MCP publishing (Part 8) - README only
   ```

   **Note**: Some parts (2, 5, 8) contain only README files as they focus on exploration, deployment, or publishing rather than code creation.

### 2. Testing Updates

The workshop testing will include validation prompts for both the existing AI Web Chat functionality and the new MCP server capabilities.

#### AI Web Chat Testing (Parts 1-5)

- No changes to existing prompts
- Continue testing chat functionality, product queries, etc.

#### MCP Server Testing (Parts 6-8)

**New Testing Prompts:**

1. **Basic MCP Functionality**:

   ```text
   "Give me a random number between 1 and 100"
   ```

   *Expected*: MCP server responds with random number using `get_random_number` tool

2. **Custom Business Tool**:

   ```text
   "Look up order details for order ID 12345"
   ```

   *Expected*: MCP server uses `ContosoOrders` tool to return business data

3. **Configuration Testing**:

   ```text
   "What's the weather in Seattle?"
   ```

   *Expected*: MCP server uses environment variable configuration for weather choices

4. **Tool Discovery**:
   - Check VS Code tools panel shows custom MCP server
   - Verify all expected tools are listed and available

### 3. ContosoOrders Business Data Tool

#### Implementation

```csharp
using System.Text.Json;

[McpServerTool]
[Description("Retrieves order information from the Contoso business system.")]
public async Task<string> GetOrderDetails(
    [Description("The order ID to look up")] string orderId)
{
    // Simulate business data lookup
    var orders = new Dictionary<string, object>
    {
        ["12345"] = new { Customer = "John Doe", Total = "$150.00", Status = "Shipped", Items = new[] { "Camping Tent", "Sleeping Bag" } },
        ["12346"] = new { Customer = "Jane Smith", Total = "$89.99", Status = "Processing", Items = new[] { "Hiking Boots" } },
        ["12347"] = new { Customer = "Bob Johnson", Total = "$245.50", Status = "Delivered", Items = new[] { "Backpack", "Water Bottle", "Trail Mix" } }
    };
    
    if (orders.TryGetValue(orderId, out var order))
    {
        return JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
    }
    
    return $"Order {orderId} not found in the system.";
}

[McpServerTool]
[Description("Searches for orders by customer name.")]
public async Task<string> SearchOrdersByCustomer(
    [Description("Customer name to search for")] string customerName)
{
    // Implementation for customer-based order search
    // This could be expanded to include partial matching, etc.
    return $"Search functionality for customer '{customerName}' would be implemented here.";
}
```

### 4. Learning Objectives by Unit

#### Part 6: Introduction and Basic Setup

- **Learning Objectives**:
  - Understand what MCP is and why it's useful
  - Learn how MCP extends AI agents with custom capabilities
  - Set up development environment for MCP
  - Create and test a basic MCP server

- **Key Concepts**:
  - Model Context Protocol fundamentals
  - MCP servers vs. MCP clients
  - Tool registration and discovery
  - VS Code integration with MCP

#### Part 7: Custom Business Tools

- **Learning Objectives**:
  - Design tools that solve real business problems
  - Implement data access in MCP tools
  - Use configuration and environment variables
  - Test complex tool interactions

- **Key Concepts**:
  - Tool design principles
  - Parameter validation and descriptions
  - Error handling in MCP tools
  - Security considerations for business data

#### Part 8: Publishing and Distribution (Optional)

- **Learning Objectives**:
  - Package MCP servers for distribution
  - Understand NuGet packaging for MCP
  - Configure server metadata
  - Best practices for production deployment

- **Key Concepts**:
  - NuGet packaging workflows
  - Server configuration schemas
  - Version management
  - Documentation and discoverability

## Implementation Checklist

### Prerequisites ✅ ALL VERIFIED

- [x] **P.1:** Install .NET 10.0 SDK (preview 6 or higher) ✅ **Verified: 10.0.100-preview.6.25358.103**
- [x] **P.2:** Install `Microsoft.Extensions.AI.Templates` NuGet package ✅ **Verified: mcpserver template available**
- [x] **P.3:** Verify Visual Studio Code installation ✅ **Verified: 1.102.2**
- [x] **P.4:** Verify GitHub Copilot extension installation ✅ **Verified: github.copilot extensions installed**
- [x] **P.5:** Optional: Set up NuGet.org account for publishing ✅ **Confirmed: User has existing account**

### Phase 1: Repository Restructuring ✅ COMPLETED (Commit: 92c601a)

- [x] **1.1** Create `Part 1 - Setup` directory
- [x] **1.2** Move `lab/part0-setup.md` → `Part 1 - Setup/README.md` using `git mv`
- [x] **1.3** Create `Part 2 - Project Creation` directory  
- [x] **1.4** Move `lab/part1-create-project.md` → `Part 2 - Project Creation/README.md` using `git mv`
- [x] **1.5** Create `Part 3 - Template Exploration` directory
- [x] **1.6** Move `lab/part2-explore-template.md` → `Part 3 - Template Exploration/README.md` using `git mv`
- [x] **1.7** Create `Part 4 - Azure OpenAI` directory
- [x] **1.8** Move `lab/part3-azure-openai.md` → `Part 4 - Azure OpenAI/README.md` using `git mv`
- [x] **1.9** Create `Part 5 - Products Page` directory
- [x] **1.10** Move `lab/part4-products-page.md` → `Part 5 - Products Page/README.md` using `git mv`
- [x] **1.11** Create `Part 6 - Deployment` directory
- [x] **1.12** Move `lab/part5-deploy-azure.md` → `Part 6 - Deployment/README.md` using `git mv`
- [x] **1.13** Move `lab/images` and `lab/manuals` to repository root using `git mv`
- [x] **1.14** Update README.md to reflect new 6-part structure
- [x] **1.15** Fix all internal cross-references and image paths
- [x] **1.16** Remove empty `lab` directory
- [x] **1.17** Commit Phase 1 changes: "MCP refactor Phase 1: Restructure repository to 'Part X - Description' format"

### Phase 2: MCP Projects Creation

- [ ] **2.1** Create Part 6 MCP Server project structure
- [ ] **2.2** Implement basic MCP server template in Part 6
- [ ] **2.3** Create Part 7 MCP Business Tools project structure
- [ ] **2.4** Implement ContosoOrders tools in Part 7
- [ ] **2.5** Test basic MCP functionality (Part 6)
- [ ] **2.6** Test enhanced MCP functionality (Part 7)

### Phase 3: Documentation Creation

- [ ] **3.1** Create Part 6 README.md (MCP Server basics)
- [ ] **3.2** Create Part 7 README.md (MCP Business Tools)
- [ ] **3.3** Create Part 8 README.md (MCP Publishing)
- [ ] **3.4** Update main repository README.md
- [ ] **3.5** Update Part 2 README.md (exploration only)
- [ ] **3.6** Update Part 5 README.md (deployment only)

### Phase 4: Testing and Validation

- [ ] **4.1** Test Part 1 independently (AI Project creation)
- [ ] **4.2** Test Part 3 independently (Azure OpenAI)
- [ ] **4.3** Test Part 4 independently (Products Page)
- [ ] **4.4** Test Part 6 independently (MCP Server)
- [ ] **4.5** Test Part 7 independently (MCP Business Tools)
- [ ] **4.6** Validate progression flow Parts 1→3→4
- [ ] **4.7** Validate progression flow Parts 6→7
- [ ] **4.8** Test MCP integration with VS Code
- [ ] **4.9** Test MCP integration with GitHub Copilot

### Phase 5: Final Integration

- [ ] **5.1** Update testing documentation and prompts
- [ ] **5.2** Create instructor guides for MCP units
- [ ] **5.3** Validate complete workshop flow (Parts 1-8)
- [ ] **5.4** Final review and refinement
- [ ] **5.5** Update prerequisites documentation

### Prerequisites Setup (Before Starting)

- [ ] **P.1** Install .NET 10.0 SDK (preview 6 or higher)
- [ ] **P.2** Install Visual Studio Code
- [ ] **P.3** Install GitHub Copilot extension for VS Code
- [ ] **P.4** Install Microsoft.Extensions.AI.Templates
- [ ] **P.5** Verify current workshop functionality

## Quick Reference

**Current Status**: Ready to begin implementation
**Next Item**: P.1 - Install .NET 10.0 SDK

**Commands Reference:**

```bash
# Git moves (preserve history)
git mv src/start "src/Part 1 - Create AI Project"
git mv src/complete "src/temp-complete"

# Directory creation
mkdir "src/Part 2 - Explore Template"
mkdir "src/Part 3 - Azure OpenAI"
# ... etc

# MCP template creation
dotnet new mcpserver -n SampleMcpServer
```

---

## Prerequisites and Dependencies

### Additional Prerequisites for MCP Units

- **.NET 10.0 SDK** (preview 6 or higher) - Required for `dnx` command
- **Visual Studio Code** - Required for MCP integration
- **GitHub Copilot extension** - Required to test MCP server functionality
- **NuGet.org account** - Optional, for Part 8 publishing

### Template Dependencies

- `Microsoft.Extensions.AI.Templates` - For MCP server template
- `ModelContextProtocol` NuGet package - Core MCP functionality

## Implementation Considerations

### .NET 10 Preview Requirement

- **Consideration**: MCP requires .NET 10 preview, workshop uses .NET 9
- **Solution**: Clear documentation about version requirements, provide installation links

#### VS Code Dependency

- **Consideration**: Main workshop uses Visual Studio, MCP requires VS Code
- **Solution**: Make MCP section optional, provide clear VS Code setup instructions

#### GitHub Copilot Requirement

- **Consideration**: Testing MCP requires GitHub Copilot subscription
- **Solution**: Provide alternative testing methods, make this section optional for users without Copilot

#### Complexity Management

- **Consideration**: Adding MCP might overwhelm beginners
- **Solution**: Position MCP as advanced/optional content, clear prerequisite communication

### 8. Success Metrics

#### Workshop Completion Metrics

- Users can successfully create an MCP server project
- Users can test MCP functionality in VS Code
- Users understand the relationship between AI web apps and MCP servers
- Users can implement custom business tools

#### Learning Outcome Validation

- Pre/post knowledge assessment on MCP concepts
- Successful completion of testing prompts
- User feedback on clarity and usefulness of MCP content

## Implementation Guidelines

### Directory Content Guidelines

### For Code Units (Parts 1, 3, 4, 6, 7)

- Complete, working project snapshot
- Unit-specific README.md with:
  - Learning objectives
  - Prerequisites
  - Key concepts covered
  - Setup instructions
  - Testing validation

### For Documentation-Only Units (Parts 2, 5, 8)

- README.md with detailed instructions
- Links to code from previous relevant unit
- Clear explanation of why no code snapshot is needed

### Migration Strategy

The transition from the current structure will follow these principles:

1. **Preserve git history** - use `git mv` commands for all file relocations to maintain version history
2. **Preserve existing functionality** - ensure all current lab content works
3. **Create unit snapshots** by systematically building through each part
4. **Test each unit independently** before moving to next
5. **Update documentation iteratively** as units are created
6. **Validate complete workshop flow** from start to finish

**Key Git History Preservation:**

- All existing lab documentation will be moved using `git mv` to preserve commit history
- Current `/src/start` and `/src/complete` will be relocated using `git mv`
- New directories will be created only where no existing content exists

## Next Steps

1. **Begin Phase 1** (repository restructuring and unit creation)
2. **Set up development environment** for MCP testing (.NET 10 preview)
3. **Create proof-of-concept** for Part 6 basic MCP server
4. **Validate workshop flow** by testing complete progression through all units
5. **Develop instructor training materials** for new MCP content
6. **Create comprehensive testing scripts** for validation
7. **Finalize documentation** and review process

## Key Success Factors

- **Independent unit validation** - each unit must work as a standalone snapshot
- **Clear progression path** - students should understand how units build on each other
- **Technology transition** - smooth handoff from AI Web Chat (.NET 9, VS) to MCP (.NET 10, VS Code)
- **Comprehensive testing** - validate both technical functionality and learning outcomes
- **Instructor readiness** - ensure teaching materials support the new structure

---

*This implementation plan will be updated as development progresses.*
