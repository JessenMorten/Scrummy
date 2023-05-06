using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class NoEtcAnalyzer : IIssueAnalyzer
{
    private static readonly TimeSpan _highEtcThreshold = TimeSpan.FromHours(28);

    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        if (current.IsNone || current.Value.Etc.IsSome)
        {
            return Option<IAnalysisResult>.None();
        }

        var text = $"has no ETC";
        return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
    }
}