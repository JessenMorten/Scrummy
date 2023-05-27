using Scrummy.Models;

namespace Scrummy.Analysis.IssueAnalyzers;

public class TimeSpentVersusEtcAnalyzer : IIssueAnalyzer
{
    public Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current)
    {
        if (current.IsNone || previous.IsNone)
        {
            return Option<IAnalysisResult>.None();
        }

        var previousTimeSpent = previous.MatchOr(i => i.TimeSpent.SomeOr(TimeSpan.Zero), TimeSpan.Zero).TotalHours;
        var currentTimeSpent = current.MatchOr(i => i.TimeSpent.SomeOr(TimeSpan.Zero), TimeSpan.Zero).TotalHours;

        var previousEtc = previous.MatchOr(i => i.Etc.SomeOr(TimeSpan.Zero), TimeSpan.Zero).TotalHours;
        var currentEtc = current.MatchOr(i => i.Etc.SomeOr(TimeSpan.Zero), TimeSpan.Zero).TotalHours;

        var timeSpentDiff = currentTimeSpent - previousTimeSpent;
        var etcDiff = Math.Abs(currentEtc - previousEtc);

        if (timeSpentDiff > 0 && etcDiff == 0)
        {
            var text = $"time spent increased with {timeSpentDiff}h, but ETC wasn't updated";
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
        }

        if (timeSpentDiff > 0 && etcDiff < timeSpentDiff)
        {
            var text = $"time spent increased with {timeSpentDiff}h, but ETC only decreased with {etcDiff}h";
            return Option<IAnalysisResult>.Some(new IssueResult(current.Value, text, Severity.Warning));
        }

        return Option<IAnalysisResult>.None();
    }
}