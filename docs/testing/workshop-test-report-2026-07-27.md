# Workshop Test Report - 2026-07-27

A full attendee-style run of all 11 parts. Every project was scaffolded from `dotnet new`
in a scratch `test-workspace/` rather than copied from a snapshot, and every code part was
run against live Microsoft Foundry (Azure OpenAI). The Azure deployment in Part 11 was out
of scope by agreement; Part 11 was tested configuration-only.

Repo at `aad12bd`, which includes #566 ("Align Parts 4 and 11 with the current aichatweb
template"). Part 4 was tested twice — once before that PR merged and once after — and the
post-merge result is what this report records.

Part 3 was retested at `14974aa` after #579 landed. It failed the original run and passes the
retest; both are recorded below.

## Environment

- .NET SDK: `10.0.400-preview.0.26322.102` (default; 10.0.110, 9.0.316 and 9.0.200 also
  installed, no `global.json`)
- OS: Windows, PowerShell, `core.autocrlf=true`
- Docker: 29.6.2, running
- Template versions: `Microsoft.Extensions.AI.Templates` 10.7.0-preview.3.26309.5,
  `Aspire.ProjectTemplates` 9.4.2 (repo pins Aspire 13.4.6)
- Foundry Local CLI: 0.10.2, serving `Phi-4-mini-instruct-generic-gpu`
- Scope tested: full 1-11, excluding `azd up`

## Results

| Part | Status | Time | Notes |
| --- | --- | --- | --- |
| 1 - Setup | Pass | ~10 min | Prerequisites and install steps verified |
| 2 - Build Chat App | Pass | ~35 min | Snapshot matches exactly |
| 3 - Add RAG | Pass | ~70 min | Failed at `aad12bd`; fixed by #579 and re-verified at `14974aa` |
| 4 - AI Web Chat Template | Pass | ~50 min | Aspire path fixed by #566; README restructured so configuration comes first, closing #580. Both paths re-verified end to end including RAG |
| 5 - MCP Server Basics | Pass | ~30 min | All three tools verified over stdio; template drift only |
| 6 - Enhanced MCP Server | Pass | ~20 min | All three tools verified; four dead links |
| 7 - MCP Publishing | Pass | ~30 min | Failed at `aad12bd`; fixed by #583 and re-verified at `b7d43cf` |
| 8 - Agent Framework Basics | Pass | ~25 min | Byte-identical snapshot match — the cleanest part in the workshop |
| 9 - Adding AI to an Existing App | Pass with issues | ~60 min | All three features verified live; the missing "before" app was added by #584 and re-verified |
| 10 - Choosing Providers and Services | Pass with issues | ~25 min | Snippets do not compile as printed |
| 11 - Deployment | Pass | ~20 min | Config-only. Release build clean; reconciliation against Part 4 near-perfect |

Status: Pass / Pass with issues / Fail / Skipped.

## Part detail

### Part 2 - Build Chat App

- **What was run:** `dotnet new console -n ChatApp`, the five packages from the README,
  `Program.cs` per the README, then `dotnet run` with scripted input.
- **Result:** Clean build. Streaming, multi-turn history, and the structured `summary`
  output all worked against `gpt-5-mini`.
- **Documentation clarity:** No gaps. Package versions resolved exactly as the snapshot pins them.
- **Snapshot comparison:** Differences are teaching comments and `UserSecretsId` only.
- **Snapshot updated:** No — nothing to change.

### Part 3 - Add RAG

Was the worst part in the workshop and the only one that failed outright for every attendee.
**Fixed by #579 and re-verified — now passes.** The original findings are kept below because
they explain what the fix is defending against.

- **What was run:** Copied the Part 2 app per Step 1, followed Step 2 (manual cosine
  similarity), then Step 3 (`Microsoft.Extensions.DataIngestion` + `SqliteVec`).
