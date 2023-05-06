using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class RemovedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        return previous.IsSome && current.IsNone
            ? Option<IAnalysisResult>.Some(new IssueResult(previous.Value, "was removed", Severity.Info))
            : Option<IAnalysisResult>.None();
    }
}