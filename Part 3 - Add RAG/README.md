# Part 3: Add RAG by hand (console)

In Part 2 you built a chat app. The problem: the model only knows what it was
trained on. Ask it about *your* product, *your* policies, or anything private and
it will guess. **Retrieval-Augmented Generation (RAG)** fixes that by retrieving
relevant text and injecting it into the prompt.

In this part you build a **minimal** RAG loop by hand first, then replace the
ingestion plumbing with [`Microsoft.Extensions.DataIngestion` (MEDI)](https://learn.microsoft.com/en-us/dotnet/ai/conceptual/data-ingestion). This gives
you the conceptual understanding without spending most of your time on
boilerplate.

> Adapted with thanks from [Steve Sanderson's dotnet-ai-workshop](https://github.com/SteveSandersonMS/dotnet-ai-workshop) (chapters 2, 3, and 6).

## What you will build

```text
question ─▶ embed ─▶ semantic search over ingested chunks ─▶ top-k context
                                                             │
document ─▶ read/chunk/embed/store (manual slice, then MEDI) ─┘
                                                             ▼
                             augment system prompt ─▶ chat model ─▶ grounded answer
```

1. **`IEmbeddingGenerator`**: turn text into vectors
2. **Chunk** the document into retrievable pieces
3. **Embed + store** chunks (manual in-memory first, then MEDI + vector store)
4. **Cosine similarity search**: naive top-k retrieval, written by hand
5. **Augment the prompt** with the retrieved context, then answer

## Prerequisites

- Completed [Part 2](../Part%202%20-%20Build%20Chat%20App/README.md)
- A [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) resource with **`gpt-5-mini`** *and*
  **`text-embedding-3-small`** deployed (see [Part 1 - Setup](../Part%201%20-%20Setup/README.md))

## Step 1: Start from the Part 2 project

Copy your Part 2 `ChatApp` (or the [provided project](RagChatApp)) and add one
package for embeddings. Everything else is already there.

Create a `sample-docs` folder in the same directory as your project file and copy
the sample markdown document into it:

```bash
mkdir sample-docs
copy "..\Part 3 - Add RAG\RagChatApp\sample-docs\contoso-trailblazer-3000.md" "sample-docs\"
```

> [!TIP]
> Keep model names in code/config (not secrets). In this part, use a normal
> code default for embeddings: `text-embedding-3-small`.

## Step 2: Minimal manual slice (instructional)

Build only the smallest manual ingestion slice so students learn the mechanics:

1. Chunk a markdown document into paragraph-sized chunks.
1. Generate embeddings for chunks using `IEmbeddingGenerator`.
1. Rank chunks with cosine similarity and take top-k.
1. Inject retrieved context into a grounded system prompt.

For portability, resolve the doc path from the current working directory first,
then fall back to the output directory if needed.

This keeps the conceptual value while avoiding unnecessary boilerplate.

### Checkpoint A: Complete manual implementation

Use this as the full manual reference checkpoint:

- [RagChatApp/Program.cs](RagChatApp/Program.cs)
- [checkpoints/manual-program.cs](checkpoints/manual-program.cs)

At this checkpoint, your manual `Program.cs` should match
[RagChatApp/Program.cs](RagChatApp/Program.cs).

## Step 3: Replace ingestion plumbing with MEDI (recommended)

Now swap your hand-written ingestion pipeline for
`Microsoft.Extensions.DataIngestion` so class time can focus on retrieval quality
and prompting strategy instead of ingestion boilerplate.

### 3.1 Add MEDI packages

```bash
dotnet add package Microsoft.Extensions.DataIngestion --prerelease
dotnet add package Microsoft.Extensions.DataIngestion.Markdig --prerelease
dotnet add package Microsoft.Extensions.Logging.Console
dotnet add package Microsoft.ML.Tokenizers.Data.O200kBase
dotnet add package Microsoft.SemanticKernel.Connectors.SqliteVec --prerelease
```

### 3.2 Compose the ingestion pipeline

Use MEDI components for reader + chunker + writer:

1. `MarkdownReader` reads source docs.
1. `SemanticSimilarityChunker` creates semantically coherent chunks.
1. `VectorStoreWriter<string>` stores chunks + embeddings in `SqliteVectorStore`.
1. `IngestionPipeline<string>` orchestrates ingestion end-to-end.

### 3.3 Keep retrieval and grounded answering

After ingestion completes, query the vector collection for top matches and use
that context in the same grounded chat pattern from the manual approach.

### Checkpoint B: Complete MEDI implementation

Use this as the complete MEDI reference checkpoint:

- [checkpoints/medi-program.cs](checkpoints/medi-program.cs)

At this checkpoint, your MEDI-based program should match
[checkpoints/medi-program.cs](checkpoints/medi-program.cs).

## Step 4: See the difference

Run it and ask something only the document knows:

```bash
dotnet run
```

```text
You: How do I dry the boots?
Assistant: Air-dry them away from direct heat. Never place them on a radiator,
as that damages the waterproof membrane.

You: What is the warranty?
Assistant: A 2-year limited warranty covering manufacturing defects...

You: Who won the 2022 World Cup?
Assistant: I don't know. That isn't in the provided context.
```

The last answer is the point: the assistant is now **grounded**. It answers from
your document and declines when the answer isn't there.

## What's next

Your knowledge base lives in memory, so it's rebuilt on every run and can't scale.
In **Part 4** you'll scaffold the **aichatweb template** and see how it solves
these problems with a real vector store (Qdrant), ingestion services, and
semantic search, using the same `IChatClient` and `IEmbeddingGenerator`
abstractions you just used by hand.

**Continue to** -> [Part 4: AI Web Chat Template](../Part%204%20-%20AI%20Web%20Chat%20Template/README.md)
