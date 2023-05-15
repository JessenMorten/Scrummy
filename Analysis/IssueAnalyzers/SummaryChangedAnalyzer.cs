using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class SummaryChangedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        if (!previous.IsSome || !current.IsSome)
        {
            return Option<IAnalysisResult>.None();
        }

        var previousSummary = previous.Value.Summary;
        var currentSummary = current.Value.Summary;

        if (previousSummary == currentSummary)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"summary changed from '{previousSummary}' to '{currentSummary}'";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Info));
    }
}