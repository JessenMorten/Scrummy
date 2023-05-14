using Scrummy.Models;

namespace Scrummy.Analysis.SnapshotAnalyzers;

public class DeviationAnalyzer : ISnapshotAnalyzer
{
    public Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current)
    {
        var previousDeviation = GetDeviation(previous);
        var currentDeviation = GetDeviation(current);

        var previousMessage = GetMessage(previousDeviation);
        var currentMessage = GetMessage(currentDeviation);

        var message = previousDeviation == currentDeviation ? $"We're still {currentMessage}" : $"Previously {previousMessage}, now {currentMessage}";
        var severity = currentDeviation < previousDeviation ? Severity.Warning : Severity.Info;

        var result = new StatisticResult(message, severity);
        return Option<IAnalysisResult>.Some(result);
    }

    private string GetMessage(double deviation)
    {
        if (deviation > 0.0)
        {
            return $"{deviation}h ahead";
        }
        else if (deviation < 0.0)
        {
            return $"{Math.Abs(deviation)}h behind";
        }

        return "on track";
    }

    private double GetDeviation(Snapshot snapshot)
    {
        var totalEstimate = snapshot.Issues.Sum(i => i.Estimate.SomeOr(TimeSpan.Zero).TotalHours);
        var totalEtc = snapshot.Issues.Sum(i => i.Etc.SomeOr(TimeSpan.Zero).TotalHours);
        var totalTimeSpent = snapshot.Issues.Sum(i => i.TimeSpent.SomeOr(TimeSpan.Zero).TotalHours);
        var idealEtc = totalEstimate - totalTimeSpent;
        var hoursAhead = idealEtc - totalEtc;
        return hoursAhead;
    }
}