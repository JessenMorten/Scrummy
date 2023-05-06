using Scrummy.Models;

namespace Scrummy.Analysis.SnapshotAnalyzers;

public class PercentBlockedAnalyzer : ISnapshotAnalyzer
{
    public Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current)
    {
        double blockedCount = current.Issues.Count(i => i.IsBlocked);
        double totalCount = current.Issues.Count();
        var percentBlocked = blockedCount / totalCount * 100.0;
        var severity = percentBlocked >= 10 ? Severity.Warning : Severity.Info;
        return Option<IAnalysisResult>.Some(new StatisticResult($"{(int)percentBlocked}% of all issues are blocked", severity));
    }
}