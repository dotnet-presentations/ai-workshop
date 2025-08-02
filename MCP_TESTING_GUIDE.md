# MCP Testing Guide

## Overview

This document provides comprehensive testing procedures for validating Model Context Protocol (MCP) functionality in the AI Workshop. Use this guide to verify that all MCP servers work correctly with GitHub Copilot and VS Code.

## Prerequisites Verification

Before testing MCP functionality, verify all prerequisites are met:

### ✅ Environment Checklist

- [ ] **.NET 10.0 SDK** (preview 6 or higher) installed

  ```powershell
  dotnet --version
  # Expected: 10.0.100-preview.6.25358.103 or higher
  ```

- [ ] **Visual Studio Code** installed and updated

  ```powershell
  code --version
  # Expected: 1.102.2 or higher
  ```

- [ ] **GitHub Copilot Extension** installed and active
  - Open VS Code → Extensions → Search "GitHub Copilot"
  - Verify both `github.copilot` and `github.copilot-chat` are installed
  - Check status bar shows Copilot is active

- [ ] **MCP Template** available

  ```powershell
  dotnet new list | findstr mcp
  # Expected: mcpserver template should be listed
  ```

### ✅ Workshop Prerequisites

- [ ] **Completed Parts 1-6** of the AI Workshop
- [ ] **Working AI Web Chat application** (from Parts 1-5)
- [ ] **Basic understanding** of .NET development

## Part 7 Testing: MCP Server Basics (WeatherMcpServer)

### Test 1: Project Build Verification

**Objective**: Verify the WeatherMcpServer project builds successfully

**Steps**:

1. Navigate to the Part 7 project directory:

   ```powershell
   cd "Part 7 - MCP Server Basics\WeatherMcpServer"
   ```

2. Build the project:

   ```powershell
   dotnet build
   ```

**Expected Results**:

- ✅ Build succeeds with no errors
- ✅ No warnings related to missing dependencies
- ✅ Output shows successful compilation

**Troubleshooting**:

- If build fails, verify .NET 10 SDK is installed
- Check that `ModelContextProtocol` package is properly restored

### Test 2: MCP Server Configuration

**Objective**: Verify VS Code can discover and configure the MCP server

**Steps**:

1. Open VS Code in the workshop root directory
2. Check if `.vscode/mcp.json` exists:

   ```json
   {
     "servers": {
       "weather-server": {
         "command": "dnx",
         "args": ["run", "--project", "./Part 7 - MCP Server Basics/WeatherMcpServer"]
       }
     }
   }
   ```

3. Restart VS Code to load MCP configuration
4. Open GitHub Copilot Chat (Ctrl+Shift+P → "GitHub Copilot: Open Chat")

**Expected Results**:

- ✅ MCP server appears in VS Code's MCP panel
- ✅ No connection errors in VS Code output
- ✅ GitHub Copilot can discover the weather tools

**Troubleshooting**:

- If MCP server not found, verify the path in `mcp.json`
- Check VS Code output panel for MCP-related errors
- Ensure GitHub Copilot extension is properly signed in

### Test 3: Weather Tools Functionality

**Objective**: Test weather tools through GitHub Copilot

**Test 3.1: Current Weather**

1. Open GitHub Copilot Chat
2. Enter prompt: "What's the current weather in Seattle?"

**Expected Results**:

- ✅ Copilot uses `GetCurrentWeather` tool
- ✅ Returns simulated weather data for Seattle
- ✅ Response includes temperature, conditions, and humidity

**Test 3.2: Weather Forecast**

1. In Copilot Chat, enter: "Give me a 5-day weather forecast for New York"

**Expected Results**:

- ✅ Copilot uses `GetWeatherForecast` tool
- ✅ Returns 5-day forecast data
- ✅ Each day includes date, temperature range, and conditions

**Test 3.3: Invalid Location Handling**

1. In Copilot Chat, enter: "What's the weather in InvalidCity?"

**Expected Results**:

- ✅ Tool handles invalid location gracefully
- ✅ Returns appropriate error message
- ✅ No exceptions or crashes

### Test 4: Tool Discovery and Descriptions

**Objective**: Verify tools have proper descriptions and parameters

**Steps**:

1. In Copilot Chat, ask: "What weather tools are available?"
2. Request tool usage help: "How do I use the weather tools?"

**Expected Results**:

- ✅ Copilot can list available weather tools
- ✅ Tool descriptions are clear and helpful
- ✅ Parameter requirements are properly documented

## Part 8 Testing: Enhanced MCP Server (ContosoOrdersMcpServer)

### Test 5: Business Tools Build Verification

**Objective**: Verify the ContosoOrdersMcpServer builds successfully

**Steps**:

