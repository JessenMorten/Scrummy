using Scrummy.Models;

namespace Scrummy.Analysis.SnapshotAnalyzers;

public class HighestEtcAnalyzer : ISnapshotAnalyzer
{
    public Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current)
    {
        var issues = current.Issues.Where(i => i.Assignee.IsSome && i.Etc.IsSome);

        if (!issues.Any())
        {
            return Option<IAnalysisResult>.None();
        }

        var maxEtc = current.Issues
            .Where(i => i.Assignee.IsSome && i.Etc.IsSome)
            .GroupBy(i => i.Assignee.Value.FullName)
            .Select(i => (FullName: i.Key, A: i.First().Assignee.Value, Etc: i.Sum(x => x.Etc.Value.TotalHours)))
            .MaxBy(x => x.Etc);

        return Option<IAnalysisResult>.Some(new StatisticResult($"{maxEtc.A.FullName} the highest ETC of {maxEtc.Etc}h", Severity.Info));
    }
}