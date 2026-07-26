// =============================================================================
// CAPABILITY 4 - App capabilities as tools
// =============================================================================
// Once an assistant needs live data, the fastest path is to hand it a database
// connection. That path gives away every business rule the application enforces.
//
// THE AGENT USES THE APP, NOT THE DATABASE. Every tool below calls the existing
// services in Store/, so validation, stock rules, and (in a real app)
// authorization and auditing all still apply.
//
// These are ordinary .NET methods wrapped by AIFunctionFactory - the same shape
// as Part 8, and the same shape you would expose over MCP in Part 5. The
// transport differs; the discipline does not:
//   - start read-only
//   - keep each tool narrow, with a description good enough to be its only docs
//   - return data, not prose - let the calling model do the wording
//   - treat the tool list as a reviewed, versioned API surface
// =============================================================================

using System.ComponentModel;
using Microsoft.Extensions.AI;
using StoreApp.Store;

namespace StoreApp.Ai;

public class StoreTools(Catalog catalog, SearchLog searchLog, OperationsLog operationsLog)
{
    [Description("Search the product catalog by keyword and return matching products with price and stock.")]
    public string SearchProducts(
        [Description("A keyword such as 'jacket', 'tent', or a category name")] string keyword)
    {
        IReadOnlyList<Product> results = catalog.KeywordSearch(keyword);
        return results.Count == 0
            ? $"No products match '{keyword}'."
            : string.Join(Environment.NewLine, results.Select(p => p.ToString()));
    }

    [Description("Get full details for a single product by its numeric ID.")]
    public string GetProduct(
        [Description("The product ID, for example 3")] int productId)
    {
        Product? product = catalog.FindById(productId);
        return product is null
            ? $"No product found with ID {productId}."
            : $"{product}{Environment.NewLine}{product.Description}";
    }

    [Description("List products that are at or below a stock threshold, lowest stock first.")]
    public string GetLowStock(
        [Description("Stock level at or below which a product counts as low, for example 10")] int threshold = 10)
    {
        IReadOnlyList<Product> low = catalog.LowStock(threshold);
        return low.Count == 0
            ? $"No products at or below {threshold} units."
            : string.Join(Environment.NewLine, low.Select(p => $"{p.Name}: {p.InStock} remaining"));
    }

    [Description("List customer searches that returned no results, most frequent first. Use this to find unmet demand.")]
    public string GetUnmetDemand()
    {
        var unmet = searchLog.UnmetDemand();
        return unmet.Count == 0
            ? "Every recorded search returned at least one result."
            : string.Join(Environment.NewLine, unmet.Select(u => $"\"{u.Query}\": {u.Count} search(es), 0 results"));
    }

    [Description("Get recent application log entries, including errors, warnings, and timings.")]
    public string GetRecentLogs(
        [Description("How many recent entries to return, for example 20")] int count = 20)
        => operationsLog.ToText(count);

    /// <summary>The reviewed surface this app chooses to expose. Read-only, on purpose.</summary>
    public IList<AITool> CatalogTools =>
    [
        AIFunctionFactory.Create(SearchProducts),
        AIFunctionFactory.Create(GetProduct),
        AIFunctionFactory.Create(GetLowStock),
    ];

    public IList<AITool> ObservabilityTools =>
    [
        AIFunctionFactory.Create(GetRecentLogs),
    ];

    public IList<AITool> BusinessTools =>
    [
        AIFunctionFactory.Create(GetUnmetDemand),
        AIFunctionFactory.Create(GetLowStock),
    ];
}
