using Scrummy.Models;

namespace Scrummy.Analysis.SnapshotAnalyzers;

public class TotalEstimateAnalyzer : ISnapshotAnalyzer
{
    public Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current)
    {
        var previousTotalEstimate = GetTotalEstimate(previous);
        var currentTotalEstimate = GetTotalEstimate(current);
        var diff = currentTotalEstimate.TotalHours - previousTotalEstimate.TotalHours;
        Severity severity;
        string message;

        if (diff == 0.0)
        {
            severity = Severity.Info;
            message = $"Total estimate is still {currentTotalEstimate.TotalHours}h";
        }
        else if (diff > 0.0)
        {
            severity = Severity.Warning;
            message = $"Total estimate increased with {diff}h, from {previousTotalEstimate.TotalHours}h to {currentTotalEstimate.TotalHours}h";
        }
        else
        {
            severity = Severity.Info;
            message = $"Total estimate decreased with {Math.Abs(diff)}h, from {previousTotalEstimate.TotalHours}h to {currentTotalEstimate.TotalHours}h";
        }

        var result = new StatisticResult(message, severity);
        return Option<IAnalysisResult>.Some(result);
    }

    private TimeSpan GetTotalEstimate(Snapshot snapshot)
    {
        var totalHours = snapshot.Issues.Sum(i => i.Estimate.SomeOr(TimeSpan.Zero).TotalHours);
        return TimeSpan.FromHours(totalHours);
    }
}