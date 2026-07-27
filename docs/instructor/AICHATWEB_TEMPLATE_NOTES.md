# `aichatweb` template notes for instructors

Instructor-facing background for Parts 4 and 11. Attendees don't need any of this,
but it explains why the workshop deviates from the template in a couple of places
and what to say when someone asks.

Verified against `Microsoft.Extensions.AI.Templates` **10.7.0-preview.3.26309.5**
on .NET SDK 10.0.301, July 2026.

## Azure provisioning is the one real deviation

The template's Aspire AppHost declares a **provisionable** Azure OpenAI resource:

```csharp
var openai = builder.AddAzureOpenAI("openai");
openai.AddDeployment(name: "gpt-4o-mini", modelName: "gpt-4o-mini", modelVersion: "2024-07-18");
openai.AddDeployment(name: "text-embedding-3-small", modelName: "text-embedding-3-small", modelVersion: "1");
```

Part 4 has attendees replace that whole block with:

```csharp
var openai = builder.AddConnectionString("openai");
```

Three things to know about why:

1. **Setting `ConnectionStrings:openai` does not make `AddAzureOpenAI` stand down.**
   This was tested directly: with the connection string present in the AppHost
   user secrets, the resource log still read `Provisioning openai...` and the app
   never started. `AddConnectionString` declares no Azure resource at all, so
   there is nothing to provision and the string is simply passed through.

2. **The dashboard's "Enter values" button doesn't help.** It opens a form asking
   for Tenant ID, Subscription ID, Resource group, and Location. There is no field
   for an endpoint or an API key, so an attendee on shared workshop credentials
   has nothing to type into it.

3. **There is no template switch for this.** `--managed-identity` exists but its
   condition is `!UseAspire && VectorStore != "qdrant" && ...`, so it has no effect
   on the Aspire + Qdrant path. Provisioning is baked into that variant.

Anyone who *does* want the template's behavior can keep it, but they need **Owner**
or **User Access Administrator** on the subscription, because provisioning also
creates role assignments (there's an `openai-roles` resource in the model).

## Model names

The template hardcodes `gpt-4o-mini` in both `AppHost.cs` and `Program.cs`. Part 4
has attendees change `AddChatClient` to `gpt-5-mini` to match the workshop
resource. It's worth pausing on: that string is a **deployment** name, not a model
name, and it's the same coupling Part 10 works around.

`dotnet new aichatweb` does accept `-C/--ChatModel` and `-E/--EmbeddingModel`, and
they substitute correctly — but `modelVersion` in `AddDeployment` stays pinned at
`2024-07-18` regardless, so `--ChatModel gpt-5-mini` generates a gpt-5-mini
deployment carrying a gpt-4o-mini version string. Doesn't matter for the workshop
since the deployment block is removed anyway. The deliberate choice is to have
attendees edit it by hand.

## Package versions are a security step, not housekeeping

The template scaffolds Aspire **13.0.0**, which pulls in a MessagePack version with
known high-severity advisories. `dotnet restore` reports `NU1903` until you move to
13.4.6. Keep the version-bump step in Part 4 even if it looks like tidiness.

The Docker-free variant has its own version of this: a transitive
`SQLitePCLRaw.lib.e_sqlite3` 2.1.10 reference with the same advisory as
[issue #564](https://github.com/dotnet-presentations/ai-workshop/issues/564).
Part 4 documents the `SQLitePCLRaw.bundle_e_sqlite3` 3.0.4 pin that clears it.

## The Docker-free path is a different app

`--vector-store local` is not "the Aspire one minus the containers":

| | Aspire path | Docker-free path |
| --- | --- | --- |
| Projects | Three | One |
| Vector store | Qdrant container | SQLite via `AddSqliteVectorStore` |
| PDF reading | `mcp/markitdown` container | `PdfPig`, in-process |
| Chat API | `AddChatClient(...)` | `GetResponsesClient().AsIChatClient(...)` |
| Auth (default) | connection string | Entra ID via `Azure.Identity` |

Pass `--managed-identity false` for this variant. Left at its default it wants the
attendee's signed-in identity to hold the **Azure AI Developer** role on the Azure
OpenAI resource, which won't be true for shared workshop credentials. Turning it
off produces `AzureOpenAI:Endpoint` and `AzureOpenAI:Key` user secrets — the same
two names Parts 2 and 3 already use.

## Ingestion changed shape

Older versions of the template read PDFs in-process with `PdfPig` through a
`PDFDirectorySource`. The current one uses `Microsoft.Extensions.DataIngestion`
with a reader/chunker/writer pipeline, dispatching PDFs to an `mcp/markitdown`
container over MCP and Markdown to an in-process Markdig reader. Chunking is
`SemanticSimilarityChunker`, which splits where meaning shifts rather than at fixed
lengths.

Two consequences for the room:

- **The first question is slow.** Ingestion is lazy — `SemanticSearch.LoadDocumentsAsync()`
  runs it on the first search, not at startup. The UI shows "Loading relevant
  documents (this will take a minute)". Later questions are fast. Warn people
  before they think it's broken.
- **`azd` deploys markitdown too.** It becomes a third container app alongside the
  web app and Qdrant.

## Things that go wrong in the room

| Symptom | Cause |
| --- | --- |
| Dashboard shows an "Azure provisioning" banner and resources stay in Starting | `AddAzureOpenAI` still in `AppHost.cs`; the swap to `AddConnectionString` was missed |
| 404 or "deployment not found" on the first chat message | `AddChatClient` still says `gpt-4o-mini` |
| First question hangs for a minute | Expected — lazy ingestion, including the PDF round-trip through markitdown |
| Qdrant or markitdown never start | Docker Desktop isn't running |
| `NU1903` on restore | Package bump step skipped |
