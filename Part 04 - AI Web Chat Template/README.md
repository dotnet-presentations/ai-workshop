# Part 4: AI Web Chat Template - scaffold `aichatweb` and understand the code

> **⏱️ Estimated Time:** 30-45 minutes

You've now built a chat app (Part 2) and a RAG loop (Part 3) **by hand**. You know
what an `IChatClient` is, what an embedding is, what a vector search does, and why
an in-memory store doesn't scale.

In this part you scaffold the **`aichatweb` template** and compare its generated
web app with what you built by hand. The template includes the same concepts,
with production-oriented wiring for persistence and orchestration.

> **Aspire enters here**, motivated by a real need: your Part 3 vectors lived in
> memory and vanished on exit. A production app needs a persistent, orchestrated
> vector database.

## Prerequisites

- Familiarity with the concepts introduced in [Part 2](../Part%2002%20-%20Build%20Chat%20App/README.md) and [Part 3](../Part%2003%20-%20Add%20RAG/README.md) is helpful, but not required.
- **Docker Desktop** (or Podman) running for the recommended Qdrant + Aspire path.
- [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) with `gpt-5-mini` + `text-embedding-3-small` (see [Part 1](../Part%2001%20-%20Setup/README.md))

> [!NOTE]
> Docker is not required to learn the core template concepts. If you cannot run
> containers, use the [Docker-free path](#alternative-docker-free-path-local-vector-store-without-aspire)
> at the end of this part. You will still work with the Blazor chat app, dependency
> injection, ingestion, embeddings, semantic search, and RAG.

## Completed code for this part

The finished version of the app you are about to build lives in
**[`Part 11 - Deployment/GenAiLab/`](../Part%2011%20-%20Deployment/GenAiLab/)**.

It is the same solution, already scaffolded, already updated to the current
package versions, and already carrying every change this part asks you to make.
Part 11 reuses it for deployment, which is why it lives there rather than in this
folder. The **only** difference from a correctly completed Part 4 is one line that
Part 11 adds — `WithExternalHttpEndpoints()` in `AppHost.cs` — which is harmless
when running locally.

Use it to check your work if a step doesn't behave, or to catch up if you fall
behind. It still needs your own credentials: see [Step 2.4](#24-store-the-connection-string)
and set `ConnectionStrings:openai` on `GenAiLab.AppHost`.

## Step 1: Install the template and scaffold

```bash
dotnet new install Microsoft.Extensions.AI.Templates
dotnet new aichatweb --provider azureopenai --vector-store qdrant --aspire --name GenAiLab --output GenAiLab
```

### Alternative: scaffold in Visual Studio 2026

If you prefer Visual Studio instead of the CLI:

> [!NOTE]
> If **AI Chat Web App** does not appear in the **Create a new project** dialog, install the template first from a terminal:
>
> ```bash
> dotnet new install Microsoft.Extensions.AI.Templates
> ```
>
> Then restart Visual Studio 2026 and search again.

1. Open Visual Studio 2026 and select Create a new project.
1. Search for and choose AI Chat Web App.

    ![AI Chat Web App selected in the Visual Studio project template dialog](../images/vs-ai-webchat-template.png)

1. Use GenAiLab as the project name.

    ![GenAiLab entered as the project name in Visual Studio](../images/vs-configure-new-project.png)

1. In Additional information, set:
    - AI service provider: Azure OpenAI
    - Vector store: Qdrant
    - Use Aspire orchestration: enabled
1. Create the project and let package restore complete.

This generates a solution with three projects:

| Project | Role |
| --- | --- |
| `GenAiLab.Web` | The Blazor chat web app |
| `GenAiLab.AppHost` | The **Aspire** orchestrator (starts the app, Qdrant, and a document reader) |
| `GenAiLab.ServiceDefaults` | Shared telemetry, health checks, resilience |

## Step 2: Configure it before you run it

> [!IMPORTANT]
> Do this **before** you press F5 or run `dotnet run`. A freshly scaffolded project
> is not ready to talk to the workshop's Azure OpenAI resource, and running it
> first produces a confusing *Azure provisioning* prompt rather than a useful error.

Four things need to change, and all four are small:

| # | Change | File |
| --- | --- | --- |
| 2.1 | Bring the packages up to date | both `.csproj` files |
| 2.2 | Point the AppHost at an **existing** resource | `GenAiLab.AppHost/AppHost.cs` |
| 2.3 | Use the deployment name your resource actually has | `GenAiLab.Web/Program.cs` |
| 2.4 | Store the connection string | user secrets on `GenAiLab.AppHost` |

### 2.1 Bring the packages up to date

Templates ship on their own release cadence, so a freshly scaffolded project is
usually a few versions behind the current packages. Update it before you go any
further — this is what you would do on any real project, and it keeps your code
matching the completed solution in this repo.

This is not only housekeeping. The template scaffolds Aspire **13.0.0**, which
pulls in a MessagePack version carrying known high-severity advisories, so
`dotnet restore` reports `NU1903` until you move off it.

In `GenAiLab.AppHost/GenAiLab.AppHost.csproj`, change the SDK version to `13.4.6`:

```xml
<Sdk Name="Aspire.AppHost.Sdk" Version="13.4.6" />
```

The `<Sdk>` element has to be edited by hand — `dotnet add package` only manages
`<PackageReference>` items. For the rest, run these from the `GenAiLab` folder:

```bash
dotnet add GenAiLab.AppHost package Aspire.Hosting.AppHost --version 13.4.6
dotnet add GenAiLab.AppHost package Aspire.Hosting.Qdrant --version 13.4.6
dotnet add GenAiLab.Web package Aspire.Qdrant.Client --version 13.4.6
dotnet add GenAiLab.Web package Aspire.Azure.AI.OpenAI --prerelease
dotnet add GenAiLab.Web package Microsoft.Extensions.AI
dotnet add GenAiLab.Web package Microsoft.Extensions.AI.OpenAI
dotnet add GenAiLab.Web package Microsoft.SemanticKernel.Connectors.Qdrant --prerelease
```

`Aspire.Azure.AI.OpenAI` and `Microsoft.SemanticKernel.Connectors.Qdrant` need
`--prerelease` because neither has ever shipped a stable build; without the flag
the command fails.

> [!NOTE]
> The completed solution in this repo pins exact versions, so if a newer release
> has shipped since this was written you may end up slightly ahead of it. That is
> fine — the code in this part does not depend on anything that changed.

**Already ran the app before doing this step?** Updating Aspire also updates the
Qdrant container image, and the data volume written by the old version can stop the
new one from starting. See
[Qdrant won't start after updating packages](#qdrant-wont-start-after-updating-packages)
in Step 3.

### 2.2 Point the AppHost at your existing Azure OpenAI resource

The template assumes you want Aspire to **create** an Azure OpenAI account for you.
That is what `builder.AddAzureOpenAI("openai")` means: it declares a provisionable
Azure resource, and the two `AddDeployment` calls describe the models to deploy
into it. Run it as-is and the Aspire dashboard stops with an *Azure provisioning*
prompt asking for a tenant, subscription, and location.

For the workshop you already have a resource, so point at it instead. In
`GenAiLab.AppHost/AppHost.cs`, delete this whole block from the top of the file:

```csharp
// See https://learn.microsoft.com/dotnet/aspire/azure/local-provisioning#configuration
// for instructions providing configuration values
var openai = builder.AddAzureOpenAI("openai");

openai.AddDeployment(
    name: "gpt-4o-mini",
    modelName: "gpt-4o-mini",
    modelVersion: "2024-07-18");

openai.AddDeployment(
    name: "text-embedding-3-small",
    modelName: "text-embedding-3-small",
    modelVersion: "1");
```

and replace it with one line:

```csharp
var openai = builder.AddConnectionString("openai");
```

`AddConnectionString` declares no Azure resource at all — Aspire just reads the
`openai` connection string from configuration and passes it to the web project.
Leave the rest of `AppHost.cs` alone; `WithReference(openai)` further down keeps
working unchanged.

Then drop the package that the provisioning code needed:

```bash
dotnet remove GenAiLab.AppHost package Aspire.Hosting.Azure.CognitiveServices
```

> [!IMPORTANT]
> Setting `ConnectionStrings:openai` does **not** make `AddAzureOpenAI` skip
> provisioning — it is ignored, and the dashboard still asks for a subscription.
> The dialog behind the dashboard's **Enter values** button collects tenant,
> subscription, resource group, and location, so there is nowhere to paste an
> endpoint and key. Swapping to `AddConnectionString` is the fix.
>
> If you *do* want Aspire to provision the resource — in [Part 11](../Part%2011%20-%20Deployment/README.md), against your own subscription — put the template's
> version back. You will need Owner or User Access Administrator on the
> subscription, because provisioning also creates role assignments.

### 2.3 Use the deployment name your resource actually has

The template hardcodes `gpt-4o-mini`; the workshop resource deploys `gpt-5-mini`.
In `GenAiLab.Web/Program.cs`, change:

```csharp
openai.AddChatClient("gpt-4o-mini")
```

to:

```csharp
openai.AddChatClient("gpt-5-mini")
```

That string is a **deployment** name, not a model name — it has to match what is
deployed on the resource you are pointing at. If yours is named something else,
use that instead. This is the same coupling you will work around in
[Part 10](../Part%2010%20-%20Choosing%20Providers%20and%20Services/README.md).

The embedding line below it already matches, so leave
`openai.AddEmbeddingGenerator("text-embedding-3-small")` as it is.

### 2.4 Store the connection string

The AppHost needs one setting, `ConnectionStrings:openai`, and it goes in **user
secrets** — the same secrets-first rule as Parts 2 and 3. Never put a key in
`appsettings.json`.

> [!IMPORTANT]
> The secret belongs to **`GenAiLab.AppHost`**, not `GenAiLab.Web`. Aspire reads it
> in the AppHost and injects it into the web project at launch, so setting it on
> the wrong project silently does nothing.

The value is a single string combining the endpoint and key from
[Part 1](../Part%2001%20-%20Setup/README.md):

```text
Endpoint=https://YOUR-RESOURCE.openai.azure.com/;Key=YOUR-KEY
```

#### Visual Studio

1. In **Solution Explorer**, right-click the **`GenAiLab.AppHost`** project.
1. Choose **Manage User Secrets**. Visual Studio creates and opens `secrets.json`.
1. Replace the contents with this, substituting your own endpoint and key:

    ```json
    {
      "ConnectionStrings": {
        "openai": "Endpoint=https://YOUR-RESOURCE.openai.azure.com/;Key=YOUR-KEY"
      }
    }
    ```

1. Save the file. It lives outside your project folder, so it is never committed.

> [!TIP]
> `secrets.json` must be valid JSON. If the app still reports a missing connection
> string, the usual causes are a trailing comma, a missing closing brace, or having
> opened **Manage User Secrets** on `GenAiLab.Web` by mistake.

#### Command line

From the `GenAiLab` folder:

```bash
dotnet user-secrets --project GenAiLab.AppHost set ConnectionStrings:openai "Endpoint=https://YOUR-RESOURCE.openai.azure.com/;Key=YOUR-KEY"
```

Confirm it landed where you expect:

```bash
dotnet user-secrets --project GenAiLab.AppHost list
```

You should see exactly one entry, `ConnectionStrings:openai`.

### 2.5 Check it compiles

From the `GenAiLab` folder:

```bash
dotnet build
```

Expect a clean build with no warnings. `NU1903` here means step 2.1 was missed;
an error about `AddAzureOpenAI` or `AddDeployment` means step 2.2 is incomplete.

## Step 3: Run the app

> [!IMPORTANT]
> For Aspire solutions, launch the `GenAiLab.AppHost` project. AppHost bootstraps the other projects and supporting services (such as Qdrant). If you run only `GenAiLab.Web`, you skip the full orchestrated experience.

Make sure Docker Desktop is running, then:

```bash
cd GenAiLab
dotnet run --project GenAiLab.AppHost
```

In Visual Studio, set **`GenAiLab.AppHost`** as the startup project and press
Ctrl+F5.

Aspire launches a **dashboard** (URL printed in the console) showing every service,
its health, logs, traces, and metrics. This is why `UseOpenTelemetry(...)` was in
`Program.cs`: the telemetry you registered now has a destination.

> [!TIP]
> If the dashboard shows an **Azure provisioning** prompt asking for a subscription,
> step 2.2 didn't take effect — `AppHost.cs` is still calling `AddAzureOpenAI`.

### Qdrant won't start after updating packages

If you ran the app before Step 2.1 and then updated the packages, `vectordb` may
never reach **Running**. Its logs in the Aspire dashboard show a storage or version
error rather than the usual startup banner.

Two things in `AppHost.cs` cause this, and both are deliberate:

```csharp
var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()                                  // storage survives shutdown
    .WithLifetime(ContainerLifetime.Persistent);       // container survives shutdown
```

`WithDataVolume()` is the whole point of this part — it is the answer to Part 3's
vectors vanishing on exit. But bumping Aspire also bumps the Qdrant image, and a
data volume written by the older Qdrant can be incompatible with the newer one.
`WithLifetime(ContainerLifetime.Persistent)` compounds it: the old container is
kept and reused rather than recreated, so it does not pick up the new image.

The fix is to throw away both. Nothing of value is lost — the volume holds only
embeddings of the two sample documents, which the app regenerates on your next
question.

```bash
# find the leftovers (Aspire names them after your AppHost)
docker ps -a --filter "name=vectordb"
docker volume ls --filter "name=vectordb-data"

# remove them, substituting the names you just saw
docker rm -f vectordb-38984c72
docker volume rm genailab.apphost-38984c7271-vectordb-data
```

In Docker Desktop, do the same from the **Containers** and **Volumes** tabs.

Then run the AppHost again. Aspire recreates the container from the new image with
an empty volume, and your first question re-ingests the sample documents — so it
will be slow again, exactly like the first run.

> [!TIP]
> Doing Step 2 in order avoids this entirely: update the packages *before* the
> first run and there is never an old volume to conflict with.

## Step 4: Test the app end to end

1. In the Aspire dashboard, wait until `aichatweb-app`, `vectordb`, and `markitdown` are running.
1. Open the `aichatweb-app` URL from the dashboard.
1. Ask a question grounded in the sample data, for example "What water purification supplies are in the emergency survival kit?"
1. The first question takes a while — that is ingestion running for the first time, including sending the sample PDF to the markitdown container. Later questions are fast.
1. Confirm the answer carries citations back to `Example_Emergency_Survival_Kit.pdf` or `Example_GPS_Watch.md` rather than reading like a generic model response. Clicking a citation opens the source document at the quoted text.
1. In the dashboard, open logs and traces for `aichatweb-app` to see the search calls the model made on its own — the template registers search as a tool, so the model decides when to retrieve.

If you want the official step-by-step quickstart for scaffold + config + first run, see:
[.NET AI templates quickstart](https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/ai-templates?tabs=visual-studio%2Cconfigure-visual-studio&pivots=azure-openai).

## Step 5: Map the generated code to what you built by hand

Now that it runs, read what the template wrote. All of it maps directly to the
parts you already built.

### Chat client: your Part 2 pipeline, as DI configuration

`GenAiLab.Web/Program.cs`:

```csharp
var openai = builder.AddAzureOpenAIClient("openai");
openai.AddChatClient("gpt-5-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
openai.AddEmbeddingGenerator("text-embedding-3-small");
```

| Part 2/3 (by hand) | Template (generated) |
| --- | --- |
| `new AzureOpenAIClient(endpoint, key).GetChatClient(...).AsIChatClient()` | `builder.AddAzureOpenAIClient("openai").AddChatClient(...)` |
| `.AsBuilder().UseLogging(...).Build()` | `.UseFunctionInvocation().UseOpenTelemetry(...)` |
| `azureClient.GetEmbeddingClient(...).AsIEmbeddingGenerator()` | `openai.AddEmbeddingGenerator("text-embedding-3-small")` |

Same abstractions, now registered in DI and wrapped with a richer middleware
pipeline (function calling + telemetry instead of your hand-added logging).

Note what the endpoint and key did *not* do here: there is no `new
AzureOpenAIClient(endpoint, key)`. `AddAzureOpenAIClient("openai")` looks up a
connection string **by name**, and Aspire supplied it from the AppHost. That
indirection is the reason step 2.4 set the secret on the AppHost instead.

### Retrieval: your Part 3 cosine search, as a service

`GenAiLab.Web/Services/SemanticSearch.cs`:

```csharp
public class SemanticSearch(
    VectorStoreCollection<Guid, IngestedChunk> vectorCollection,
    [FromKeyedServices("ingestion_directory")] DirectoryInfo ingestionDirectory,
    DataIngestor dataIngestor)
{
    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        await LoadDocumentsAsync();

        var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk>
        {
            Filter = documentIdFilter is { Length: > 0 } ? record => record.DocumentId == documentIdFilter : null,
        });

        return await nearest.Select(result => result.Record).ToListAsync();
    }
}
```

| Part 3 (by hand) | Template (generated) |
| --- | --- |
| Your `CosineSimilarity(...)` loop over a `List` | `vectorCollection.SearchAsync(...)` on a real vector store |
| Manual top-k `.OrderByDescending().Take(k)` | `maxResults` argument |
| No filtering | `Filter` by document id |

You wrote the math; the template delegates it to Qdrant through the
`Microsoft.Extensions.VectorData` abstraction. It applies the same swap idea at
the vector-store level.

### Ingestion: your Part 3 chunk-and-embed step, as a pipeline

`GenAiLab.Web/Services/Ingestion/DataIngestor.cs` builds an ingestion pipeline out
of three pieces — a reader, a chunker, and a writer — and runs every file in
`wwwroot/Data` through it:

```csharp
using var pipeline = new IngestionPipeline<string>(
    reader: new DocumentReader(directory),
    chunker: new SemanticSimilarityChunker(embeddingGenerator, new(TiktokenTokenizer.CreateForModel("gpt-4o"))),
    writer: writer,
    loggerFactory: loggerFactory);

await foreach (var result in pipeline.ProcessAsync(directory, searchPattern))
{
    logger.LogInformation("Completed processing '{id}'. Succeeded: '{succeeded}'.", result.DocumentId, result.Succeeded);
}
```

`DocumentReader.cs` dispatches on media type: Markdown goes through an in-process
Markdig reader, and PDFs go to the **markitdown** container over MCP.

| Part 3 (by hand) | Template (generated) |
| --- | --- |
| Split doc into paragraph chunks | `SemanticSimilarityChunker` — splits where the meaning shifts, not at fixed lengths |
| `GenerateAsync(chunks)` once at startup | `IngestionPipeline<T>.ProcessAsync(...)` |
| `store.Add((text, vector))` in a `List` | `VectorStoreWriter<T>` upserting into Qdrant |
| Plain text only | Markdown and PDF, via pluggable readers |

Ingestion is lazy. `SemanticSearch.LoadDocumentsAsync()` runs it once on the first
search, which is why your first question takes noticeably longer than the rest.

> ⚠️ Only ingest **trusted** content. Ingested text is reflected back to users and
> is a prompt-injection risk.

### Persistence + orchestration: the answer to "in-memory doesn't scale"

`GenAiLab.AppHost/AppHost.cs`, as you left it after step 2.2:

```csharp
var openai = builder.AddConnectionString("openai");

var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var markitdown = builder.AddContainer("markitdown", "mcp/markitdown")
    .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001")
    .WithHttpEndpoint(targetPort: 3001, name: "http");

var webApp = builder.AddProject<Projects.GenAiLab_Web>("aichatweb-app");
webApp.WithReference(openai).WaitFor(openai);
webApp.WithReference(vectorDB).WaitFor(vectorDB);
webApp.WithEnvironment("MARKITDOWN_MCP_URL", markitdown.GetEndpoint("http"));
```

This is the direct fix for Part 3's limitation: the vectors now live in a **Qdrant
container with a persistent data volume**, and Aspire starts the database and the
document reader, waits for them to be ready, then starts the web app and wires the
connection strings and endpoints between them automatically.

## Alternative: Docker-free path (local vector store without Aspire)

The template also supports a single-project variant that stores vectors in a local
SQLite file and skips containers entirely. Use this if you cannot run Docker.

The same configuration concepts from Steps 1-4 apply, but this scaffold uses
different files and settings, so the whole path is given here. Read Step 5 as
an architecture comparison rather than a description of files in your project.

### Scaffold it

```bash
dotnet new aichatweb --provider azureopenai --vector-store local --managed-identity false --name GenAiLab --output GenAiLab
```

In Visual Studio, choose **Local** for the vector store, leave **Use Aspire
orchestration** disabled, and clear **Use managed identity**.

`--managed-identity false` matters. Left at its default the template authenticates
with Entra ID, which requires your signed-in account to hold the **Azure AI
Developer** role on the Azure OpenAI resource. Turning it off gives you API-key
authentication and the same two secrets you already used in Parts 2 and 3.

### Configure it before you run it

There is no AppHost here, so all three changes are in the single `GenAiLab`
project.

**Set the deployment name.** Exactly as in [step 2.3](#23-use-the-deployment-name-your-resource-actually-has),
this variant also hardcodes `gpt-4o-mini`, just through a different API. In
`GenAiLab/Program.cs`, change:

```csharp
var chatClient = azureOpenAi.GetResponsesClient().AsIChatClient("gpt-4o-mini");
```

to:

```csharp
var chatClient = azureOpenAi.GetResponsesClient().AsIChatClient("gpt-5-mini");
```

Skipping this is the most common failure on this path: the app builds and starts
fine, then the first chat message fails with a 404 because `gpt-4o-mini` is not
deployed on the workshop resource.

**Store the credentials.** This variant reads two plain settings rather than a
connection string, because there is no Aspire to compose one:

```bash
dotnet user-secrets --project GenAiLab set AzureOpenAI:Endpoint "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets --project GenAiLab set AzureOpenAI:Key "YOUR-KEY"
```

In Visual Studio, right-click the **`GenAiLab`** project → **Manage User Secrets**
and paste:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "Key": "YOUR-KEY"
  }
}
```

**Clear the package advisory.** `dotnet restore` reports `NU1903` for a transitive
`SQLitePCLRaw.lib.e_sqlite3` 2.1.10 reference. Pin the fixed version:

```bash
dotnet add GenAiLab package SQLitePCLRaw.bundle_e_sqlite3 --version 3.0.4
```

### Run it

```bash
dotnet run --project GenAiLab
```

Then open the printed URL and test with the same questions as Step 4. There is no
Aspire dashboard on this path, so logs go to the console.

### How the two paths differ

This variant is not just "the Aspire one minus the containers":

| | Aspire path | Docker-free path |
| --- | --- | --- |
| Projects | Three (Web, AppHost, ServiceDefaults) | One |
| Vector store | Qdrant container with a data volume | SQLite file via `AddSqliteVectorStore` |
| PDF reading | `mcp/markitdown` container | `PdfPig`, in-process |
| Chat API | `AddChatClient("gpt-5-mini")` | `GetResponsesClient().AsIChatClient("gpt-5-mini")` |
| Credentials | `ConnectionStrings:openai` on the AppHost | `AzureOpenAI:Endpoint` + `AzureOpenAI:Key` on the app |

| You can still complete | This path omits |
| --- | --- |
| Blazor chat UI and AI client configuration | Qdrant and its persistent container volume |
| Document ingestion, embeddings, and semantic search | The Aspire AppHost and service orchestration |
| RAG and the vector-store abstraction | The Aspire dashboard, distributed health checks, logs, traces, and metrics |

> [!NOTE]
> [Part 11](../Part%2011%20-%20Deployment/README.md) deploys the Aspire path. If you
> took this one, read Part 11 as a walkthrough rather than following along, or use
> the completed solution in `Part 11 - Deployment/GenAiLab/`.

## What you learned

You can now open **any** generated file and explain it, because you built each
concept by hand first:

- `IChatClient` + middleware → Part 2
- `IEmbeddingGenerator`, chunking, vector search, prompt augmentation → Part 3
- Qdrant + Aspire → the persistence and orchestration your hand-built version lacked

You also saw two things the template can't decide for you: which Azure OpenAI
resource to talk to, and which deployment name to ask for. Both are choices about
*your* environment, and both are one line of code.

## What's next

In **Part 5** you'll step outside the application itself and build a **Model
Context Protocol (MCP) server** — a tool an AI agent can call. That tool becomes
the one you hand to an agent in Part 8.

One thread stays open until the end of the day. Everything you have written so
far talks to `IChatClient` and `IEmbeddingGenerator` rather than to Azure
OpenAI directly, and nothing yet explains why that indirection is worth it. In
[Part 10](../Part%2010%20-%20Choosing%20Providers%20and%20Services/README.md) you
swap the provider out entirely — for a local model, with no application code
changes — right before you deploy.

**Continue to** → [Part 5: MCP Server Basics](../Part%2005%20-%20MCP%20Server%20Basics/README.md)

---

📖 **Return to**: [Workshop Overview](../README.md) | 🔄 **Previous**: [Part 3: Add RAG](../Part%2003%20-%20Add%20RAG/README.md) | ➡️ **Next**: [Part 5: MCP Server Basics](../Part%2005%20-%20MCP%20Server%20Basics/README.md)
