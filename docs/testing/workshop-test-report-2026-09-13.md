# Workshop Test Report - 2026-09-13

Full attendee-style validation of Parts 1-11 after the workshop moved its Aspire
baseline to 13.4.6 and updated the Part 5 and Part 7 MCP guidance. This report
supersedes the earlier September 13 test results.

All generated and modified code stayed under
`test-workspace/full-2026-09-13-aspire-1346/`. Committed snapshots were copied
to scratch directories when validation required building or running them.

## Environment

- .NET SDK: `10.0.401` (`9.0.200`, `9.0.318`, and `10.0.112` also installed)
- OS: Windows 11 Enterprise 64-bit, build 26200
- Docker: Docker Desktop 29.6.2, Linux containers
- Azure Developer CLI: 1.20.0
- Aspire CLI: 13.5.3
- Template versions:
  - `Microsoft.Extensions.AI.Templates` 10.10.0-preview.3.26459.2
  - SDK-provided MCP Server App template from .NET SDK 10.0.401
- Scope tested: Parts 1-11; Part 11 configuration-only, no Azure deployment
- AI access: Microsoft Azure AI Proxy with `gpt-5-mini` and
  `text-embedding-3-small`
- Repository commit: `6040eaf`

## Results

| Part | Status | Time | Notes |
| --- | --- | --- | --- |
| 1 - Setup | Pass | Not measured | SDK, templates, Docker, azd, and proxy credentials verified |
| 2 - Build Chat App | Pass | Not measured | Streaming, history, typed output, build, and audit passed |
| 3 - Add RAG | Pass | Not measured | Manual RAG, MEDI, and both checkpoint builds passed |
| 4 - AI Web Chat Template | Pass with issues | 25s first response | Qdrant path passed; Docker-free path fails on a string/Guid key mismatch |
| 5 - MCP Server Basics | Pass with issues | Not measured | Package/snapshot alignment fixed; one troubleshooting attribute name is wrong |
| 6 - Enhanced MCP Server | Pass with issues | Not measured | Tools pass; input validation, scenario coverage, and sample data have defects |
| 7 - MCP Publishing | Pass with issues | Not measured | Package claims fixed; documented `dnx --yes` option is unsupported |
| 8 - Agent Framework Basics | Pass with issues | Not measured | Tool use passed; model still offers unsupported notifications |
| 9 - Adding AI to an Existing App | Pass with issues | 5.9s grounded UI | Required flow passed; explicit HTTP profile and optional Step 3 remain incomplete |
| 10 - Choosing Providers and Services | Pass with issues | Not measured | Migration builds, but literal progression does not reproduce snapshot dependencies |
| 11 - Deployment | Pass with issues | Not measured | Configuration/builds pass; template baseline claims and snapshot drift remain |

Status: Pass / Pass with issues / Fail / Skipped.

## Part detail

### Part 1 - Setup

- **What was run:** Updated the repository with `git pull --ff-only`, then
  checked SDKs, AI and MCP templates, Docker client/server, Azure Developer CLI,
  and configured proxy variables.
- **Result:** All required tooling was available. Docker client and server were
  both 29.6.2. The proxy credentials worked for chat and embeddings.
- **Documentation clarity:** No new setup blocker was found.
- **Snapshot comparison:** Not applicable.
- **Snapshot updated:** No.

### Part 2 - Build Chat App

- **What was run:** Scaffolded a fresh console project, followed all package,
  secret, and code steps, built with warnings as errors, and tested streaming,
  multi-turn history, and typed structured output.
- **Result:** The assistant remembered the user's language preference and
  returned a typed conversation summary. The Release build had zero warnings
  and the vulnerability audit was clean.
- **Documentation clarity:** The instructions were complete.
- **Snapshot comparison:** Direct package versions matched the snapshot. Only
  generated identity/encoding differences remained.
- **Snapshot updated:** No.

### Part 3 - Add RAG

- **What was run:** Continued the nested Part 2 project, copied the sample
  document using the repository-root-aware path, built and ran manual RAG, then
  built and ran the MEDI/SQLite implementation. Built both checkpoint
  verification projects from scratch copies.
- **Result:** Manual RAG embedded 11 chunks, answered the boot-drying question,
  and refused an unrelated World Cup question. MEDI ingested the document,
  created `vectors.db`, returned a grounded answer, and refused the unrelated
  question. All builds and the vulnerability audit passed.
