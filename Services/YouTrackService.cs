using Scrummy.Models;
using Scrummy.Models.Configuration;
using YouTrackSharp;
using YouTrackSharp.Issues;

namespace Scrummy.Services;

public class YouTrackService
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
        var issues = new List<YouTrackSharp.Issues.Issue>();
        var limit = 420;
        var offset = 0;

        while (offset >= 0)
        {
            var batch = await _issuesService.Value.GetIssues(_configuration.Filter, skip: offset, take: limit);
            issues.AddRange(batch);
            offset = batch.Count >= limit ? issues.Count : -1;
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
            State = GetState(i),
            IsBlocked = HasTag(i, "blocked", "waiting", "needs more info")
        });
    }

    private static Option<TimeSpan> GetEtc(YouTrackSharp.Issues.Issue issue)
    {
        var etcString = GetSingleStringValue(issue, "ETC");
        return etcString.IsSome
            ? Option<TimeSpan>.Some(TimeSpan.FromMinutes(int.Parse(etcString.Value)))
            : Option<TimeSpan>.None();
    }

    private static IssueState GetState(YouTrackSharp.Issues.Issue issue)
    {
        var stateString = GetSingleStringValue(issue, "State");

        if (stateString.IsSome)
        {
            return stateString.Value switch
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
        var estimateString = GetSingleStringValue(issue, "Estimate");
        return estimateString.IsSome
            ? Option<TimeSpan>.Some(TimeSpan.FromMinutes(int.Parse(estimateString.Value)))
            : Option<TimeSpan>.None();
    }

    private static Option<TimeSpan> GetTimeSpent(YouTrackSharp.Issues.Issue issue)
    {
        var timeSpentString = GetSingleStringValue(issue, "Time spent");
        return timeSpentString.IsSome
            ? Option<TimeSpan>.Some(TimeSpan.FromMinutes(int.Parse(timeSpentString.Value)))
            : Option<TimeSpan>.None();
    }

    private static Option<string> GetSingleStringValue(YouTrackSharp.Issues.Issue issue, string field)
    {
        var val = issue?.GetField(field)?.Value;

        if (val is IEnumerable<string> list)
        {
            var stringVal = list.SingleOrDefault();

            if (stringVal is not null)
            {
                return Option<string>.Some(stringVal);
            }
        }

        return Option<string>.None();
    }

    private static bool HasTag(YouTrackSharp.Issues.Issue issue, params string[] tags)
    {
        var testTags = tags.Select(t => t.ToUpperInvariant());
        var issueTags = issue.Tags.Select(t => t.Value.ToUpperInvariant());

        foreach (var testTag in testTags)
        {
            if (issueTags.Any(issueTag => issueTag.Contains(testTag)))
            {
                return true;
            }
        }

        return false;
    }
}
