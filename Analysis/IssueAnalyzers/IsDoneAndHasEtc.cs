using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class IsDoneAndHasEtcAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var result =
            current.IsSome &&
            current.Value.State == IssueState.Done &&
            current.Value.Etc.IsSome &&
            current.Value.Etc.Value > TimeSpan.Zero;

        if (!result)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"is {current.Value.State} but has ETC of {current.Value.Etc.Value.TotalHours}h";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
    }
}