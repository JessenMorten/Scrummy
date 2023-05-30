namespace Scrummy.Models;

public class Issue
{
    public required string Id { get; init; }

    public required string Summary { get; init; }

    public required Option<User> Assignee { get; init; }

    public required Uri Url { get; init; }

    public required Option<TimeSpan> Etc { get; init; }

    public required Option<TimeSpan> Estimate { get; init; }

    public required Option<TimeSpan> TimeSpent { get; init; }

    public required IssueState State { get; init; }

    public required bool IsBlocked { get; init; }
}
