using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class StateChangeAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        if (previous.IsNone || current.IsNone)
        {
            return Option<IAnalysisResult>.None();
        }

        var previousState = previous.Value.State;
        var currentState = current.Value.State;

        var result =
            (currentState == IssueState.Done || previousState == IssueState.Done) &&
            currentState != previousState;

        if (result)
        {
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, $"was moved from '{previousState}' to '{currentState}'", Severity.Info));
        }

        return Option<IAnalysisResult>.None();
    }
}