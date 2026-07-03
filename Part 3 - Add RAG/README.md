# Part 3: Add RAG by hand (console)

In Part 2 you built a chat app. The problem: the model only knows what it was
trained on. Ask it about *your* product, *your* policies, or anything private and
it will guess. **Retrieval-Augmented Generation (RAG)** fixes that by retrieving
relevant text and injecting it into the prompt.

In this part you build a complete RAG loop **by hand** — no vector database, no
framework magic. When you meet the template in Part 4, you'll recognize every
piece because you wrote it yourself.

> Adapted with thanks from [Steve Sanderson's dotnet-ai-workshop](https://github.com/SteveSandersonMS/dotnet-ai-workshop) (chapters 2, 3, and 6).

## What you will build

```
question ─▶ embed ─▶ cosine search over stored chunks ─▶ top-k context
                                                              │
document ─▶ chunk ─▶ embed ─▶ store (in-memory list) ─────────┘
                                                              ▼
                              augment system prompt ─▶ chat model ─▶ grounded answer
```

1. **`IEmbeddingGenerator`** — turn text into vectors
2. **Chunk** the document into retrievable pieces
3. **Embed + store** the chunks in an in-memory list
4. **Cosine similarity search** — naive top-k retrieval, written by hand
5. **Augment the prompt** with the retrieved context, then answer

## Prerequisites

- Completed [Part 2](../Part%202%20-%20Build%20Chat%20App/README.md)
- An Azure AI Foundry resource with **`gpt-4.1-mini`** *and*
  **`text-embedding-3-small`** deployed (see [Part 1 - Setup](../Part%201%20-%20Setup/README.md))

## Step 1: Start from the Part 2 project

Copy your Part 2 `ChatApp` (or the [provided project](RagChatApp)) and add one
package for embeddings — everything else is already there.

The embedding model needs one extra secret:

```bash
dotnet user-secrets set "AzureOpenAI:EmbeddingModel" "text-embedding-3-small"
```

## Step 2: Create an embedding generator

The **same** `AzureOpenAIClient` gives you both a chat client and an embedding
generator — two abstractions over one resource:

```csharp
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    azureClient.GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator();
```

An embedding is just an array of floats. Texts with **similar meaning** produce
vectors pointing in **similar directions** — that's what lets us search by meaning
instead of by keyword.

## Step 3: Chunk, embed, and store

Split the document into paragraph-sized chunks, embed them all in one call, and
keep the `(text, vector)` pairs in a plain `List`:

```csharp
GeneratedEmbeddings<Embedding<float>> embeddings =
    await embeddingGenerator.GenerateAsync(chunks);

var store = new List<(string Text, ReadOnlyMemory<float> Vector)>();
for (int i = 0; i < chunks.Length; i++)
    store.Add((chunks[i], embeddings[i].Vector));
```

> This is deliberately naive. The list doesn't persist between runs and it won't
> scale past a few thousand chunks — which is exactly the motivation for the real
> vector store in Part 4.

## Step 4: Cosine similarity search (by hand)

For each question, embed it with the **same** generator, then rank every stored
chunk and take the top matches:

```csharp
var topChunks = store
    .Select(item => (item.Text, Score: CosineSimilarity(questionVector.Span, item.Vector.Span)))
    .OrderByDescending(x => x.Score)
    .Take(topK)
    .Select(x => x.Text)
    .ToArray();
```

Cosine similarity is just `dot(a, b) / (|a| * |b|)` — a value from -1 to 1. See
`CosineSimilarity` in [RagChatApp/Program.cs](RagChatApp/Program.cs).

## Step 5: Augment the prompt, then answer

Rebuild the system message each turn with the retrieved context, and tell the
model to answer **only** from it:

```csharp
var systemPrompt = new ChatMessage(ChatRole.System,
    "Answer using ONLY the context below. If the answer isn't in the context, say you don't know.\n\n" +
    $"Context:\n{context}");
```

Then send `systemPrompt + history + question` to the same streaming chat loop from
Part 2.

## Step 6: See the difference

Run it and ask something only the document knows:

```bash
dotnet run
```

```
You: How do I dry the boots?
Assistant: Air-dry them away from direct heat. Never place them on a radiator,
as that damages the waterproof membrane.

You: What is the warranty?
Assistant: A 2-year limited warranty covering manufacturing defects...

You: Who won the 2022 World Cup?
Assistant: I don't know — that isn't in the provided context.
```

The last answer is the point: the assistant is now **grounded**. It answers from
your document and declines when the answer isn't there.

## What's next

Your knowledge base lives in memory, so it's rebuilt on every run and can't scale.
In **Part 4** you'll scaffold the **aichatweb template** and see how it solves
exactly these problems — a real vector store (Qdrant), a document ingestion
pipeline, and semantic search — using the same `IChatClient` and
`IEmbeddingGenerator` abstractions you just used by hand.
