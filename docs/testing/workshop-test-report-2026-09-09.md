# Workshop Test Report - 2026-09-09

An attendee-style validation of the Model Context Protocol progression in Parts 5-8 after
upgrading the workshop to `ModelContextProtocol` 2.2.0. The Part 5 project was scaffolded
in `test-workspace/` from the documented template rather than copied from the committed
snapshot. Publishing to NuGet.org and manual IDE client flows were out of scope.

## Environment

- .NET SDK: `10.0.400` (temporarily pinned for validation)
- OS: Windows, PowerShell
- Docker / Podman: not required
- Template versions: `Microsoft.McpServer.ProjectTemplates` 1.2.1
- MCP SDK versions: `ModelContextProtocol` 2.2.0 and `ModelContextProtocol.Core` 2.2.0
- Scope tested: Parts 5-8 MCP instructions

## Results

| Part | Status | Time | Notes |
| --- | --- | --- | --- |
| 1 - Setup | Skipped | - | Outside scope |
| 2 - Build Chat App | Skipped | - | Outside scope |
| 3 - Add RAG | Skipped | - | Outside scope |
| 4 - AI Web Chat Template | Skipped | - | Outside scope |
| 5 - MCP Server Basics | Pass | Not measured | Fresh scaffold build/reconciliation and committed-snapshot discovery/calls passed |
| 6 - Enhanced MCP Server | Pass | Not measured | Build, discovery, success cases, and missing-order case passed |
| 7 - MCP Publishing | Pass | Not measured | Seven packages created; base package contents verified |
| 8 - Agent Framework Basics | Pass | Not measured | Snapshot and optional MCP client excerpt compiled |
| 9 - Adding AI to an Existing App | Skipped | - | Outside scope |
| 10 - Choosing Providers and Services | Skipped | - | Outside scope |
| 11 - Deployment | Skipped | - | Outside scope |

Status: Pass / Pass with issues / Fail / Skipped.

## Part detail

### Part 5 - MCP Server Basics

- **What was run:** Installed `Microsoft.McpServer.ProjectTemplates::1.2.1`, scaffolded a
  new `mcpserver`, upgraded it to `ModelContextProtocol` 2.2.0, followed the README edits,
  built Release, and ran `tests/McpStructuredOutputTests` against the committed server.
- **Result:** Release build passed with zero warnings. The client discovered all three
  tools and called the weather, forecast, and random-number tools successfully. Structured
  results were typed, including direct numeric content for the scalar tool.
- **Documentation clarity:** Reinstalling the already-installed exact template can exit
  with code 106 even though version 1.2.1 is available. The README now explains how to
  verify and continue. It also correctly explains that a stdio server may show no startup
  banner because stdout carries protocol messages.
- **Snapshot comparison:** Template metadata and `.mcp/server.json` match the committed
  baseline. Source differences are the documented attendee edits: namespace alignment,
  structured random output, weather tools, and registration.
- **Snapshot updated:** No; the committed snapshot already represents the completed lab.

### Part 6 - Enhanced MCP Server

- **What was run:** Built the committed server in Release and used
  `tests/McpStructuredOutputTests` for `tools/list` and `tools/call` over stdio.
- **Result:** Build passed with zero warnings. Order lookup, customer search, inventory
  lookup, and the unknown-order typed response all passed.
- **Documentation clarity:** The active testing guide now reflects the deterministic
  sample data instead of expecting errors for arbitrary cities or empty customer names.
- **Snapshot comparison:** No differences were introduced by this documentation run.
- **Snapshot updated:** No.

### Part 7 - MCP Publishing

- **What was run:** Applied the documented test metadata to the attendee-created Part 5
  project, ran `dotnet pack --configuration Release`, and extracted the base package.
- **Result:** Packing passed and produced one base package plus six RID packages. The base
  package contains `README.md`, `.mcp/server.json`, the nuspec, and
  `tools/any/any/DotnetToolSettings.xml`. Its package types are `DotnetTool` and
  `McpServer`; the manifest uses the `2025-10-17` schema and stdio transport.
- **Documentation clarity:** The expected tool-settings path was corrected from the stale
  target-framework-specific location. Publishing to a registry was not performed.
- **Snapshot comparison:** Not applicable; Part 7 extends the attendee project.
- **Snapshot updated:** No.

### Part 8 - Agent Framework Basics

- **What was run:** Built the committed Agent Framework snapshot. In a separate .NET 10
  console project, added `ModelContextProtocol.Core` 2.2.0 and compiled the revised optional
  MCP client excerpt with its complete `StdioClientTransport` configuration.
- **Result:** Both Release builds passed with zero warnings.
- **Documentation clarity:** The optional excerpt now includes its package dependency,
  defines the transport it consumes, and states its relative-path assumption. The live
  Visual Studio and VS Code client paths were not run.
- **Snapshot comparison:** No snapshot changes were needed.
- **Snapshot updated:** No.

## Snapshot reconciliation

| Snapshot | Differences found | Resolution |
| --- | --- | --- |
| `Part 05 - MCP Server Basics/MyMcpServer/` | Only documented attendee source edits from the 1.2.1 template | No snapshot change needed |
| `Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/` | None in this scoped run | No snapshot change needed |
| `Part 08 - Agent Framework Basics/AgentApp/` | None in this scoped run | No snapshot change needed |

## Issues

No unresolved defects were found in the scoped command-line flow. During validation, MCP
SDK 2.2.0 advertised the scalar tool as either an integer schema or an object with a required
integer `result`, while calls consistently returned direct numeric structured content. The
protocol harness now accepts only those two observed discovery forms and still enforces the
direct scalar call result.

## Recommended documentation improvements

The stale instructions found during this run were corrected in the same change. No further
documentation changes are required from this validation.

## Summary

The command-line MCP progression in Parts 5-8 is reproducible with .NET 10.0.400, template
1.2.1, and MCP SDK 2.2.0. Both committed servers build without warnings or known vulnerable
packages, real protocol discovery and calls pass, the publishing lab creates the expected
seven-package layout, and the optional Part 8 MCP client excerpt compiles.
