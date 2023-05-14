using Scrummy.Models;

namespace Scrummy.Analysis.SnapshotAnalyzers;

public class TotalEtcAnalyzer : ISnapshotAnalyzer
{
    public Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current)
    {
        var previousTotalEtc = GetTotalEtc(previous);
        var currentTotalEtc = GetTotalEtc(current);
        var diff = currentTotalEtc.TotalHours - previousTotalEtc.TotalHours;
        Severity severity;
        string message;

        if (diff == 0.0)
        {
            severity = Severity.Info;
            message = $"Total ETC is still {currentTotalEtc.TotalHours}h";
        }
        else if (diff > 0.0)
        {
            severity = Severity.Warning;
            message = $"Total ETC increased with {diff}h, from {previousTotalEtc.TotalHours}h to {currentTotalEtc.TotalHours}h";
        }
        else
        {
            severity = Severity.Info;
            message = $"Total ETC decreased with {Math.Abs(diff)}h, from {previousTotalEtc.TotalHours}h to {currentTotalEtc.TotalHours}h";
        }

        var result = new StatisticResult(message, severity);
        return Option<IAnalysisResult>.Some(result);
    }

    private TimeSpan GetTotalEtc(Snapshot snapshot)
    {
        var totalHours = snapshot.Issues.Sum(i => i.Etc.SomeOr(TimeSpan.Zero).TotalHours);
        return TimeSpan.FromHours(totalHours);
    }
}