using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class AssignedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var wasAssigned =
            previous.MatchOr(i => i.Assignee.IsNone, false) &&
            current.MatchOr(i => i.Assignee.IsSome, false);

        if (!wasAssigned)
        {
            return Option<IAnalysisResult>.None();
        }

        var assignee = current.MatchOr(i => i.Assignee.MatchOr(a => a.FullName, string.Empty), string.Empty);
        var text = $"was picked up by {assignee}";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Info));
    }
}