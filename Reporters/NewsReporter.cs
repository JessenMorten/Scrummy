using Scrummy.Models;

namespace Scrummy.Reporters;

public class NewsReporter : IReporter
{
    public IEnumerable<Observation> GetObservations(IEnumerable<Issue> previous, IEnumerable<Issue> current)
    {
        var observations = new List<Observation>();

        foreach (var currentIssue in current)
        {
            var previousIssue = previous.SingleOrDefault(i => i.Id == currentIssue.Id);
            if (previousIssue is null)
            {
                observations.Add(new() { Issue = currentIssue, Text = "was added" });
            }

            var increasedEtc =
                previousIssue is not null &&
                previousIssue.Etc.IsSome &&
                currentIssue.Etc.IsSome &&
                currentIssue.Etc.Value > previousIssue.Etc.Value;
            if (increasedEtc)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"increased ETC from {previousIssue.Etc.Value.TotalHours}h to {currentIssue.Etc.Value.TotalHours}h" });
            }

            var gotNewAssignee =
                previousIssue is not null &&
                previousIssue.Assignee.IsSome &&
                currentIssue.Assignee.IsSome &&
                previousIssue.Assignee.Value.UserName != currentIssue.Assignee.Value.UserName;
            if (gotNewAssignee)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"got assigned to {currentIssue.Assignee.Value.FullName} instead of {previousIssue.Assignee.Value.FullName}" });
            }

            var gotPickedUp =
                previousIssue is not null &&
                previousIssue.Assignee.IsNone &&
                currentIssue.Assignee.IsSome;
            if (gotPickedUp)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"got picked up by {currentIssue.Assignee.Value.FullName}" });
            }

            var gotAbandoned =
                previousIssue is not null &&
                previousIssue.Assignee.IsSome &&
                currentIssue.Assignee.IsNone;
            if (gotAbandoned)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"got abondoned by {previousIssue.Assignee.Value.FullName}" });
            }

            var gotUnresolved =
                previousIssue is not null &&
                previousIssue.State == IssueState.Done &&
                currentIssue.State != IssueState.Done;
            if (gotUnresolved)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"was moved from '{previousIssue.State}' to '{currentIssue.State}'" });
            }

            var gotResolved =
                previousIssue is not null &&
                previousIssue.State != IssueState.Done &&
                currentIssue.State == IssueState.Done;
            if (gotResolved)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"was resolved" });
            }

            var increasedEstimate =
                previousIssue is not null &&
                previousIssue.Estimate.IsSome &&
                currentIssue.Estimate.IsSome &&
                currentIssue.Estimate.Value > previousIssue.Estimate.Value;
            if (increasedEstimate)
            {
                observations.Add(new() { Issue = currentIssue, Text = $"increased estimate from {previousIssue.Estimate.Value.TotalHours}h to {currentIssue.Estimate.Value.TotalHours}h" });
            }

            var forgetToUpdateEtc =
                previousIssue is not null &&
                previousIssue.Etc.IsSome &&
                currentIssue.Etc.IsSome &&
                currentIssue.Etc.Value == previousIssue.Etc.Value &&
                previousIssue.TimeSpent.IsSome &&
                currentIssue.TimeSpent.IsSome &&
                currentIssue.TimeSpent.Value > previousIssue.TimeSpent.Value;
            if (forgetToUpdateEtc)
            {
                var timeSpent = currentIssue.TimeSpent.Value - previousIssue.TimeSpent.Value;
                observations.Add(new() { Issue = currentIssue, Text = $"increased time spent with {timeSpent.TotalHours}h but ETC did not change" });
            }
        }

        foreach (var previousIssue in previous)
        {
            var currentIssue = current.SingleOrDefault(i => i.Id == previousIssue.Id);
            if (currentIssue is null)
            {
                observations.Add(new() { Issue = previousIssue, Text = "was removed" });
            }
        }

        return observations;
    }
}
