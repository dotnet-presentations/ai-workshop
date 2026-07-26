---
name: workshop-testing
description: 'Walk through the .NET AI Workshop as an attendee to validate that the READMEs, commands, and code snapshots still work. USE FOR: testing the workshop, testing a specific Part, dry-running the labs, verifying a README against its snapshot, reconciling or refreshing code snapshots, producing a workshop test report. DO NOT USE FOR: authoring new workshop content, general repo edits, or fixing a single build error (just fix it directly).'
---

# Workshop Testing

Complete the workshop the way an attendee would — follow each `Part N - */README.md` literally, in a scratch directory — then reconcile what you produced against the committed snapshot and write a report.

## Ground rules

1. **Follow the README, not this skill.** The README is the artifact under test. If it is wrong or ambiguous, that is a finding, not something to silently work around.
2. **Always scaffold with `dotnet new`.** Never hand-author or copy a `.csproj`, and never use a committed snapshot as your working directory. The template workflow is part of what is being tested.
3. **Work in `test-workspace/`** at the repo root (gitignored / delete when done). Never build inside the snapshot folders while testing.
4. **Ask, don't guess.** If a step needs a value you don't have (subscription, endpoint, model name), ask the user.
5. **Record every clarification you needed.** Anything you had to infer is a documentation gap worth reporting.

## Setup

```pwsh
.\.github\scripts\setup-workshop-credentials.ps1 -ApplyUserSecrets
dotnet --list-sdks                                  # expect 10.0.x
dotnet new install Microsoft.Extensions.AI.Templates
docker --version                                    # only needed for Parts 4 and 11
```

The script collects the `WORKSHOP_*` variables and, with `-ApplyUserSecrets`, writes them into the snapshot projects. Add `-Force` if a key has been rotated since the last run.

Nothing in the workshop reads `WORKSHOP_*` directly. The console samples read user secrets (`AzureOpenAI:Endpoint`, `AzureOpenAI:Key`) and the Aspire app reads `ConnectionStrings:openai`. **Projects you scaffold yourself during a test run still need their secrets set by hand** — the script only knows about the committed snapshots.

The samples hardcode the deployment names `gpt-5-mini` and `text-embedding-3-small`. If the test resource uses different names, that is a source edit in every part, not a config change.

## Part-by-part

| Part | What to do | Snapshot to reconcile |
| --- | --- | --- |
| 1 - Setup | Verify prerequisites and install steps actually work | — |
| 2 - Build Chat App | `dotnet new console -n ChatApp`, add packages and code per README. Run it: chat, streaming, structured output | `Part 2 - Build Chat App/ChatApp/` |
| 3 - Add RAG | Continue from your Part 2 app (README says copy it). Verify retrieval answers from `manuals/`. Also check the two `checkpoints/*.cs` variants still compile against the described packages | `Part 3 - Add RAG/RagChatApp/` |
| 4 - AI Web Chat Template | Scaffold with the exact command in the README (`--provider azureopenai --vector-store qdrant --aspire --name GenAiLab`). Run via `GenAiLab.AppHost`. Also sanity-check the documented Docker-free `--vector-store local` path | Compare against `Part 11 - Deployment/GenAiLab/` |
| 5 - Providers and Fallbacks | Documentation only — read for accuracy of provider names, packages, and config keys | — |
| 6 - MCP Server Basics | `dotnet new mcpserver -n MyMcpServer`, add `WeatherTools` per README. Keep the template's `RandomNumberTools` | `Part 6 - MCP Server Basics/MyMcpServer/` |
| 7 - Enhanced MCP Server *(bonus)* | Exploration only — build and run the existing snapshot, review the README's business-integration guidance | `Part 7 - Enhanced MCP Server/ContosoOrdersMcpServer/` |
| 8 - MCP Publishing *(bonus)* | Documentation review only. **Do not publish anything** | — |
| 9 - Agent Framework Basics | `dotnet new console` → `AgentApp`, add `Microsoft.Agents.AI` per README. Verify the agent runs and can call the Part 6 weather tool if the README wires that up | `Part 9 - Agent Framework Basics/AgentApp/` |
| 10 - Adding AI to an Existing App | Run the completed `StoreApp` snapshot. The local-inference module needs `LocalModel:Endpoint` / `LocalModel:Model` (Ollama or Foundry Local) — note it as skipped if unavailable | `Part 10 - Adding AI to an Existing App/StoreApp/` |
| 11 - Deployment | See below | `Part 11 - Deployment/GenAiLab/` |

### MCP server verification (Parts 6-7)

An MCP server is a stdio process — it starts and waits. Check:

1. `dotnet build` succeeds (Part 7 legitimately emits 3 × CS1998).
2. `dotnet run` logs `Server (stream) (<Name>) transport reading messages`.
3. Ctrl+C shuts it down cleanly.
4. *Optional:* register it in `.vscode/mcp.json` and confirm the tools appear in Copilot Chat.

### Part 11 deployment

Default to **configuration-only** testing: confirm `AppHost.cs` calls `WithExternalHttpEndpoints()`, and that `dotnet build -c Release` on `GenAiLab.sln` succeeds. Note that `azure.yaml` and `.azure/` are *not* committed — `azd init` generates them, and `.azure/` is gitignored.

Only run a real deployment if the user explicitly asks. If so:

- Use `Push-Location` with the absolute path to the Part 11 folder; terminal cwd does not persist reliably across azd invocations.
- `azd auth login --use-device-code`, then **interactive** `azd up`. `--no-prompt` fails on the secure OpenAI connection-string parameter.
- Use a unique environment name, e.g. `workshoptest-<timestamp>`.
- Provision takes roughly 7-12 minutes; **`azd down --purge --force` is mandatory afterward** and takes 20-25 minutes. Confirm the resource group is gone in the portal.

## Reconciling snapshots

At the end of each code part, diff your working directory against the committed snapshot and classify each difference:

- **Template drift** (the template now emits different code) → update the snapshot, note the change.
- **README gap** (a step is missing, stale, or ambiguous) → record it as a finding; fix the README if the user asks.
- **Your mistake** → redo the step.

Before copying a working directory over a snapshot, strip `bin/`, `obj/`, `.vs/`, and `TestResults/`. Preserve `Properties/launchSettings.json` exactly for the Aspire projects — port drift breaks the Part 11 instructions. Then verify with `dotnet clean; dotnet build -c Release`.

## Report

Write `docs/testing/workshop-test-report-<YYYY-MM-DD>.md` using [report-template.md](./report-template.md), and add it to the index in `docs/README.md`. Older reports live in `docs/archive/testing/`.

## Known quirks

- Package versions drift between parts (`Microsoft.Extensions.AI`, `Microsoft.Agents.AI`, `Azure.AI.OpenAI`). Flag inconsistencies in the report rather than unilaterally bumping them.
- The MCP template's namespace style has changed between previews; the Part 6 snapshot uses `namespace MyMcpServer.Tools;` while Part 7's tool file has none. Note which style the current template emits.
- Missing `--vector-store qdrant` in Part 4 silently produces a local JSON store app that no longer matches the Part 11 snapshot.
- The AI Web Chat template ships `ChatInput.razor.js` and `ChatMessageList.razor.js` (auto-resize and auto-scroll). They must survive into any snapshot update.
