using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class AddedAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        return previous.IsSome
            ? Option<IAnalysisResult>.None()
            : Option<IAnalysisResult>.Some(new IssueResult(current.Value, "was added", Severity.Info));
    }
}