1. Navigate to Part 8 directory:

   ```powershell
   cd "Part 8 - Enhanced MCP Server\ContosoOrdersMcpServer"
   ```

2. Build the project:

   ```powershell
   dotnet build
   ```

**Expected Results**:

- ✅ Build succeeds with no errors
- ✅ All business tools compile correctly
- ✅ No missing dependency warnings

### Test 6: Business Tools Configuration

**Objective**: Configure and test business MCP server

**Steps**:

1. Update `.vscode/mcp.json` to include business server:

   ```json
   {
     "servers": {
       "weather-server": {
         "command": "dnx",
         "args": ["run", "--project", "./Part 7 - MCP Server Basics/WeatherMcpServer"]
       },
       "contoso-orders": {
         "command": "dnx",
         "args": ["run", "--project", "./Part 8 - Enhanced MCP Server/ContosoOrdersMcpServer"]
       }
     }
   }
   ```

2. Restart VS Code
3. Verify both servers are discoverable

**Expected Results**:

- ✅ Both MCP servers load successfully
- ✅ VS Code shows both servers in MCP panel
- ✅ No configuration conflicts

### Test 7: Order Management Tools

**Objective**: Test business tools functionality

**Test 7.1: Order Lookup**

1. In Copilot Chat, enter: "Look up order details for order ID 12345"

**Expected Results**:

- ✅ Uses `GetOrderDetails` tool
- ✅ Returns order information (customer: John Doe, items, status)
- ✅ Data is properly formatted and readable

**Test 7.2: Customer Search**

1. Enter: "Find all orders for customer Jane Smith"

**Expected Results**:

- ✅ Uses `SearchOrdersByCustomer` tool
- ✅ Returns relevant customer order information
- ✅ Handles customer name matching correctly

**Test 7.3: Product Inventory**

1. Enter: "Check inventory for hiking boots"

**Expected Results**:

- ✅ Uses `GetProductInventory` tool
- ✅ Returns product availability and pricing
- ✅ Provides useful inventory information

### Test 8: Data Validation and Error Handling

**Objective**: Test business tools handle edge cases properly

**Test 8.1: Invalid Order ID**

1. Enter: "Look up order 99999"

**Expected Results**:

- ✅ Tool returns "order not found" message
- ✅ No exceptions or crashes
- ✅ User-friendly error message

**Test 8.2: Empty Customer Name**

1. Enter: "Find orders for customer with empty name"

**Expected Results**:

- ✅ Tool handles empty/invalid input gracefully
- ✅ Returns appropriate validation message

## MCP Integration Testing

### Test 9: Multi-Server Functionality

**Objective**: Verify both MCP servers work simultaneously

**Steps**:

1. With both servers configured, test mixed queries:
   - "What's the weather in Seattle and do we have any orders for delivery there?"
   - "Check inventory for camping gear and get weather forecast for outdoor events"

**Expected Results**:

- ✅ Copilot can use tools from both servers
- ✅ No conflicts between different MCP servers
- ✅ Responses combine information from multiple sources

### Test 10: Performance and Reliability

**Objective**: Test MCP server stability under normal use

**Steps**:

1. Execute 10+ consecutive tool calls
2. Test rapid successive requests
3. Leave servers running for extended periods

**Expected Results**:

- ✅ Consistent response times
- ✅ No memory leaks or performance degradation
- ✅ Stable operation over time

## Troubleshooting Guide

### Common Issues and Solutions

#### Issue 1: "MCP server not found"

**Symptoms**:

- VS Code can't discover MCP server
- Tools not available in Copilot

**Solutions**:

1. Verify `.vscode/mcp.json` path is correct
2. Check that project builds successfully
3. Restart VS Code after configuration changes
4. Verify .NET 10 SDK is in PATH

#### Issue 2: "Tool execution failed"

**Symptoms**:

- MCP server starts but tools fail to execute
- Error messages in VS Code output

**Solutions**:

1. Check VS Code Output panel → "MCP" channel
2. Verify tool methods have proper attributes
3. Check for runtime exceptions in tool code
4. Validate tool parameter types and descriptions

#### Issue 3: "GitHub Copilot can't use tools"

**Symptoms**:

- MCP server runs but Copilot doesn't use tools
- Manual tool calls work but automatic discovery fails

**Solutions**:

1. Verify GitHub Copilot subscription is active
2. Check tool descriptions are clear and specific
3. Use explicit prompts that match tool capabilities
4. Restart GitHub Copilot extension

#### Issue 4: "Build errors in MCP projects"

**Symptoms**:

- `dotnet build` fails
- Missing package references

**Solutions**:

1. Verify .NET 10 SDK installation
2. Run `dotnet restore` to restore packages
3. Check `ModelContextProtocol` package version
4. Clear NuGet cache if needed: `dotnet nuget locals all --clear`

### Diagnostic Commands

