using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class HighEtcAnalyzer : IIssueAnalyzer
{
    private static readonly TimeSpan _highEtcThreshold = TimeSpan.FromHours(28);

    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var hasHighEtc =
            current.IsSome &&
            current.Value.Etc.IsSome &&
            current.Value.Etc.Value > _highEtcThreshold;

        if (!hasHighEtc)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"has high ETC: {current.Value.Etc.Value.TotalHours}h";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
    }
}