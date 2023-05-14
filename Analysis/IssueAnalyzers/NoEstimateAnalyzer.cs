using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class NoEstimateAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        if (current.IsNone || current.Value.Estimate.IsSome)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"has no estimate";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
    }
}