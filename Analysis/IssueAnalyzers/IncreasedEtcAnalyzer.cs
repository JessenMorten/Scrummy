using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class IncreasedEtcAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var result =
            previous.IsSome &&
            previous.Value.Etc.IsSome &&
            current.IsSome &&
            current.Value.Etc.IsSome &&
            current.Value.Etc.Value > previous.Value.Etc.Value;

        if (result)
        {
            var diff = current.Value.Etc.Value - previous.Value.Etc.Value;
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, $"increased ETC with {diff.TotalHours}h", Severity.Info));
        }

        return Option<IAnalysisResult>.None();
    }
}