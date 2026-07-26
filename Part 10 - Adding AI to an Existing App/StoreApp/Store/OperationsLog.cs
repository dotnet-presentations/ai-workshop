// =============================================================================
// THE EXISTING APP - operational telemetry
// =============================================================================
// A stand-in for the structured logs and traces your app already emits (in a
// real app this is OpenTelemetry data you would read from the Aspire dashboard,
// Application Insights, or a log store).
//
// The seeded data contains a deliberate story: the payment gateway starts timing
// out, checkout latency climbs, and orders begin failing. A human can find that
// by reading carefully. Part 10 section 2 has a local model find it instead.
// =============================================================================

namespace StoreApp.Store;

public record LogEntry(DateTimeOffset Timestamp, string Level, string Service, string Message)
{
    public override string ToString() =>
        $"{Timestamp:HH:mm:ss} [{Level,-5}] {Service,-16} {Message}";
}

public class OperationsLog
{
    private readonly List<LogEntry> _entries = [];

    public OperationsLog()
    {
        var t = DateTimeOffset.UtcNow.AddMinutes(-45);

        Add(ref t, "INFO", "catalog-api", "GET /products completed in 41ms");
        Add(ref t, "INFO", "search-api", "Query 'hiking boots' returned 1 result in 78ms");
        Add(ref t, "INFO", "checkout-api", "Order ORD-4417 placed, total $189.99, completed in 320ms");
        Add(ref t, "INFO", "catalog-api", "GET /products/3 completed in 22ms");
        Add(ref t, "WARN", "payments", "Dependency 'payment-gateway' responded in 2841ms (threshold 1000ms)");
        Add(ref t, "INFO", "checkout-api", "Order ORD-4418 placed, total $329.99, completed in 3120ms");
        Add(ref t, "WARN", "payments", "Dependency 'payment-gateway' responded in 4210ms (threshold 1000ms)");
        Add(ref t, "INFO", "search-api", "Query 'snowshoes' returned 0 results in 61ms");
        Add(ref t, "ERROR", "payments", "Dependency 'payment-gateway' timed out after 5000ms. TraceId 7f3a91");
        Add(ref t, "ERROR", "checkout-api", "Order ORD-4419 failed: PaymentTimeoutException. TraceId 7f3a91");
        Add(ref t, "INFO", "catalog-api", "GET /products completed in 38ms");
        Add(ref t, "ERROR", "payments", "Dependency 'payment-gateway' timed out after 5000ms. TraceId 8b1c04");
        Add(ref t, "ERROR", "checkout-api", "Order ORD-4420 failed: PaymentTimeoutException. TraceId 8b1c04");
        Add(ref t, "WARN", "checkout-api", "Retry 1 of 3 for order ORD-4420");
        Add(ref t, "ERROR", "payments", "Dependency 'payment-gateway' timed out after 5000ms. TraceId 8b1c04");
        Add(ref t, "INFO", "search-api", "Query 'winter gloves' returned 0 results in 55ms");
        Add(ref t, "ERROR", "checkout-api", "Order ORD-4421 failed: PaymentTimeoutException. TraceId c92d17");
        Add(ref t, "INFO", "catalog-api", "GET /products/7 completed in 25ms");
        Add(ref t, "WARN", "payments", "Circuit breaker opened for 'payment-gateway'");
        Add(ref t, "INFO", "checkout-api", "Checkout disabled, returning 503 to clients");
    }

    private void Add(ref DateTimeOffset t, string level, string service, string message)
    {
        _entries.Add(new LogEntry(t, level, service, message));
        t = t.AddMinutes(2);
    }

    public IReadOnlyList<LogEntry> Recent(int count = 40) =>
        _entries.OrderByDescending(e => e.Timestamp).Take(count).OrderBy(e => e.Timestamp).ToList();

    public IReadOnlyList<LogEntry> Errors() =>
        _entries.Where(e => e.Level is "ERROR").ToList();

    /// <summary>The plain-text view handed to a model as grounding context.</summary>
    public string ToText(int count = 40) =>
        string.Join(Environment.NewLine, Recent(count).Select(e => e.ToString()));
}
