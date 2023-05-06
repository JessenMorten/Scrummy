using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class ReassignedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var result =
            previous.IsSome &&
            previous.Value.Assignee.IsSome &&
            current.IsSome &&
            current.Value.Assignee.IsSome &&
            previous.Value.Assignee.Value.UserName != current.Value.Assignee.Value.UserName;

        if (!result)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"got assigned to {current.Value.Assignee.Value.FullName} instead of {previous.Value.Assignee.Value.FullName}";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Info));
    }
}