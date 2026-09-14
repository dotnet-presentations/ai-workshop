# Workshop Test Report - 2026-09-11

Full attendee-style validation of Parts 1-11. All generated and modified code
was kept under `test-workspace/full-2026-09-11/`; committed snapshots were used
only for reconciliation.

## Environment

- .NET SDK: `10.0.401` (`9.0.200`, `9.0.318`, and `10.0.112` also installed)
- OS: Windows 11 Enterprise 64-bit, version 10.0.26200
- Docker: Docker Desktop 29.6.2, Linux containers
- Azure Developer CLI: 1.20.0
- Template versions:
  - `Microsoft.Extensions.AI.Templates` 10.10.0-preview.3.26459.2
  - `Microsoft.McpServer.ProjectTemplates` 1.2.1
- Scope tested: Parts 1-11; Part 11 configuration-only, no Azure deployment
- AI access: Microsoft Azure AI Proxy with `gpt-5-mini` and
  `text-embedding-3-small`

The proxy rejected requests until the event was marked active. After activation,
the same endpoint and attendee key worked without source changes.

## Results

| Part | Status | Time | Notes |
| --- | --- | --- | --- |
| 1 - Setup | Pass | Not measured | .NET 10, templates, Docker, azd, and credentials verified |
| 2 - Build Chat App | Pass with issues | Not measured | Streaming, history, and typed summary passed; unversioned packages drifted |
| 3 - Add RAG | Pass with issues | Not measured | Manual and MEDI paths passed; sample copy path assumes a specific project location |
| 4 - AI Web Chat Template | Pass with issues | Not measured | End-to-end Qdrant path passed after correcting a stale connector instruction |
| 5 - MCP Server Basics | Pass | Not measured | Fresh scaffold, build, discovery, and calls passed |
| 6 - Enhanced MCP Server | Pass | Not measured | Build and business-tool protocol calls passed |
| 7 - MCP Publishing | Pass | Not measured | Documentation reviewed; local pack produced seven packages; nothing published |
| 8 - Agent Framework Basics | Pass with issues | Not measured | Agent called the correct tool across turns; package versions drifted |
| 9 - Adding AI to an Existing App | Pass with issues | 8m 24s | Semantic search and grounded answers passed; README and answer-key gaps found |
| 10 - Choosing Providers and Services | Pass with issues | Not measured | Azure AI Search migration built after correcting stale Qdrant package removal |
| 11 - Deployment | Pass | Not measured | Configuration-only checks and Release build passed |

Status: Pass / Pass with issues / Fail / Skipped.

## Part detail

### Part 1 - Setup

- **What was run:** Verified installed SDKs, templates, Docker Desktop, the Docker
  daemon, Azure Developer CLI, and proxy-backed Foundry credentials.
- **Result:** Required tooling was available. The proxy event had to be marked
  active before the issued attendee key was accepted.
- **Documentation clarity:** The instructor proxy note should call out that
  attendee keys return HTTP 401 while an event is inactive.
- **Snapshot comparison:** Not applicable.
- **Snapshot updated:** No.

### Part 2 - Build Chat App

- **What was run:** Scaffolded `ChatApp` with `dotnet new console`, ran every
  package command, added the README code, built in Release with warnings treated
  as errors, then tested streaming, conversation history, and `summary`.
- **Result:** All runtime behavior passed. The structured response produced a
  typed topic, sentiment, and follow-up list.
- **Documentation clarity:** The instructions were sufficient.
- **Snapshot comparison:** Source behavior matched. Current unversioned commands
  resolved `Microsoft.Extensions.AI*` 10.10.0 and
  `Microsoft.Extensions.*` 10.0.12, while the snapshot pins 10.9.0 and 10.0.11.
- **Snapshot updated:** No; package drift is recorded for coordinated review.

### Part 3 - Add RAG

- **What was run:** Continued the Part 2 scratch project, built the manual
  in-memory RAG implementation, then added the MEDI/SQLite pipeline. Tested
  grounded and unrelated questions in both variants. Built both checkpoint
  verification projects from a scratch copy.
