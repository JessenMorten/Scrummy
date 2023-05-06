using Scrummy.Models;

namespace Scrummy.Analysis.SnapshotAnalyzers;

public class LowestEtcAnalyzer : ISnapshotAnalyzer
{
    public Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current)
    {
        var issues = current.Issues.Where(i => i.Assignee.IsSome && i.Etc.IsSome);

        if (!issues.Any())
        {
            return Option<IAnalysisResult>.None();
        }

        var minEtc = current.Issues
            .Where(i => i.Assignee.IsSome && i.Etc.IsSome)
            .GroupBy(i => i.Assignee.Value.FullName)
            .Select(i => (FullName: i.Key, A: i.First().Assignee.Value, Etc: i.Sum(x => x.Etc.Value.TotalHours)))
            .MinBy(x => x.Etc);

        return Option<IAnalysisResult>.Some(new StatisticResult($"{minEtc.A.FullName} has the lowest ETC of {minEtc.Etc}h", Severity.Info));
    }
}