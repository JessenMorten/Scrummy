using Scrummy.Models;

namespace Scrummy.Models;

public class Snapshot
{
    public required DateTime Timestamp { get; init; }

    public required IEnumerable<Issue> Issues { get; set; }
}