- **Result:** The manual path embedded 11 chunks. Both variants answered product
  care/warranty questions from the supplied document and refused an unrelated
  World Cup question. Release builds had zero warnings, and the final dependency
  graph had no known vulnerabilities.
- **Documentation clarity:** The sample-document copy command only works when the
  attendee project is directly under the repository root. From the skill's
  required nested `test-workspace/` directory, the literal path fails.
- **Snapshot comparison:** The final MEDI implementation matches
  `checkpoints/medi-program.cs`, not the committed `RagChatApp/Program.cs`, which
  represents the manual checkpoint. Current unversioned packages resolve
  DataIngestion 10.10 preview and Microsoft.Extensions.AI 10.10 rather than the
  snapshot/checkpoint versions.
- **Snapshot updated:** No.

### Part 4 - AI Web Chat Template

- **What was run:** Scaffolded the exact Qdrant + Aspire command using template
  10.10.0-preview.3.26459.2, followed the configuration steps, built Release, and
  ran AppHost with Docker. Also scaffolded and built the Docker-free variant.
- **Result:** The literal package instructions initially failed with `CS0121`.
  The current template already uses `CommunityToolkit.VectorData.Qdrant`, while
  the README adds `Microsoft.SemanticKernel.Connectors.Qdrant`, producing
  ambiguous Qdrant registrations. Removing the newly added Semantic Kernel
  connector restored a zero-warning build. Qdrant, markitdown, and the web app
  then became healthy. The first browser query completed in 11.5 seconds,
  returned two PDF citations, and opened the citation viewer successfully. The
  Docker-free app built cleanly and returned HTTP 200.
- **Documentation clarity:** Step 2.1 is stale for the current template and makes
  the instructed project fail to compile.
- **Snapshot comparison:** The current template changes package versions,
  OpenTelemetry versions, DOMPurify 3.2.4 to 3.4.13, generated assets/signatures,
  ports, GUIDs, and generated README content.
- **Snapshot updated:** No.

### Part 5 - MCP Server Basics

- **What was run:** Installed template 1.2.1, scaffolded `MyMcpServer`, upgraded
  `ModelContextProtocol` to 2.2.0, added the weather tools, built Release, and
  exercised real `tools/list` and `tools/call` requests.
- **Result:** Random number, current weather, and five-day forecast discovery and
  calls passed with typed structured content. No vulnerable packages were found.
- **Documentation clarity:** The documented template reinstall exit code 106
  behavior matched.
- **Snapshot comparison:** Source behavior matched. The current template emits
  `Microsoft.Extensions.Hosting` 10.0.12 rather than 10.0.11.
- **Snapshot updated:** No.

### Part 6 - Enhanced MCP Server

- **What was run:** Copied the existing server into the scratch workspace, built
  it, and used the protocol harness for order, customer, inventory, and missing
  order scenarios.
- **Result:** All discovery and call assertions passed with zero build warnings
  and no known vulnerable packages.
- **Documentation clarity:** No blocking issue found.
- **Snapshot comparison:** No functional differences were introduced.
- **Snapshot updated:** No.

### Part 7 - MCP Publishing

- **What was run:** Reviewed the publishing instructions and packed the fresh
  Part 5 server locally. No package was published or globally installed.
- **Result:** Packing produced the base package and six RID packages. The base
  package contained the nuspec, `README.md`, `.mcp/server.json`, and
  `tools/any/any/DotnetToolSettings.xml`.
- **Documentation clarity:** Expected package layout matched the README.
- **Snapshot comparison:** Not applicable.
- **Snapshot updated:** No.

### Part 8 - Agent Framework Basics

- **What was run:** Scaffolded `AgentApp`, ran the package commands, added the
  README code, built Release, and asked about `ORD-1001` followed by `ORD-1002`.
- **Result:** The agent selected `GetOrderStatus` and returned the expected
  status for both orders across turns. No vulnerable packages were found.
