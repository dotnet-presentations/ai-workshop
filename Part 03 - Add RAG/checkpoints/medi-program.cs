// Checkpoint B (MEDI ingestion): complete reference implementation
// This sample shows the same RAG flow, but ingestion/chunking/storage are composed
// through Microsoft.Extensions.DataIngestion.

using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

using ILoggerFactory loggerFactory =
    LoggerFactory.Create(builder => builder.AddSimpleConsole());

IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint");
string apiKey = config["AzureOpenAI:Key"]
    ?? throw new InvalidOperationException("Missing AzureOpenAI:Key");
const string chatModel = "gpt-5-mini";
const string embeddingModel = "text-embedding-3-small";

AzureOpenAIClient azureClient = new(new Uri(endpoint), new AzureKeyCredential(apiKey));
IChatClient chatClient = azureClient.GetChatClient(chatModel).AsIChatClient();
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    azureClient.GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator();

// Reader + chunker + writer are composed into a pipeline.
IngestionDocumentReader reader = new MarkdownReader();

IngestionChunkerOptions chunkerOptions = new(TiktokenTokenizer.CreateForModel(chatModel))
{
    MaxTokensPerChunk = 1200,
    OverlapTokens = 150
};

IngestionChunker<string> chunker =
    new SemanticSimilarityChunker(embeddingGenerator, chunkerOptions);

using SqliteVectorStore vectorStore = new(
    "Data Source=vectors.db;Pooling=false",
    new()
    {
        EmbeddingGenerator = embeddingGenerator
    });

using VectorStoreWriter<string> writer = new(
    vectorStore,
    dimensionCount: 1536,
    new VectorStoreWriterOptions { CollectionName = "product-docs" });

using IngestionPipeline<string> pipeline =
    new(reader, chunker, writer, loggerFactory: loggerFactory);

// Ingest the sample markdown docs.
bool ingestedAnything = false;

await foreach (IngestionResult result in pipeline.ProcessAsync(
    new DirectoryInfo("./sample-docs"),
    searchPattern: "*.md"))
{
    Console.WriteLine($"Completed processing '{result.DocumentId}'. Succeeded: '{result.Succeeded}'.");

    if (result.Succeeded)
    {
        ingestedAnything = true;
    }
    else
    {
        Console.WriteLine($"  {result.Exception?.Message}");
    }
}

// If nothing was ingested the vector store collection was never created, and
// reading writer.VectorStoreCollection below would throw. Fail with a message
// that points at the real problem instead.
if (!ingestedAnything)
{
    Console.WriteLine("No documents were ingested, so there is nothing to search. Check the errors above.");
    return;
}

// Retrieve from the vector store and answer with grounded chat.
VectorStoreCollection<object, Dictionary<string, object?>> collection = writer.VectorStoreCollection;

var history = new List<ChatMessage>
{
    new(ChatRole.System, "You are a product support assistant for Contoso TrailBlazer 3000 boots.")
};

Console.WriteLine("MEDI-based RAG app ready. Type a question (or 'exit' to quit).");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var contexts = new List<string>();

    await foreach (VectorSearchResult<Dictionary<string, object?>> result in
        collection.SearchAsync(input, top: 3))
    {
        if (result.Record.TryGetValue("content", out var content) && content is string text)
        {
            contexts.Add(text);
        }
    }

    string context = string.Join("\n\n---\n\n", contexts);
    var systemPrompt = new ChatMessage(ChatRole.System,
        "Answer using ONLY the context below. If the answer is not in context, say you don't know.\n\n" +
        $"Context:\n{context}");

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

    history.Add(new ChatMessage(ChatRole.User, input));
    history.Add(new ChatMessage(ChatRole.Assistant, answer.ToString()));
}

Console.WriteLine("Goodbye!");
