using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class InProgressAndUnassignedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var inProgressAndUnassigned =
            current.IsSome &&
            current.Value.Assignee.IsNone &&
            current.Value.State == IssueState.InProgress;

        if (!inProgressAndUnassigned)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"is {current.Value.State} and unassigned";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
    }
}