- **Documentation clarity:** The hands-on flow was sufficient.
- **Snapshot comparison:** Source behavior matched. Current unversioned package
  commands resolve Microsoft.Extensions.AI 10.10.0 and configuration 10.0.12,
  while the snapshot pins 10.9.0 and 10.0.11.
- **Snapshot updated:** No.

### Part 9 - Adding AI to an Existing App

- **What was run:** Copied `eShopLite-start` into the scratch workspace, verified
  baseline keyword search, followed Steps 1 and 2, tested all six documented
  semantic queries, tested grounded chat and the no-match path, built Release
  with warnings as errors, and audited dependencies.
- **Result:** Semantic ranking and the 0.75 distance threshold behaved as
  documented. The grounded camping answer used two retrieved products. The
  socket-wrench query returned the fixed no-match response without calling chat.
  Build and audit passed. Optional Step 3 was skipped because the configured
  local `/v1/models` endpoint was not reachable and no model download was
  attempted.
- **Documentation clarity:** Baseline `water` search returns three products, not
  only the bottle. Step 2.4 does not provide a complete `Discovery.razor`, so an
  attendee must infer or copy it from the answer key.
- **Snapshot comparison:** Steps 1 and 2 matched behavior. The answer key has an
  undocumented stale-vector safeguard using `FirstOrDefault`/`OfType`, whereas
  the README teaches `First`. Unversioned AI packages resolve 10.10.0 rather than
  the answer key's 10.9.0. Step 3-only files were intentionally absent.
- **Snapshot updated:** No.

### Part 10 - Choosing Providers and Services

- **What was run:** Applied the documented Azure AI Search package, AppHost, web
  registration, and collection-key changes to the Part 4 scratch application.
- **Result:** The migration built with zero warnings after correcting the Qdrant
  package removal.
- **Documentation clarity:** The README removes
  `Microsoft.SemanticKernel.Connectors.Qdrant`, but the current template uses
  `CommunityToolkit.VectorData.Qdrant`. The latter remains installed unless the
  attendee discovers and removes it.
- **Snapshot comparison:** Shared application behavior aligned with Part 11
  after the package correction. Current template dependencies and generated
  assets have drifted from the committed snapshot.
- **Snapshot updated:** No.

### Part 11 - Deployment

- **What was run:** Confirmed `AddConnectionString("openai")`,
  `AddAzureSearch("search")`, `WithExternalHttpEndpoints()`, Azure AI Search
  registrations, string vector keys, and the absence of committed `azure.yaml`
  and `.azure/`. Built `GenAiLab.sln` in Release and audited packages.
- **Result:** Configuration checks, build, and vulnerability audit passed.
- **Documentation clarity:** No additional configuration-only issue found.
- **Snapshot comparison:** The snapshot remains internally consistent, but its
  dependencies and generated web assets trail the current template.
- **Snapshot updated:** No. No Azure resources were provisioned.

## Snapshot reconciliation

| Snapshot | Differences found | Resolution |
| --- | --- | --- |
| `Part 02 - Build Chat App/ChatApp/` | Current unversioned packages resolve one patch/minor newer | Record package drift; review versions together |
| `Part 03 - Add RAG/RagChatApp/` | Snapshot is the manual checkpoint; final README flow is MEDI; package drift | Clarify snapshot/checkpoint role and review pins |
| `Part 05 - MCP Server Basics/MyMcpServer/` | Hosting package 10.0.12 versus 10.0.11 | Template drift; no behavior change |
| `Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/` | No functional difference | No change |
| `Part 08 - Agent Framework Basics/AgentApp/` | Microsoft.Extensions.AI 10.10.0 versus 10.9.0 | Record package drift |
| `Part 09 - Adding AI to an Existing App/eShopLite-start/` | Baseline prose does not match `water` results | Update README example |
| `Part 09 - Adding AI to an Existing App/eShopLite/` | Package drift and undocumented stale-vector safeguard | Teach the safeguard and reconcile packages |
| `Part 11 - Deployment/GenAiLab/` | Current template package/assets drift; Qdrant connector changed | Re-scaffold progression and review coordinated update |