- **Documentation clarity:** The copy workflow and checkpoint roles remain
  clear.
- **Snapshot comparison:** Package versions and behavior matched the current
  checkpoints.
- **Snapshot updated:** No.

### Part 4 - AI Web Chat Template

- **What was run:** Installed the current template and scaffolded the exact
  Qdrant/Aspire command. Applied the connection-string/model changes, built in
  Release with warnings as errors, ran the AppHost with Docker, verified
  ingestion and citations, then scaffolded and tested the documented
  Docker-free local-vector variant.
- **Result:** The primary path passed:
  - Web app returned HTTPS 200.
  - Qdrant `v1.15.5` and markitdown became healthy.
  - Both sample documents ingested.
  - The first grounded answer completed in 25 seconds.
  - It identified the water filter and purification tablets and returned three
    citations into `Example_Emergency_Survival_Kit.pdf`.
  - The Release build had zero warnings and the audit was clean.
- **Docker-free result:** The project built and audited successfully, but the
  first interactive render failed:

  ```text
  The collection's generic key type is 'String', but the key property
  'Key' has type 'Guid'.
  ```

  The current local-vector template registers a string-keyed collection while
  the generated `IngestedChunk.Key` remains a `Guid`.
- **Documentation clarity:** The instructor notes say the current template
  scaffolds Aspire 13.4.6 throughout. The observed fresh scaffold was mixed:
  `Aspire.Hosting.AppHost` was 13.4.6, while `Aspire.AppHost.Sdk` and several
  other Aspire references remained 13.0.0. Part 4 also still pins
  SQLitePCLRaw 3.0.4 while the dependency policy identifies 3.0.5.
- **Snapshot comparison:** The Part 11 snapshot uses a coordinated 13.4.6
  Aspire set, but its AI, ingestion, tokenizer, and browser-library graph does
  not match current template output.
- **Snapshot updated:** No.

### Part 5 - MCP Server Basics

- **What was run:** Scaffolded with the SDK-provided MCP template, followed the
  README, built and audited, verified stdio waiting behavior, and sent real
  discovery and structured call requests.
- **Result:** The build had zero warnings. Calls passed for random number,
  current Seattle weather, and a five-day forecast. No vulnerable packages
  were found.
- **Documentation clarity:** The snapshot now correctly uses the template's
  `ModelContextProtocol` 2.1.0, and optional Visual Studio files are explained.
  One troubleshooting section refers to `[McpTool]`; the actual attribute is
  `[McpServerTool]`.
- **Snapshot comparison:** Project code, metadata, and package references
  matched. Differences were limited to optional `.mcp.json`/`.slnx` files and
  generated template README drift.
- **Snapshot updated:** No.

### Part 6 - Enhanced MCP Server

- **What was run:** Copied the snapshot to scratch, built and audited it, and
  exercised real discovery and calls for orders, customer history, inventory,
  partial matching, and missing values.
- **Result:** Build, schemas, and implemented structured calls passed.
- **Documentation clarity and data findings:**
  - Empty customer and product searches incorrectly return John Doe and Camping
    Tent because every string contains the empty string.
  - Scenario 4 asks for recent orders containing products, but no tool can
    search orders by product or enumerate recent orders.
  - Customer history exposes order `12350`, while
    `GetOrderDetails("12350")` reports it missing.
- **Snapshot comparison:** Scratch source matched the snapshot.
- **Snapshot updated:** No.

### Part 7 - MCP Publishing

- **What was run:** Applied package metadata to a scratch copy of Part 5 and ran
  local packing only. Inspected the base and RID package contents.
- **Result:** Seven packages were produced. The base package contained the
  nuspec, `.mcp/server.json`, consumer README, and
  `DotnetToolSettings.xml`, and declared both `DotnetTool` and `McpServer`.
- **Documentation clarity:** The corrected README now describes simulated
  Celsius weather, removes unsupported environment variables, avoids claiming
  a packaged LICENSE file, and requires replacing package placeholders.
  However, SDK 10.0.401's `dnx --help` exposes no `--yes` option. Several
  consumer commands include `--yes`, while the Visual Studio example does not.
- **Snapshot comparison:** Not applicable.
- **Snapshot updated:** No.

### Part 8 - Agent Framework Basics

- **What was run:** Scaffolded a fresh console app, added the documented
  packages/code, built and audited, and ran a live three-turn order
  conversation.
