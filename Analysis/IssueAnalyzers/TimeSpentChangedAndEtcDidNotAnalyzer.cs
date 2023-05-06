using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class TimeSpentChangedAndEtcDidNotAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        var forgetToUpdateEtc =
            previous.IsSome &&
            current.IsSome &&
            previous.Value.Etc.IsSome &&
            current.Value.Etc.IsSome &&
            current.Value.Etc.Value == previous.Value.Etc.Value &&
            current.Value.TimeSpent.IsSome &&
            current.Value.TimeSpent.Value > previous.Value.TimeSpent.SomeOr(TimeSpan.Zero);

        if (forgetToUpdateEtc)
        {
            var diff = current.Value.TimeSpent.Value - previous.Value.TimeSpent.Value;
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, $"time spent was inceased by {diff.TotalHours}h, but ETC didn't change", Severity.Warning));
        }

        return Option<IAnalysisResult>.None();
    }
}