## Issues

1. **Part 4 installs a conflicting Qdrant connector**
   - Reproduce: scaffold with the exact Part 4 command using template
     10.10.0-preview.3.26459.2, then run all Step 2.1 package commands and build.
   - Expected: zero-warning build.
   - Actual: `CS0121` ambiguous Qdrant registration errors because both
     CommunityToolkit and Semantic Kernel connectors are installed.
   - Change: `Part 04 - AI Web Chat Template/README.md`.

2. **Part 10 removes the obsolete connector instead of the current one**
   - Reproduce: carry the current Part 4 scaffold into Part 10 and run Step 1.
   - Expected: all Qdrant packages are removed before Azure AI Search is added.
   - Actual: `CommunityToolkit.VectorData.Qdrant` remains installed.
   - Change: `Part 10 - Choosing Providers and Services/README.md`.

3. **Part 9 does not provide the complete page attendees are told to create**
   - Reproduce: follow Step 2.4 without opening the answer key.
   - Expected: enough markup and code to create a working `Discovery.razor`.
   - Actual: only selected fragments are supplied.
   - Change: `Part 09 - Adding AI to an Existing App/README.md`.

4. **Part 9 teaches a less defensive vector-result lookup than the answer key**
   - Reproduce: compare the Step 1.6 endpoint with the completed snapshot.
   - Expected: README and answer key implement the same behavior.
   - Actual: README uses `First`, while the answer key skips stale vector IDs
     using `FirstOrDefault` and `OfType`.
   - Change: `Part 09 - Adding AI to an Existing App/README.md`.

5. **Part 9's baseline `water` result is inaccurate**
   - Reproduce: run the starting app and search for `water`.
   - Expected from prose: the Insulated Water Bottle.
   - Actual: Outdoor Rain Jacket, Insulated Water Bottle, and Daypack.
   - Change: `Part 09 - Adding AI to an Existing App/README.md`.

6. **Part 3's sample copy command depends on project location**
   - Reproduce: create the Part 2 project in a nested attendee/test directory and
     run the Part 3 `copy "..\Part 03 - Add RAG\..."` command literally.
   - Expected: sample document copied.
   - Actual: source path does not exist.
   - Change: `Part 03 - Add RAG/README.md`.

7. **Unversioned package commands no longer reproduce several snapshots**
   - Reproduce: run Parts 2, 3, 8, and 9 package commands today.
   - Expected: generated project matches the committed answer.
   - Actual: Microsoft.Extensions.AI resolves 10.10.0 instead of 10.9.0, with
     related patch drift.
   - Change: affected READMEs and snapshots, coordinated through the dependency
     policy.

## Recommended documentation improvements

- Update Parts 4 and 10 together for the current
  `CommunityToolkit.VectorData.Qdrant` template output.
- Make Part 3's sample-document source path independent of where the Part 2
  project was created, or explicitly require the project location.
- Provide the complete `Discovery.razor` in Part 9 and teach the answer key's
  stale-vector handling.
- Correct Part 9's baseline `water` example.
- Clarify that `RagChatApp/` is the manual checkpoint while
  `checkpoints/medi-program.cs` is the final recommended implementation.
- Decide whether unversioned package commands are intentional. If snapshots are
  expected to be reproducible, pin the documented versions and update the
  coordinated snapshots.
- Add an instructor proxy troubleshooting note: attendee keys are unauthorized
  until the event is active.

## Summary

The workshop remains runnable end to end on .NET 10.0.401. Live chat, manual RAG,
MEDI ingestion, Qdrant-backed web RAG with citations, MCP tools, Agent Framework,
eShopLite semantic search, and grounded product discovery all passed. All tested
projects built with zero warnings, and vulnerability audits were clean.

The main release blocker is current AI Web Chat template drift: Part 4 adds a
second Qdrant connector and fails to compile, and Part 10 removes the wrong
connector. The remaining findings are documentation completeness, location
assumptions, answer-key parity, and expected package-version drift.
