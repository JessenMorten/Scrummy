using Scrummy.Models;

namespace Scrummy.Data;

public class IssueEntity
{
    public string? Id { get; init; }

    public string? Summary { get; init; }

    public string? UserName { get; init; }

    public string? FullName { get; init; }

    public string? Url { get; init; }

    public double? Etc { get; init; }

    public double? Estimate { get; init; }

    public double? TimeSpent { get; init; }

    public int State { get; init; }

    public bool IsBlocked { get; init; }

    public static IssueEntity From(Issue issue)
    {
        return new IssueEntity
        {
            Id = issue.Id,
            Summary = issue.Summary,
            UserName = issue.Assignee.Match<string?>(u => u.UserName, () => null),
            FullName = issue.Assignee.Match<string?>(u => u.FullName, () => null),
            Url = issue.Url.ToString(),
            Etc = issue.Etc.Match<double?>(e => e.TotalHours, () => null),
            Estimate = issue.Estimate.Match<double?>(e => e.TotalHours, () => null),
            TimeSpent = issue.TimeSpent.Match<double?>(e => e.TotalHours, () => null),
            State = (int)issue.State,
            IsBlocked = issue.IsBlocked
        };
    }

    public Issue ToIssue()
    {
        var assignee = string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(FullName)
            ? Option<User>.None()
            : Option<User>.Some(new User { UserName = UserName, FullName = FullName });

        var etc = Etc.HasValue
            ? Option<TimeSpan>.Some(TimeSpan.FromHours(Etc.Value))
            : Option<TimeSpan>.None();

        var estimate = Estimate.HasValue
            ? Option<TimeSpan>.Some(TimeSpan.FromHours(Estimate.Value))
            : Option<TimeSpan>.None();

        var timeSpent = TimeSpent.HasValue
            ? Option<TimeSpan>.Some(TimeSpan.FromHours(TimeSpent.Value))
            : Option<TimeSpan>.None();

        return new Issue
        {
            Id = Id ?? throw new InvalidOperationException("Missing id"),
            Summary = Summary ?? throw new InvalidOperationException("Missing summary"),
            Assignee = assignee,
            Url = new Uri(Url ?? throw new InvalidOperationException("Missing url")),
            Etc = etc,
            Estimate = estimate,
            TimeSpent = timeSpent,
            State = (IssueState)State,
            IsBlocked = IsBlocked
        };
    }
}