- **Result:** The agent invoked the tool, returned the expected order statuses,
  and retained context for "the first order." Package versions matched the
  snapshot and no vulnerabilities were found.
- **Documentation clarity:** The model again offered to set a notification even
  though no notification tool exists.
- **Snapshot comparison:** Only the generated user-secret ID and BOM differed.
- **Snapshot updated:** No.

### Part 9 - Adding AI to an Existing App

- **What was run:** Copied tracked starting files into scratch, verified baseline
  keyword behavior, completed Steps 1 and 2 literally, built and audited, ran
  all semantic queries, tested a stale vector, grounded/no-match service flows,
  AppHost resources, and the interactive Discovery page.
- **Result:** All required behavior passed:
  - `water` returned Rain Jacket, Water Bottle, and Daypack.
  - All six semantic queries matched documented ranking and filtering.
  - A stale Sleeping Bag vector was skipped without an exception.
  - Grounded UI completed in 5.96 seconds and showed three source cards.
  - The no-match UI returned the exact fixed response in 399 ms.
  - Products and Store were healthy under Aspire AppHost SDK/runtime 13.4.6.
- **Documentation clarity:** The explicit `http` AppHost profile still fails
  unless `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` is supplied. The optional
  telemetry/operations step still requires consulting the answer key for its
  complete implementation.
- **Snapshot comparison:** Required Steps 1/2 and package versions matched.
  Optional Step 3 was skipped because the configured local endpoint was
  unreachable; no model was downloaded.
- **Snapshot updated:** No.

### Part 10 - Choosing Providers and Services

- **What was run:** Applied the literal 13.4.6 Azure AI Search migration to the
  Part 4 scratch project, removed Qdrant packages/registrations, changed the
  collection key to string, built with warnings as errors, and audited.
- **Result:** The migration built with zero warnings. No Qdrant references
  remained and the fallback remains documented.
- **Documentation clarity:** Provider migration steps are internally valid.
- **Snapshot comparison:** The literal progression retains the fresh template's
  current AI/DataIngestion dependencies, while the Part 11 snapshot uses older
  pinned AI/DataIngestion versions and a direct `Microsoft.Bcl.Memory` override.
  Following Parts 4 and 10 therefore does not reproduce the full Part 11
  dependency graph.
- **Snapshot updated:** No.

### Part 11 - Deployment

- **What was run:** Confirmed connection-string OpenAI, Azure AI Search,
  external endpoints, references/waits, string collection keys, markitdown
  wiring, and absence of committed `azure.yaml`/`.azure`. Built and audited both
  the migrated scratch solution and a scratch copy of the committed snapshot.
- **Result:** Configuration checks, Release builds, and audits passed. No Azure
  deployment was performed.
- **Documentation clarity:** The snapshot consistently uses Aspire 13.4.6, but
  the statement that this is exactly what the current template emits throughout
  was not reproduced by the installed preview template.
- **Snapshot comparison:** Both trees contained all required chat JavaScript.
  Differences remain in AI/DataIngestion versions, tokenizer packages,
  `Microsoft.Bcl.Memory`, ServiceDefaults/OpenTelemetry, DOMPurify, generated
  identifiers, ports, and signed JavaScript bytes.
- **Snapshot updated:** No.

## Snapshot reconciliation

| Snapshot | Differences found | Resolution |
| --- | --- | --- |
| `Part 02 - Build Chat App/ChatApp/` | Only generated identity/encoding differences | Expected |
| `Part 03 - Add RAG/RagChatApp/` | Manual and MEDI references reproduce behavior | No change |
| `Part 05 - MCP Server Basics/MyMcpServer/` | Optional IDE files and generated README only | Alignment fix verified |
| `Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/` | Source matches; input/scenario/data defects remain | Fix sample and README |
| `Part 08 - Agent Framework Basics/AgentApp/` | Only generated user-secret ID/BOM | Expected |
| `Part 09 - Adding AI to an Existing App/eShopLite-start/` | Required flow passes; HTTP profile is incomplete | Fix paired profiles |
| `Part 09 - Adding AI to an Existing App/eShopLite/` | Required flow matches; optional Step 3 not independently reproducible | Complete or relabel Step 3 |
| `Part 11 - Deployment/GenAiLab/` | Aspire is 13.4.6, but template dependency and asset drift remains | Coordinated review needed |

