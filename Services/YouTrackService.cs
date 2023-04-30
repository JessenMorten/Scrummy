using Scrummy.Models;
using Scrummy.Models.Configuration;
using YouTrackSharp;
using YouTrackSharp.Issues;

namespace Scrummy.Services;

public class YouTrackService : IIssuesService
{
    private readonly YouTrackServiceConfiguration _configuration;

    private readonly Lazy<Connection> _connection;

    private readonly Lazy<YouTrackSharp.Issues.IIssuesService> _issuesService;

    public YouTrackService(YouTrackServiceConfiguration configuration)
    {
        _configuration = configuration;
        _connection = new(() => new BearerTokenConnection(_configuration.Url, _configuration.Token));
        _issuesService = new(() => _connection.Value.CreateIssuesService());
    }

    public async Task<IEnumerable<Models.Issue>> GetIssues()
    {
        var limit = 999;
        var issues = await _issuesService.Value.GetIssues(_configuration.Filter, take: limit);

        if (issues.Count >= limit)
        {
            throw new ArgumentOutOfRangeException($"Reached limit of {limit} with {issues.Count} issues");
        }

        return issues.Select(i => new Models.Issue
        {
            Id = i.Id ?? throw new InvalidOperationException($"Issue has no id"),
            Summary = i.Summary ?? throw new InvalidOperationException($"{i.Id} has no summary"),
            Assignee = GetAssignee(i),
            Url = new Uri($"{_configuration.Url}/issue/{i.Id}"),
            Etc = GetEtc(i),
            Estimate = GetEstimate(i),
            TimeSpent = GetTimeSpent(i),
            State = GetState(i)
        });
    }

    private static Option<TimeSpan> GetEtc(YouTrackSharp.Issues.Issue issue)
    {
        var etc = issue?.GetField("ETC")?.Value;

        if (etc is IEnumerable<string> list)
        {
            var etcString = list.SingleOrDefault();

            if (etcString == null)
            {
                return Option<TimeSpan>.None();
            }

            var etcMinutes = int.Parse(etcString);
            return Option<TimeSpan>.Some(TimeSpan.FromMinutes(etcMinutes));
        }

        return Option<TimeSpan>.None();
    }

    private static IssueState GetState(YouTrackSharp.Issues.Issue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        var state = issue.GetField("State")?.Value;

        if (state is IEnumerable<string> list)
        {
            var stateString = list.SingleOrDefault();
            return stateString switch
            {
                "Not Ready" => IssueState.NotReady,
                "Ready To Start" => IssueState.ReadyToStart,
                "In Progress" => IssueState.InProgress,
                "Ready For Test" => IssueState.ReadyForTest,
                "In Test" => IssueState.InTest,
                "Done" => IssueState.Done,
                _ => throw new NotSupportedException($"{issue.Id} has unexpected state: {stateString}")
            };
        }

        throw new NotSupportedException($"{issue.Id} has no state");
    }

    private static Option<User> GetAssignee(YouTrackSharp.Issues.Issue issue)
    {
        var assignee = issue?.GetField("Assignee")?.Value;

        if (assignee is IEnumerable<Assignee> list && list.Any())
        {
            var user = list.Single();
            return Option<User>.Some(new()
            {
                UserName = user.UserName,
                FullName = user.FullName
            });
        }

        return Option<User>.None();
    }

    private static Option<TimeSpan> GetEstimate(YouTrackSharp.Issues.Issue issue)
    {
        var estimate = issue?.GetField("Estimate")?.Value;

        if (estimate is IEnumerable<string> list)
        {
            var estimateString = list.SingleOrDefault();

            if (estimateString == null)
            {
                return Option<TimeSpan>.None();
            }

            var estimateMinutes = int.Parse(estimateString);
            return Option<TimeSpan>.Some(TimeSpan.FromMinutes(estimateMinutes));
        }

        return Option<TimeSpan>.None();
    }

    private static Option<TimeSpan> GetTimeSpent(YouTrackSharp.Issues.Issue issue)
    {
        var timeSpent = issue?.GetField("Time spent")?.Value;

        if (timeSpent is IEnumerable<string> list)
        {
            var timeSpentString = list.SingleOrDefault();

            if (timeSpentString == null)
            {
                return Option<TimeSpan>.None();
            }

            var timeSpentMinutes = int.Parse(timeSpentString);
            return Option<TimeSpan>.Some(TimeSpan.FromMinutes(timeSpentMinutes));
        }

        return Option<TimeSpan>.None();
    }

}
