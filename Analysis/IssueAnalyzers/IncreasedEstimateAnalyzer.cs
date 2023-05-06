using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class IncreasedEstimateAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var result =
            previous.IsSome &&
            previous.Value.Estimate.IsSome &&
            current.IsSome &&
            current.Value.Estimate.IsSome &&
            current.Value.Estimate.Value > previous.Value.Estimate.Value;

        if (result)
        {
            var diff = current.Value.Estimate.Value - previous.Value.Estimate.Value;
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, $"increased estimate with {diff.TotalHours}h", Severity.Info));
        }

        return Option<IAnalysisResult>.None();
    }
}