## Issues

1. **Part 4 Docker-free variant fails on first interactive render**
   - Reproduce: scaffold the documented `--vector-store local` variant, apply
     credentials and SQLite override, build, run, and send the first chat.
   - Expected: local vector ingestion/search works without containers.
   - Actual: string collection key versus Guid `IngestedChunk.Key` exception.
   - Change: Part 4 local-vector instructions/template adaptation.

2. **Aspire 13.4.6 template-baseline claim does not match fresh output**
   - Reproduce: install the current AI template and inspect a new
     Qdrant/Aspire scaffold.
   - Expected: all Aspire SDK/package references are 13.4.6.
   - Actual: `Aspire.Hosting.AppHost` is 13.4.6, but the AppHost SDK and several
     other references remain 13.0.0.
   - Change: Part 4, instructor notes, or explicit package alignment steps.

3. **Part 4 SQLite override conflicts with dependency policy**
   - Reproduce: compare Part 4's 3.0.4 command with the policy's 3.0.5 entry.
   - Change: align and retest one version.

4. **Part 5 troubleshooting uses the wrong MCP attribute name**
   - Actual attribute: `[McpServerTool]`.
   - Documented troubleshooting name: `[McpTool]`.
   - Change: Part 5 README.

5. **Part 6 accepts empty searches as valid matches**
   - Reproduce: call customer or inventory search with an empty string.
   - Actual: returns the first record.
   - Change: validate input and return a structured no-match/validation result.

6. **Part 6 scenarios and sample order data exceed the tool surface**
   - Scenario 4 has no product-to-order/recent-order tool.
   - Order `12350` appears in customer history but cannot be retrieved by ID.
   - Change: tools, data, tests, and README.

7. **Part 7 documents an unsupported `dnx --yes` option**
   - Reproduce: run `dnx --help` with SDK 10.0.401.
   - Actual: no `--yes` option is listed.
   - Change: remove the flag or document the SDK/tool version that supports it.

8. **Part 8 agent offers unsupported notification actions**
   - Reproduce: ask multi-turn order status questions.
   - Actual: model may offer to set a notification without a corresponding
     tool.
   - Change: strengthen agent capability instructions.

9. **Part 9 explicit HTTP profile is incomplete**
   - Reproduce:
     `dotnet run --project eShopLite.AppHost --launch-profile http`.
   - Actual: Aspire requires `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`.
   - Change: both start and answer launch profiles.

10. **Part 9 optional Step 3 requires the answer key**
    - The telemetry and operations implementation is not fully provided.
    - Change: provide complete code or identify the section as exploration.

11. **Literal Parts 4/10 do not reproduce the Part 11 dependency graph**
    - Current template-derived AI/DataIngestion dependencies differ from the
      snapshot, including its direct security override.
    - Change: decide which graph is intentional and reconcile the full
      progression.

## Recommended documentation improvements

- Fix the Docker-free key type before advertising it as the no-container path.
- Correct or qualify the claim that the current template emits Aspire 13.4.6
  throughout; alternatively add explicit alignment steps.
- Align the SQLite override with the dependency policy.
- Correct `[McpTool]` to `[McpServerTool]`.
- Add empty-input validation and make Part 6 scenarios/data consistent with the
  exposed tools.
- Remove or validate Part 7's `dnx --yes` flag.
- Constrain the Part 8 agent to implemented capabilities.
- Fix both Part 9 HTTP profiles and make Step 3 complete or explicitly
  exploratory.
- Reconcile the current template-derived Parts 4/10 dependency graph with the
  Part 11 snapshot while preserving its launch settings and required browser
  JavaScript.

## Summary

The required workshop path remains operational on .NET 10.0.401. Live chat,
manual RAG, MEDI/SQLite ingestion, Qdrant and markitdown, grounded PDF citations,
MCP structured tools, local packaging, Agent Framework, eShopLite semantic
search and grounded discovery, Azure AI Search migration, and Part 11
configuration all passed.

The Part 5 package-alignment and Part 7 consumer-documentation fixes are
verified. The most important new regression is the Docker-free Part 4 runtime
failure. The Aspire 13.4.6 snapshot builds and runs, but the installed preview
template did not emit a uniformly 13.4.6 Aspire graph, so the template-baseline
claim and Parts 4/10/11 dependency progression still need reconciliation.
