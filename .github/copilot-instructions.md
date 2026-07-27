# .NET AI Workshop

Teaching repository for a one-day .NET AI workshop. Content is **11 self-contained parts**, each a top-level `Part NN - <Name>/` folder containing a `README.md` (the lab instructions) and, where applicable, a working code snapshot. Folder names are zero-padded so they sort correctly in the GitHub web UI; prose and headings use the unpadded "Part N" form.

Everything targets **.NET 10** and uses **Microsoft Foundry (Azure OpenAI)** as the default provider.

## Parts and code snapshots

| Part | Snapshot project | Notes |
| --- | --- | --- |
| 01 - Setup | — | README only |
| 02 - Build Chat App | `ChatApp/` | Console app built by hand (`dotnet new console`) |
| 03 - Add RAG | `RagChatApp/` | Continues from Part 2; `checkpoints/` holds two alternate `Program.cs` paths (manual cosine similarity, and MEDI + SqliteVec). `checkpoints/verify/` holds compile-only projects so CI type-checks both checkpoints — the snapshot itself is the Step 2 manual path |
| 04 - AI Web Chat Template | — | README only; scaffolds `GenAiLab` with `dotnet new aichatweb` |
| 05 - MCP Server Basics | `MyMcpServer/` | `dotnet new mcpserver` (template from `Microsoft.McpServer.ProjectTemplates`); `RandomNumberTools` + `WeatherTools` |
| 06 - Enhanced MCP Server | `ContosoOrdersMcpServer/` | Optional/bonus. Exploration of an existing snapshot — the README does **not** ask the user to scaffold it |
| 07 - MCP Publishing | — | Optional/bonus, README only |
| 08 - Agent Framework Basics | `AgentApp/` | `dotnet new console` + `Microsoft.Agents.AI` |
| 09 - Adding AI to an Existing App | `eShopLite-start/` and `eShopLite/` (5-project Aspire solutions) | Capstone. `eShopLite-start/` is the AI-free starting point attendees work in; `eShopLite/` is the finished answer key. The workshop adds semantic search, grounded discovery, and a local-model assistant. Added code lives in `Products/Ai/` and `Store/Ai/` |
| 10 - Choosing Providers and Services | — | README only. Sits immediately before deployment because provider and service selection is a deployment prerequisite |
| 11 - Deployment | `GenAiLab/` (3-project Aspire solution) | Same app as Part 4 plus `WithExternalHttpEndpoints()` in `AppHost.cs` |

Other folders: `docs/` (instructor guides, planning, archived test reports), `images/` (screenshots used by workshop instructions), `manuals/` (PDFs used as RAG source data).

## Build

CI (`.github/workflows/dotnet-build.yml`) restores and builds these nine targets on the .NET `10.0.x` SDK with `--configuration Release`. Run the same set when validating a change:

```pwsh
"Part 02 - Build Chat App/ChatApp/ChatApp.csproj",
"Part 03 - Add RAG/RagChatApp/RagChatApp.csproj",
"Part 03 - Add RAG/checkpoints/verify/Checkpoints.slnx",
"Part 05 - MCP Server Basics/MyMcpServer/MyMcpServer.csproj",
"Part 06 - Enhanced MCP Server/ContosoOrdersMcpServer/ContosoOrdersMcpServer.csproj",
"Part 08 - Agent Framework Basics/AgentApp/AgentApp.csproj",
"Part 09 - Adding AI to an Existing App/eShopLite/eShopLite.slnx",
"Part 09 - Adding AI to an Existing App/eShopLite-start/eShopLite.slnx",
"Part 11 - Deployment/GenAiLab/GenAiLab.sln" | ForEach-Object { dotnet build $_ -c Release }
```

All nine targets build clean — zero warnings. Treat any new warning as a regression.

Markdown is linted by `.github/workflows/markdownlint.yml` using `.markdownlint.json`.

## Conventions

- **Snapshots must match the README.** If you change instructions in a `README.md`, update that part's snapshot code in the same change, and vice versa.
- **Projects are created with `dotnet new`,** never by hand-authoring a `.csproj`. Templates come from `Microsoft.Extensions.AI.Templates`, `Microsoft.McpServer.ProjectTemplates`, and `Aspire.ProjectTemplates`.
- **Scaffold, then update packages.** Templates lag the current package versions. After `dotnet new`, bump to current and record the bump as an explicit README step so the snapshot and the instructions stay in sync. Aspire is pinned at **13.4.6** across the repo; note that the `<Sdk Name="Aspire.AppHost.Sdk" />` element must be edited by hand because `dotnet add package` only manages `<PackageReference>` items.
- The Part 4 scaffold command is fixed and load-bearing for later parts:
  `dotnet new aichatweb --provider azureopenai --vector-store qdrant --aspire --name GenAiLab --output GenAiLab`
  (a Docker-free variant, `--vector-store local --managed-identity false`, is documented as an alternative).
- **The AppHost deliberately diverges from the template.** The template scaffolds `builder.AddAzureOpenAI("openai")` plus `AddDeployment` calls, which makes Aspire try to *provision* an Azure OpenAI account and blocks on a tenant/subscription prompt. Setting `ConnectionStrings:openai` does not suppress it. Parts 4 and 11 replace that block with `builder.AddConnectionString("openai")` and teach the swap as an explicit step. See `docs/instructor/AICHATWEB_TEMPLATE_NOTES.md`.
- Console samples (Parts 2, 3, 8) read credentials from **user secrets**: `AzureOpenAI:Endpoint` and `AzureOpenAI:Key`. Part 9 sets the same two secrets in both its `Products` and `Store` projects, plus optional `LocalModel:Endpoint` / `LocalModel:Model` in `Store` for its Foundry Local step. Part 11 uses the Aspire connection string `ConnectionStrings:openai`.
- Never commit secrets. `.azure/` is gitignored, and `azure.yaml` is generated by `azd init` at deploy time rather than checked in.
- Don't commit `bin/`, `obj/`, or `.vs/` into snapshots.
- Docker/Podman is only needed for the Qdrant + Aspire path (Parts 4 and 11). Part 9 is Aspire but deliberately container-free — it uses SQLite and `SqliteVec`.

## Testing the workshop end to end

Use the **workshop-testing** skill (`.github/skills/workshop-testing/`) when asked to walk through the workshop as an attendee, validate a part, or reconcile snapshots against the READMEs.

Credentials for testing come from `.github/scripts/setup-workshop-credentials.ps1`, which sets `WORKSHOP_AZURE_OPENAI_ENDPOINT`, `WORKSHOP_AZURE_OPENAI_KEY`, `WORKSHOP_AZURE_OPENAI_CHAT`, `WORKSHOP_AZURE_OPENAI_EMBEDDING`, `WORKSHOP_AZURE_SUBSCRIPTION_ID`, `WORKSHOP_AZURE_LOCATION`, and optional local model values.

## Writing style

This is attendee-facing teaching material. Prefer plain, direct prose. Keep time estimates and pacing tables in sync with the root `README.md` schedule when you change the length of a part.
