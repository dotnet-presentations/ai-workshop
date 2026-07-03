// =============================================================================
// Part 3 - Add RAG by hand (console)
// =============================================================================
// This continues the Part 2 chat app. Now we ground the model in a document it
// has never seen, by building a Retrieval-Augmented Generation (RAG) loop BY HAND:
//
//   1. IEmbeddingGenerator      (turn text into vectors)
//   2. Chunk the document       (split into retrievable pieces)
//   3. Embed + store            (an in-memory list - no vector database)
//   4. Cosine similarity search (naive top-k retrieval, written by hand)
//   5. Augment the prompt        (inject retrieved context, then answer)
//
// Theme: the same swappable-abstraction idea from Part 2 (swap the chat provider)
// now applies to embeddings and vector stores. In Part 4 the template will do all
// of this for you - and you'll understand every moving part because you built it.
//
// Adapted with thanks from Steve Sanderson's dotnet-ai-workshop
// (https://github.com/SteveSandersonMS/dotnet-ai-workshop), chapters 2, 3, and 6.
// =============================================================================

using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// -----------------------------------------------------------------------------
// 0. Secrets-first configuration (same pattern as Part 2)
// -----------------------------------------------------------------------------
//   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
//   dotnet user-secrets set "AzureOpenAI:Key" "YOUR-KEY"
//   dotnet user-secrets set "AzureOpenAI:ChatModel" "gpt-4.1-mini"
//   dotnet user-secrets set "AzureOpenAI:EmbeddingModel" "text-embedding-3-small"
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException(
        "Missing 'AzureOpenAI:Endpoint'. Run: dotnet user-secrets set \"AzureOpenAI:Endpoint\" \"https://YOUR-RESOURCE.openai.azure.com/\"");
string key = config["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException(
        "Missing 'AzureOpenAI:Key'. Run: dotnet user-secrets set \"AzureOpenAI:Key\" \"YOUR-KEY\"");
string chatModel = config["AzureOpenAI:ChatModel"] ?? "gpt-4.1-mini";
string embeddingModel = config["AzureOpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

// One Azure client, two abstractions: a chat client AND an embedding generator.
var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

IChatClient chatClient = azureClient.GetChatClient(chatModel).AsIChatClient();

// -----------------------------------------------------------------------------
// 1. IEmbeddingGenerator
// -----------------------------------------------------------------------------
// An embedding turns text into a vector (an array of floats). Texts with similar
// meaning produce vectors that point in similar directions - that's what lets us
// search by meaning instead of by keyword.
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    azureClient.GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator();

// -----------------------------------------------------------------------------
// 2. Chunk the document
// -----------------------------------------------------------------------------
// We split the document into paragraph-sized chunks. Real systems use smarter
// chunking, but paragraphs are enough to see RAG work. Each chunk must be small
// enough to embed meaningfully yet large enough to carry a complete idea.
string docPath = Path.Combine(AppContext.BaseDirectory, "sample-docs", "contoso-trailblazer-3000.md");
string document = await File.ReadAllTextAsync(docPath);

string[] chunks = document
    .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Where(c => c.Length > 0)
    .ToArray();

// -----------------------------------------------------------------------------
// 3. Embed + store (in-memory - no vector database)
// -----------------------------------------------------------------------------
// We embed every chunk once at startup and keep the vectors in a plain list.
// This is deliberately naive: it doesn't persist and it won't scale - which is
// exactly the motivation for the real vector store you'll meet in Part 4.
Console.WriteLine($"Embedding {chunks.Length} chunks from the product guide...");

GeneratedEmbeddings<Embedding<float>> embeddings =
    await embeddingGenerator.GenerateAsync(chunks);

var store = new List<(string Text, ReadOnlyMemory<float> Vector)>();
for (int i = 0; i < chunks.Length; i++)
{
    store.Add((chunks[i], embeddings[i].Vector));
}

Console.WriteLine("Knowledge base ready. Ask about the Contoso TrailBlazer 3000 boots.");
Console.WriteLine("(Type 'exit' to quit.)");
Console.WriteLine();

// -----------------------------------------------------------------------------
// The chat loop - now grounded in the document
// -----------------------------------------------------------------------------
var history = new List<ChatMessage>();

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    // -------------------------------------------------------------------------
    // 4. Cosine similarity search (written by hand)
    // -------------------------------------------------------------------------
    // Embed the question with the SAME generator, then rank every stored chunk by
    // cosine similarity and take the top matches. This is the heart of retrieval.
    ReadOnlyMemory<float> questionVector =
        (await embeddingGenerator.GenerateAsync(input)).Vector;

    const int topK = 3;
    var topChunks = store
        .Select(item => (item.Text, Score: CosineSimilarity(questionVector.Span, item.Vector.Span)))
        .OrderByDescending(x => x.Score)
        .Take(topK)
        .Select(x => x.Text)
        .ToArray();

    // -------------------------------------------------------------------------
    // 5. Augment the prompt, then answer
    // -------------------------------------------------------------------------
    // We rebuild the system message every turn with the freshly retrieved context
    // and instruct the model to answer ONLY from it. That grounding is what turns
    // a general chat model into a document-aware assistant.
    string context = string.Join("\n\n---\n\n", topChunks);
    var systemPrompt = new ChatMessage(ChatRole.System,
        "You are a product support assistant. Answer the user's question using ONLY " +
        "the context below. If the answer isn't in the context, say you don't know.\n\n" +
        $"Context:\n{context}");

    // Send: system prompt (with context) + the running conversation + this question.
    var messages = new List<ChatMessage> { systemPrompt };
    messages.AddRange(history);
    messages.Add(new ChatMessage(ChatRole.User, input));

    Console.Write("Assistant: ");
    var answer = new System.Text.StringBuilder();
    await foreach (ChatResponseUpdate update in chatClient.GetStreamingResponseAsync(messages))
    {
        Console.Write(update.Text);
        answer.Append(update.Text);
    }
    Console.WriteLine();
    Console.WriteLine();

    // Keep the conversation going (history holds the user/assistant turns only;
    // the context is re-retrieved and re-injected fresh each turn).
    history.Add(new ChatMessage(ChatRole.User, input));
    history.Add(new ChatMessage(ChatRole.Assistant, answer.ToString()));
}

Console.WriteLine("Goodbye!");

// -----------------------------------------------------------------------------
// Naive cosine similarity: dot(a, b) / (|a| * |b|).
// Returns a value from -1 (opposite) to 1 (identical direction).
// -----------------------------------------------------------------------------
static float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
{
    float dot = 0f, magA = 0f, magB = 0f;
    for (int i = 0; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        magA += a[i] * a[i];
        magB += b[i] * b[i];
    }

    return magA == 0f || magB == 0f
        ? 0f
        : dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
}
