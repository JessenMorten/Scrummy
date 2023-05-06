using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class BlockedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var gotBlocked =
            previous.IsSome &&
            !previous.Value.IsBlocked &&
            current.IsSome &&
            current.Value.IsBlocked;

        var gotUnBlocked =
            previous.IsSome &&
            previous.Value.IsBlocked &&
            current.IsSome &&
            !current.Value.IsBlocked;

        if (gotBlocked)
        {
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, "is now blocked", Severity.Warning));
        }

        if (gotUnBlocked)
        {
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, "is no longer blocked", Severity.Info));
        }

        return Option<IAnalysisResult>.None();
    }
}