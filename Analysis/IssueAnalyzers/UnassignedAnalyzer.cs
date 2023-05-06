using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class UnassignedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var result =
            previous.IsSome &&
            previous.Value.Assignee.IsSome &&
            current.IsSome &&
            current.Value.Assignee.IsNone;

        if (!result)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"got abandoned by {previous.Value.Assignee.Value.FullName}";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
    }
}