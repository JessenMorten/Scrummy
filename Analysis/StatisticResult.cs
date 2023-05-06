using Scrummy.Models;

namespace Scrummy.Analysis;

public class StatisticResult : IAnalysisResult
{
    public string RelatedTo => "Statistics";

    public string Text { get; init; }

    public Severity Severity { get; init; }

    public StatisticResult(string text, Severity severity)
    {
        Text = text;
        Severity = severity;
    }
}