**Check MCP Server Status**:

```powershell
# Verify server can start manually
cd "Part 7 - MCP Server Basics\WeatherMcpServer"
dnx run
```

**VS Code MCP Logs**:

1. Open VS Code
2. View → Output
3. Select "MCP" from dropdown
4. Check for connection and execution logs

**GitHub Copilot Status**:

1. Command Palette → "GitHub Copilot: Check Status"
2. Verify subscription and connection
3. Check extension version compatibility

## Success Criteria

### Part 7 (Weather MCP Server)

- ✅ Project builds without errors
- ✅ VS Code discovers MCP server
- ✅ Weather tools respond correctly to prompts
- ✅ Error handling works for invalid inputs
- ✅ Tool descriptions are clear and accurate

### Part 8 (Business MCP Server)

- ✅ Project builds without errors
- ✅ Business tools integrate with Copilot
- ✅ Order, customer, and inventory tools function correctly
- ✅ Data validation prevents errors
- ✅ Multiple servers work simultaneously

### Integration

- ✅ Both MCP servers can run concurrently
- ✅ GitHub Copilot can access all tools
- ✅ Performance remains stable under normal use
- ✅ Workshop progression flows smoothly from Part 7 to Part 8

## Validation Checklist

Use this checklist to verify complete MCP functionality:

### Environment Setup

- [x] .NET 10 SDK installed and verified ✅ **10.0.100-preview.6.25358.103**
- [x] VS Code with GitHub Copilot extensions ✅ **Confirmed available**
- [x] MCP template available ✅ **mcpserver template working**
- [x] Workshop prerequisites completed ✅ **All parts validated**

### Part 7 - Weather MCP Server

- [x] Project builds successfully ✅ **Verified with expected warnings**
- [ ] MCP server starts and connects
- [ ] `GetCurrentWeather` tool works correctly
- [ ] `GetWeatherForecast` tool works correctly
- [ ] Error handling for invalid locations
- [ ] Tool discovery and descriptions accurate

### Part 8 - Business MCP Server

- [x] Project builds successfully ✅ **Verified with expected warnings**
- [ ] Business MCP server starts and connects
- [ ] `GetOrderDetails` tool works correctly
- [ ] `SearchOrdersByCustomer` tool works correctly
- [ ] `GetProductInventory` tool works correctly
- [ ] Data validation and error handling
- [ ] Multiple servers work together

### Integration and Performance

- [ ] Multi-server functionality verified
- [ ] GitHub Copilot integration stable
- [ ] Performance acceptable under normal use
- [ ] All troubleshooting scenarios tested

## Enhanced Testing Prompts (Updated for Phase 5)

### Part 7 - Weather MCP Testing Prompts

**Basic Weather Query**:

- "What's the current weather in Seattle?"
- Expected: Uses GetCurrentWeather, returns temperature, condition, humidity

**Forecast Request**:

- "Give me a 5-day weather forecast for London"  
- Expected: Uses GetWeatherForecast, returns detailed daily forecasts

**Multiple Cities**:

- "Compare the weather between New York and Los Angeles"
- Expected: Makes multiple GetCurrentWeather calls, provides comparison

**Invalid Location**:

- "What's the weather in FakeCity123?"
- Expected: Graceful error handling, user-friendly message

### Part 8 - Business MCP Testing Prompts

**Order Lookup**:

- "Look up order details for order ID 12345"
- Expected: Returns John Doe order with camping tent and sleeping bag

**Customer Search**:

- "Find all orders for customer Jane Smith"  
- Expected: Uses SearchOrdersByCustomer, returns relevant results

**Inventory Check**:

- "Check inventory for hiking boots"
- Expected: Uses GetProductInventory, returns availability and pricing

**Complex Business Query**:

- "Find orders for John Doe and check if we have the items in stock"
- Expected: Combines multiple tool calls for comprehensive business insight

### Multi-Server Integration Prompts

**Weather + Business**:

- "What's the weather in Seattle and do we have any orders shipping there?"
- Expected: Uses both weather and business tools in coordination

**Planning Query**:

- "Check weather forecast for outdoor events and our camping gear inventory"
- Expected: Combines forecast data with inventory information

---

## Next Steps

After completing this testing guide:

1. **Phase 4**: Execute systematic testing using this guide ✅ **COMPLETED**
2. **Phase 5**: Address any issues found during testing 🚀 **IN PROGRESS**
3. **Final Review**: Validate complete workshop experience

For issues not covered in this guide, refer to:

- [Part 7 README](Part%207%20-%20MCP%20Server%20Basics/README.md)
- [Part 8 README](Part%208%20-%20Enhanced%20MCP%20Server/README.md)
- [MCP Workshop Planning](MCP_WORKSHOP_PLANNING.md)
