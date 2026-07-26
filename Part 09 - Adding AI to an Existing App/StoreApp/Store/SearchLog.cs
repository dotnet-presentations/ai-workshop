// =============================================================================
// THE EXISTING APP - search telemetry the store already collects
// =============================================================================
// Almost every storefront already records what people searched for and how many
// results came back. Nobody reads it. Searches that returned zero results are
// unmet demand sitting in a table.
//
// Part 9 section 3 turns this existing signal into a business report. Note that
// no new data collection was needed to make that possible.
// =============================================================================

namespace StoreApp.Store;

public record SearchEvent(DateTimeOffset Timestamp, string Query, int ResultCount);

public class SearchLog
{
    private readonly List<SearchEvent> _events = [];

    public SearchLog()
    {
        // Seeded history so the reporting demo has something to say on first run.
        var now = DateTimeOffset.UtcNow;
        Add(now.AddDays(-6), "waterproof jacket", 2);
        Add(now.AddDays(-6), "hiking boots", 1);
        Add(now.AddDays(-5), "snowshoes", 0);
        Add(now.AddDays(-5), "kids sleeping bag", 0);
        Add(now.AddDays(-4), "rain shell", 1);
        Add(now.AddDays(-4), "snowshoes", 0);
        Add(now.AddDays(-3), "avalanche beacon", 0);
        Add(now.AddDays(-3), "trekking poles", 1);
        Add(now.AddDays(-2), "snowshoes", 0);
        Add(now.AddDays(-2), "kids sleeping bag", 0);
        Add(now.AddDays(-2), "winter gloves", 0);
        Add(now.AddDays(-1), "headlamp", 1);
        Add(now.AddDays(-1), "snowshoes", 0);
        Add(now.AddHours(-6), "water filter", 1);
        Add(now.AddHours(-3), "winter gloves", 0);
    }

    private void Add(DateTimeOffset at, string query, int count) =>
        _events.Add(new SearchEvent(at, query, count));

    /// <summary>Called by the app on every search, including the AI-powered one.</summary>
    public void Record(string query, int resultCount) =>
        _events.Add(new SearchEvent(DateTimeOffset.UtcNow, query, resultCount));

    public IReadOnlyList<SearchEvent> Recent(int count = 50) =>
        _events.OrderByDescending(e => e.Timestamp).Take(count).ToList();

    /// <summary>Queries that found nothing, most frequent first. This is the demand signal.</summary>
    public IReadOnlyList<(string Query, int Count)> UnmetDemand() =>
        _events.Where(e => e.ResultCount == 0)
            .GroupBy(e => e.Query, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Query: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

    public IReadOnlyList<(string Query, int Count)> TopQueries(int take = 5) =>
        _events.GroupBy(e => e.Query, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Query: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToList();
}
