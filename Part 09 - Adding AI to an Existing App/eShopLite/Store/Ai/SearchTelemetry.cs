using System.Collections.Concurrent;

namespace Store.Ai;

public record SearchEvent(DateTimeOffset At, string Query, int ResultCount, long ElapsedMs);

/// <summary>
/// A rolling in-memory record of what shoppers searched for and whether the store had
/// anything to show them. This is ordinary application telemetry - the kind of data most
/// teams already collect and rarely find time to read.
/// </summary>
public class SearchTelemetry
{
    private const int Capacity = 200;
    private readonly ConcurrentQueue<SearchEvent> events = new();

    public void Record(string query, int resultCount, long elapsedMs)
    {
        events.Enqueue(new SearchEvent(DateTimeOffset.UtcNow, query, resultCount, elapsedMs));

        while (events.Count > Capacity && events.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<SearchEvent> Recent() => events.ToArray();
}