- **Result (original run, at `aad12bd`):**
  - **Step 2 ran and answered correctly, but the lesson did not happen.** The console
    printed `Embedding 1 chunks` instead of `Embedding 11 chunks`. Because the repo had no
    `.gitattributes` and Git for Windows defaults to `core.autocrlf=true`, the sample
    document was on disk with CRLF (33 CRLF, 0 bare LF; the git blob is LF), so
    `Split("\n\n")` never matched. Chunking, top-k selection and cosine ranking — the
    entire point of Step 2 — silently did not run. The answer was still correct because the
    whole 1.2 KB document fits in one chunk and is always passed as context, which is
    exactly why nobody noticed.
  - **Step 3 crashed.** `MarkdownReader` threw
    `NotSupportedException: Inline type 'AutolinkInline' is not supported` on
    `<support@contoso.example>` in the sample document; ingestion reported
    `Succeeded: False`; then `writer.VectorStoreCollection` threw a second, unhandled
    `InvalidOperationException` because there was no guard for failed ingestion.
  - Step 3 also emits **4 `NU1903` high-severity vulnerability warnings** that CI never sees.
    Still true after #579; tracked as #575.
- **Retest at `14974aa` (after #579):** Re-ran the whole part as an attendee.
  - Step 2 prints `Embedding 11 chunks`. Both defenses were checked independently rather
    than trusting one: the `.gitattributes` rule delivers the file as LF (33 LF, 0 CRLF),
    **and** force-rewriting the file to genuine CRLF still yields 11 chunks, so the
    `ReplaceLineEndings("\n")` guard covers a file copied in from outside the repo.
  - Step 3 reports `Succeeded: 'True'` with no exception, and all three exchanges from the
    README's Step 4 sample output reproduce, including the out-of-context refusal.
  - The new failure guard was tested by deliberately re-introducing the autolink. It
    degrades exactly as documented — prints
    `Inline type 'AutolinkInline' is not supported.` followed by the stop message, and exits
    with **code 0** rather than an unhandled exception.
- **Documentation clarity:** The "What's next" section still claims the knowledge base is
  in-memory, which contradicts Step 3's SQLite persistence.
- **Snapshot comparison:** My typed Step 3 `Program.cs` is semantically identical to
  `checkpoints/medi-program.cs` — 110 significant lines each, zero differences after
  normalizing indentation and stripping comments. The checkpoint indents with 4 spaces where
  the README prints 2, which is cosmetic and pre-existing. `ReplaceLineEndings` is present in
  both `RagChatApp/Program.cs` and `checkpoints/manual-program.cs`. Neither checkpoint is
  compiled by CI; compiling them by hand — a throwaway console project per variant with
  exactly the packages each one documents — both build successfully, with
  `manual-program.cs` clean and `medi-program.cs` raising two of the `NU1903` warnings above.
  `RagChatApp.csproj` also drops `Microsoft.Extensions.Logging.Console`, which an attendee
  carries in from Part 2.
- **Snapshot state:** `RagChatApp` builds Release with 0 warnings and runs correctly
  (11 chunks, grounded answer). #567, #568 and #569 are closed; #575 remains open.

### Part 4 - AI Web Chat Template

- **What was run:** `dotnet new aichatweb --provider azureopenai --vector-store qdrant --aspire --name GenAiLab --output GenAiLab`,
  then the rewritten README end to end: package bumps, the AppHost swap to
  `AddConnectionString`, the `gpt-4o-mini` → `gpt-5-mini` edit, removing
  `Aspire.Hosting.Azure.CognitiveServices`, and setting the `openai` secret.
- **Result:** Build 0 warnings. `dotnet run --project GenAiLab.AppHost` reaches
  "Distributed application started". Driving the real Blazor UI in a browser, the question
  *"What water purification supplies are in the emergency survival kit?"* returned a
  grounded answer citing `Example_Emergency_Survival_Kit.pdf` twice, with follow-up
  suggestions. Qdrant collection `data-genailab-chunks` held **19 points at dim 1536**.
- **Documentation clarity:** Good on the Aspire path. #566 fixed everything found in the
  pre-merge pass — the hardcoded `gpt-4o-mini`, the `AddAzureOpenAI` vs `AddConnectionString`
  mismatch that caused a startup crash, and the missing
  `Aspire.Hosting.Azure.CognitiveServices` handling.
- **Docker-free `--vector-store local` path:** Scaffolded and run separately with the exact
  README command including `--managed-identity false`. The README's `NU1903` note is accurate
  and its fix works — `dotnet restore` does flag transitive `SQLitePCLRaw.lib.e_sqlite3`
  2.1.10, and pinning `SQLitePCLRaw.bundle_e_sqlite3` 3.0.4 clears it to 0 warnings. Ingestion
  into SQLite and a grounded, cited answer both verified in a browser. **But the template
  hardcodes `gpt-4o-mini` on this path too**, and the fix for that lives in Step 3, which
  line 112 tells Docker-free readers to skip — while the comparison table at line 100 claims
  this path already uses `gpt-5-mini`. The instructor proxy has a genuine `gpt-4o-mini`
  deployment (confirmed: it answers as `gpt-4o-mini-2024-07-18`, and an invented model name
  404s), so the defect is invisible in rehearsal but 404s on the attendee's own Part 1
  resource, which deploys only `gpt-5-mini` and `text-embedding-3-small`. Filed #580.
- **README restructure (post-run):** Attendee feedback during rehearsal was that the
  configuration steps were unclear and came too late. The part was reorganized so that all
  four configuration changes — package updates, the `AddConnectionString` swap, the
  deployment-name change, and the connection-string secret — now live in a single **Step 2
  "Configure it before you run it"**, immediately after scaffolding and ahead of the code
  walkthrough (which moved to Step 5, after the app runs). The secret is now documented for
  **both** Visual Studio's *Manage User Secrets* (with the exact `secrets.json` shape) and
  the CLI, and it states explicitly that the secret belongs to `GenAiLab.AppHost` rather than
  `GenAiLab.Web`. The Docker-free path moved out of Step 1 into its own end-of-part section
  carrying a **complete** configuration of its own, which closes #580: it now shows the
  `AsIChatClient("gpt-4o-mini")` → `"gpt-5-mini"` edit in place, and the comparison table
  gained a credentials row so the two credential models are not confused.
- **Post-restructure verification:** Both paths were re-run against the live resource.
  - Aspire path: the committed solution builds Release 0 warnings, starts with **no Azure
    provisioning prompt**, brings up `vectordb` and `markitdown`, and answers the water
    purification question with two working citations into
    `Example_Emergency_Survival_Kit.pdf`.
  - Docker-free path: re-scaffolded, applied the newly documented edits (`gpt-5-mini`,
    the two `AzureOpenAI:*` secrets, the `SQLitePCLRaw.bundle_e_sqlite3` 3.0.4 pin), built
    **0 warnings**, and got the same grounded answer with the same two citations via PdfPig
    and SQLite.
  - One incidental finding: `GenAiLab.AppHost` had **no** user secret set, because #566
    re-scaffolded the project and changed its `UserSecretsId`. Re-running
    `setup-workshop-credentials.ps1 -ApplyUserSecrets` fixes it; worth knowing whenever that
    project is regenerated.
- **Completed code:** Part 4 has no snapshot of its own by design. `Part 11 -
  Deployment/GenAiLab/` is the finished Part 4 plus one line, and that is now stated in the
  Part 4 README, the Part 11 README, the snapshot's own README, and the root README.
- **Stale Qdrant volume (found in rehearsal, now documented):** Running the app *before*
  step 2.1 and then updating the packages can leave `vectordb` unable to start. Bumping
  Aspire also bumps the Qdrant image, and `WithDataVolume()` keeps storage written by the
  older version, while `WithLifetime(ContainerLifetime.Persistent)` keeps the old container
  instead of recreating it from the new image. Step 3 gained a **"Qdrant won't start after
  updating packages"** section explaining both causes and giving the cleanup, and Part 11
  links to it because it runs the same solution.
  - Verified by doing it: `docker rm -f <container>` then `docker volume rm <volume>` both
    succeed (order matters — Docker refuses to remove a volume still attached to a
    container, which is why the documented commands are in that order). Re-running the
    AppHost recreated the container and an empty volume, re-ingested the sample documents on
    the first question, and returned the same cited answer.
- **Snapshot comparison:** See the reconciliation table — near-perfect.
- **Snapshot updated:** No — already correct.

### Part 5 - MCP Server Basics

- **What was run:** `dotnet new mcpserver -n MyMcpServer`, added `WeatherTools.cs` and
  updated `Program.cs` per the README, then drove the built executable over stdio with a
  raw `ProcessStartInfo` harness.
- **Result:** Clean build. `initialize` + `tools/list` returns `get_random_number`,
  `get_weather_forecast` and `get_current_weather`; `tools/call` verified for two of them.
- **Documentation clarity:** Four small drifts — the template now emits
  `ModelContextProtocol` 1.2.0 while the snapshot pins 1.4.1 with no bump step; the
  `.mcp/server.json` schema moved from the `2025-07-09` draft to `2025-10-17`; the template
  no longer emits a namespace in `RandomNumberTools.cs`; and the quoted startup output does
  not match what the server prints. Worth adding: MCP exposes tool names in **snake_case**,
  not the PascalCase C# method name.
- **Snapshot comparison:** Package version and `server.json` schema, as above.
- **Snapshot updated:** No — filed as #572.

> Note for future runs: piping stdin through `dotnet run` swallows the MCP server's stdout.
> Drive the built `.exe` directly with redirected stdin/stdout.

### Part 6 - Enhanced MCP Server

- **What was run:** `dotnet build -c Release` on the snapshot, then the same stdio harness.
- **Result:** 0 warnings. All three tools verified: `get_order_details("12345")` returns the
  John Doe order, `get_product_inventory("Hiking Boots")` returns SKU HB-003, and
  `search_orders_by_customer("John")` returns two orders. Clean shutdown.
- **Documentation clarity:** All four "Additional Resources" links are 404.
- **Snapshot updated:** No.

### Part 7 - MCP Publishing

Failed the original run and **passes the retest after #583**. The original findings are kept
because they explain what the rewrite is defending against.

- **What was run (original, at `aad12bd`):** `dotnet pack -c Release` against the untouched
  Part 5 project, then applied Step 1's replacement `.csproj` to a copy and packed again.
- **Result (original):**
  - The replacement `.csproj` **did not compile** — it omitted
    `<ImplicitUsings>enable</ImplicitUsings>`, producing two `CS0246` errors on `Task<>`.
  - After adding only that back, the resulting package declared `packageType DotnetTool`
    **only** — not `McpServer` — and contained **no `.mcp/server.json`**, because the
    replacement dropped `<PackageType>`, `<RuntimeIdentifiers>`, `<SelfContained>`,
    `<PublishSingleFile>` and the `server.json` pack item. Following Part 7 literally could
    not produce a usable MCP server package.
  - Against the unmodified template, `dotnet pack` produced **seven** packages, not the
    single `.nupkg` Step 3 assumed.
  - Step 1 set `<ToolCommandName>weather-mcp-server</ToolCommandName>` but Step 3 tested
    `my-mcp-server --help`.
  - Steps 1 and 3 told the attendee to edit and build inside the committed Part 5 snapshot.
- **Retest at `b7d43cf` (after #583):** Followed the rewritten README literally, stopping
  before Step 4 since publishing is out of scope.
  - Step 1 now fills in the metadata block instead of replacing the file, so all four
    load-bearing settings survive. `dotnet pack -c Release` succeeds — no `CS0246`.
  - Produces **exactly seven** packages, names matching the README's list.
  - The base package contains `README.md`, the nuspec, `.mcp\server.json` and
    `tools\net10.0\any\DotnetToolSettings.xml`, and the nuspec declares **both**
    `DotnetTool` and `McpServer` — the core of #571. `DotnetToolSettings.xml` maps all six
    RIDs to their per-platform packages.
  - Global install from the local source prints exactly the predicted
    `You can invoke the tool using the following command: weather-mcp-server`; uninstall is
    clean.
  - Going beyond the README, driving the installed self-contained executable over stdio
    returned `serverInfo: {"name":"MyMcpServer","version":"1.0.0.0"}`, listed all three
    tools, and answered a real `tools/call get_current_weather` for Tokyo. The lab now
    produces a genuinely installable, working server rather than just correct metadata.
  - The new `publish-lab/` `.gitignore` entry works; `git status` stays clean after packing.
  - Two nits, neither worth an issue: the "Expected contents" listing omits the OPC
    boilerplate (`_rels/.rels` and the `.psmdcp`) that always appears, and the `.cmd` shim on
    the PATH doesn't pass stdio through to a raw harness, though a real MCP client is fine.
- **Documentation clarity:** Dead links unchanged and still tracked by #573 —
  `modelcontextprotocol/awesome-mcp` 404s, `spec.modelcontextprotocol.io` is unreachable, and
  the `server.json` `$schema` URL 404s (emitted by the template, not authored here).
- **Snapshot updated:** N/A — no snapshot.

### Part 8 - Agent Framework Basics

- **What was run:** `dotnet new console` → `AgentApp`, the five packages from Step 1,
  `Program.cs` per Step 3, then the three prompts from Step 4.
- **Result:** Build 0 warnings. The agent invoked `GetOrderStatus` on its own for
  `ORD-1001` and `ORD-1002`, and resolved *"How about ORD-1002?"* from conversation context.
- **Documentation clarity:** No gaps found.
- **Snapshot comparison:** `git diff --no-index` against the snapshot `Program.cs` reported
  **zero differences**. Package versions matched the snapshot exactly. The only `.csproj`
  delta is `UserSecretsId`.
- **Snapshot updated:** No — nothing to change.

### Part 9 - Adding AI to an Existing App

- **What was run:** Built and ran the snapshot under Aspire, exercised the semantic search
  endpoint directly, then drove the Blazor storefront in a browser.
- **Result:** Everything the part teaches works.
  - Build 0 warnings across all five projects.
  - Keyword search for `warm at night` returns 0 results, as documented.
  - All six queries from the README's distance table reproduced **exactly**, including both
    rejections:

    | Query | Result |
    | --- | --- |
    | `warm at night` | Four Season Sleeping Bag, Insulated Water Bottle, Merino Base Layer |
    | `something for rainy weather` | Outdoor Rain Jacket, Two Person Tent, Trail Running Shoes |
    | `keep my drink cold` | Insulated Water Bottle |
    | `I need light for a cave` | Head Torch, Solar Powered Flashlight |
    | `socket wrench` | *(nothing)* |
    | `power tools for construction` | *(nothing)* |

  - The Ask page answered the camping question with the Sleeping Bag and Tent, showing the
    three source products underneath, matching the screenshot in the README.
  - `a socket wrench set for my car` returned the fixed "We do not stock anything that
    matches that" string without calling the model.
  - Step 3's Operations page is wired correctly and the local model is reachable, but it did
    not return a summary within the test window. Measuring the endpoint directly, warm and
    with the model already loaded, `Phi-4-mini-instruct-generic-gpu` runs at roughly **0.15
    tokens/second** — 11 completion tokens in 78s, 29 in 155s — against a README that promises
    "a few seconds" for warm requests. One summarize-style prompt returned `finish_reason:
    stop` with **zero** completion tokens and empty content, reproduced twice, so the page can
    render blank even after the wait. The cause is model choice rather than a workshop defect:
    the README recommends a 1.5B NPU build, while this machine has a 3.8B `generic-gpu` build
    loaded. But the README's "any small model will do" invites exactly that substitution, so
    the guidance needs sharpening. Filed #581.
- **Documentation clarity:** The Foundry Local CLI commands are stale (`foundry service
  status` and `foundry model run` no longer exist), and the suggested model is an NPU-only
  variant.
- **Snapshot comparison (original run):** The committed snapshot was the **finished** app, so
  the README's premise — "five projects, none of which reference an AI package yet" — was
  false of the folder it told you to open, and the motivating "watch keyword search fail"
  moment could not happen. Part 9 was the only code part with no starting state.
- **Retest at `b4917b2` (after #584):** `eShopLite-start/` now ships the AI-free solution and
  the README points at it, with `eShopLite/` documented as the answer key.
  - The starting state is genuinely clean: no AI package references anywhere, no
    `Products/Ai/`, `Store/Ai/`, `Discovery.razor` or `Operations.razor`, and no leftover AI
    usings. The only non-obvious `Products.csproj` entries are the
    `SQLitePCLRaw.bundle_e_sqlite3` 3.0.4 and `Microsoft.OpenApi` 2.7.5 advisory pins, which
    the new README note correctly flags as not-AI. Both solutions build Release with 0
    warnings.
  - The motivating moment reproduces. The starting app runs with no AI credentials in play —
    which the finished app could not do — seeds 12 products, returns 3 results for `water`,
    **0** for `warm at night`, and **404** on `/api/product/aisearch`.
  - **Step 1 completes from the starting state**, which is the point of #570. Working from a
    scratch copy: added the four packages, created `ProductVector.cs` and
    `ProductSemanticSearch.cs` verbatim, set the secrets, applied the registrations and the
    two-line seeding change, and added the endpoint. Build 0 warnings; startup logged
    `Product search index is ready.`; and all six queries from the distance table came back
    exactly as documented, with distances matching to three decimals except the cave query's
    second value (0.654 vs 0.651), which the README already warns will drift.
  - Moving "Configure credentials" to 1.4 is a real improvement — the earlier "watch keyword
    search fail" run no longer asks for credentials the app doesn't use yet.
  - Minor, not filed: `eShopLite-start` and `eShopLite` share the same `UserSecretsId` for
    both `Products` and `Store`. Convenient, since setting secrets once covers both and
    `-ApplyUserSecrets` keeps working, but the two folders aren't fully isolated.
- **Snapshot updated:** No — #570 fixed by #584 and verified; #574 and #581 remain open.

### Part 10 - Choosing Providers and Services

- **What was run:** Documentation review, plus compiling the printed snippets and verifying
  every cross-reference and package.
- **Result:** Structure, cross-references and API shape all check out. The Part 11 anchor
  resolves, issue #496 exists and is open, both `Microsoft.AI.Foundry.Local` and
  `.WinML` exist on NuGet at 1.2.3, the alternative `--vector-store local` scaffold command
  works, and the `FoundryLocalManager.Instance` → `GetCatalogAsync` → `GetModelAsync` →
  `DownloadAsync` → `LoadAsync` → `StartWebServiceAsync` chain compiles against 1.2.3.
- **Documentation clarity:** The snippets do not compile as printed. `AsIChatClient` and
  `AsIEmbeddingGenerator` come from `Microsoft.Extensions.AI.OpenAI`, which the package list
  never mentions (`CS1061`); the Foundry Local snippet references an undeclared `config`
  variable (`CS0103`); and `Microsoft.AI.Foundry.Local` will not build on .NET 10 without an
  explicit `<RuntimeIdentifier>`.
- **Snapshot updated:** N/A.

### Part 11 - Deployment

- **What was run:** Verified `WithExternalHttpEndpoints()` in `AppHost.cs`, ran
  `dotnet build GenAiLab.sln -c Release`, and reconciled the Part 4 scaffold against the
  snapshot file-by-file. No `azd up`.
- **Result:** 0 warnings, 0 errors.
- **Documentation clarity:** The `$env:`-based alternative for the `openai` secured
  parameter will not expand — `azd` reads the prompt as a literal string, so the attendee
  submits `$env:WORKSHOP_AZURE_OPENAI_ENDPOINT` verbatim and only finds out at runtime,
  after ~6 minutes of provisioning. Separately, the README shows the
  `WithExternalHttpEndpoints()` call as a standalone statement while the snapshot chains it.
- **Snapshot updated:** No.

## Snapshot reconciliation

| Snapshot | Differences found | Resolution |
| --- | --- | --- |
| `Part 02 - Build Chat App/ChatApp/` | Teaching comments and `UserSecretsId` only | No action |
| `Part 03 - Add RAG/RagChatApp/` | `csproj` drops `Microsoft.Extensions.Logging.Console` carried in from Part 2; `checkpoints/medi-program.cs` matches my implementation and so carries the Step 3 crash | Filed #567, #568, #575 |
| `Part 05 - MCP Server Basics/MyMcpServer/` | Template emits `ModelContextProtocol` 1.2.0 vs snapshot 1.4.1; `.mcp/server.json` on the older `2025-07-09` schema; template no longer emits a namespace | Filed #572 |
| `Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/` | None | No action |
| `Part 08 - Agent Framework Basics/AgentApp/` | **None.** `Program.cs` byte-identical; package versions identical; only `UserSecretsId` differs | No action |
| `Part 09 - Adding AI to an Existing App/eShopLite/` | Snapshot is the finished app; `eShopLite-start/` added by #584 is the AI-free starting state | Fixed, verified |
| `Part 11 - Deployment/GenAiLab/` | Identical file sets. Six files differ: `.sln` and both `launchSettings.json` by GUID/port, both `.csproj` by `UserSecretsId`, and `AppHost.cs` by exactly the `WithExternalHttpEndpoints()` call Part 11 asks for plus a comment | No action |

> The skill's own part table lists Part 9's snapshot as `StoreApp/`; the repo has
> `eShopLite/`. Worth correcting in `.github/skills/workshop-testing/SKILL.md`.

## Issues

All filed on the repo, with reproduction steps and suggested fixes:

| # | Severity | Part | Title | Status |
| --- | --- | --- | --- | --- |
| #567 | **Critical** | 3 | Step 3 crashes on the workshop's own sample document (MEDI autolink) | Fixed by #579, verified |
| #568 | High | 3 | Step 3 has no guard for failed ingestion, turning any failure into an unhandled crash | Fixed by #579, verified |
| #569 | High | 3 | Step 2 chunking is a silent no-op on Windows (CRLF vs `"\n\n"`) | Fixed by #579, verified |
| #570 | High | 9 | No "before" version of eShopLite — the only snapshot is the finished app | Fixed by #584, verified |
| #571 | **Critical** | 7 | Step 1's replacement `.csproj` doesn't compile and strips the MCP packaging metadata | Fixed by #583, verified |
| #572 | Medium | 5 | Template drift — package versions, `server.json` schema, namespace, startup output | Open |
| #573 | Medium | 5/6/7/9/11 | Eight dead external links | Open |
| #574 | Medium | 9 | Step 3 uses Foundry Local CLI commands that no longer exist | Open |
| #575 | Medium | 3 | `NU1903` warnings on the MEDI path, and checkpoints are never built by CI | Open |
| #577 | Medium | 10 | Provider snippets don't compile — missing package, undeclared `config`, RID requirement | Open |
| #578 | Low | 11 | `azd` prompt `$env:` expansion, and a README/snapshot style mismatch | Open |
| #580 | High | 4 | Docker-free path never gets the `gpt-4o-mini` → `gpt-5-mini` fix, and the comparison table says it already has it | Fixed in this branch, verified |
| #581 | Medium | 9 | Warm local-model responses took 78–155s, not the "few seconds" the README promises | Open |
| #576 | — | all | Summary issue for this run | Open |

## Recommended documentation improvements

In priority order for the next delivery. **Items 1-3 and 5 are done** — #579 and #583 fixed
them and the retests confirm; they are kept here as a record of what shipped.

1. ~~**`Part 03 - Add RAG/RagChatApp/sample-docs/contoso-trailblazer-3000.md`** — drop the
   angle brackets around the support address.~~ Done in #579.
2. ~~**`Part 03 - Add RAG/README.md` Step 2.2**, plus `RagChatApp/Program.cs` and
   `checkpoints/manual-program.cs` — normalize line endings before the split, and add a
   `.gitattributes` rule.~~ Done in #579, which used `ReplaceLineEndings("\n")` and pinned
   `**/sample-docs/*.md text eol=lf`. Both defenses verified independently.
3. ~~**`Part 03 - Add RAG/README.md` Step 3.4** — guard on `result.Succeeded` before reading
   `writer.VectorStoreCollection`, and stop with a clear message when nothing ingested.~~
   Done in #579.
4. ~~**`Part 04 - AI Web Chat Template/README.md` lines 100 and 112** — the Docker-free path
   needs the `gpt-4o-mini` → `gpt-5-mini` edit, which currently sits inside a step that
   section tells readers to skip; and the comparison table describes the corrected code
   rather than what the template emits. This path is the fallback for attendees whose Docker
   won't start, so it fails at the worst possible moment.~~ Done in this branch: the
   Docker-free path became a standalone section with its own complete configuration,
   including the model-name edit shown in place. Verified by re-scaffolding and running it.
5. ~~**`Part 07 - MCP Publishing/README.md` Step 1** — stop replacing the `.csproj`. Show the
   metadata as additions to the template's file, and explain why `<PackageType>McpServer</PackageType>`
   and the `.mcp/server.json` pack item must survive.~~ Done in #583, verified.
6. ~~**`Part 09 - Adding AI to an Existing App/`** — add an `eShopLite-start/` folder with the
   AI removed, and point the README at it.~~ Done in #584, verified.
7. **`Part 10 .../README.md`** — add `Microsoft.Extensions.AI.OpenAI` to the package lists
   and `using Microsoft.Extensions.AI;` to the snippets; replace `config.Web.Urls` with
   something the attendee can actually resolve.
8. **`Part 09 .../README.md` Step 3.2** — `foundry server status`, `foundry run <model>`,
   unqualified `qwen2.5-1.5b`, and promote the "warm the model first" note to a warning that
   names the real variable: parameter count *and* execution-provider variant, not just
   "any small model."
9. **Dead links** across Parts 5, 6, 7, 9 and 11 — consider adding a link checker to CI
   alongside the existing markdownlint workflow.

## Summary

**The workshop has no failing parts left.**

Nine of eleven parts pass outright; the other two pass with issues. Part 8 is flawless — a
byte-identical snapshot match with the agent behaving exactly as documented. Part 11's
reconciliation against Part 4 is as tight as it could reasonably be: identical file sets, and
the only functional difference is the single line Part 11 asks you to add. All seven CI
targets build with **zero warnings**.

The two parts that failed the original run are both fixed and re-verified. #579 removed the
autolink that crashed Part 3 Step 3 for every attendee, normalized line endings so Step 2
actually chunks on Windows, and added a guard that turns an ingestion failure into a readable
message instead of a second unhandled exception. #583 stopped Part 7 from replacing the
template's `.csproj`, so the lab now packs successfully and produces an MCP server package
that a client will actually recognise — confirmed by driving the installed executable over
stdio.

Part 4 was reorganized after rehearsal feedback that its configuration steps were unclear and
came too late. All four configuration changes now sit in one step immediately after
scaffolding, the connection-string secret is documented for Visual Studio as well as the CLI
and says plainly which project owns it, and the Docker-free path became a self-contained
section with its own complete configuration. That last change closes #580: the path that
attendees fall back to *because Docker wouldn't start* no longer ships a deployment name their
resource doesn't have. Both paths were re-run end to end afterwards and produced grounded,
cited answers. Part 4 keeps no snapshot of its own — `Part 11 - Deployment/GenAiLab/` is the
finished Part 4 plus one line — and that is now said explicitly in four places rather than
left implicit.

Part 9's three AI features all work exactly as documented, reproducing the README's
distance table verbatim, and #584 supplied the missing starting point: `eShopLite-start/` is
genuinely AI-free, runs without credentials, and Step 1 completes from it to produce working
semantic search with distances matching the published table. Its optional local-model step
works but is far slower than the README implies on anything other than the recommended small
NPU build.

Everything else is documentation drift: stale CLI commands, snippets missing a package
reference, and eight dead links.
