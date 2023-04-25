using Scrummy.Models;

namespace Scrummy.Reporters;

public class IllegalIssueStateReporter : IReporter
{
    public IEnumerable<Observation> GetObservations(IEnumerable<Issue> previous, IEnumerable<Issue> current)
    {
        var observations = new List<Observation>();

        foreach (var issue in current)
        {
            if (issue.Etc.IsNone)
            {
                observations.Add(new() { Issue = issue, Text = "is missing ETC" });
            }

            if (issue.Etc.IsSome && issue.Etc.Value == TimeSpan.Zero && issue.State != IssueState.Done)
            {
                observations.Add(new() { Issue = issue, Text = $"is '{issue.State}' but has ETC of {issue.Etc.Value.TotalHours}h" });
            }

            if (issue.Assignee.IsNone && issue.State == IssueState.InProgress)
            {
                observations.Add(new() { Issue = issue, Text = $"is unassigned but '{issue.State}'" });
            }

            if (issue.State == IssueState.Done && issue.Etc.IsSome && issue.Etc.Value != TimeSpan.Zero)
            {
                observations.Add(new() { Issue = issue, Text = $"is '{issue.State}' but has ETC of {issue.Etc.Value.TotalHours}h" });
            }

            if (issue.Etc.IsSome && issue.Etc.Value > TimeSpan.FromDays(2))
            {
                observations.Add(new() { Issue = issue, Text = $"has high ETC of {issue.Etc.Value.TotalHours}h" });
            }
        }

        return observations;
    }
}
