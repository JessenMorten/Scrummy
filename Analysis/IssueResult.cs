using Scrummy.Models;

namespace Scrummy.Analysis;

public class IssueResult : IAnalysisResult
{
    public string RelatedTo { get; init; }

    public string Text { get; init; }

    public Severity Severity { get; init; }

    public IssueResult(Issue issue, string text, Severity severity)
    {
        RelatedTo = $"{issue.Summary}\n{issue.Url}";
        Text = text;
        Severity = severity;
    }
}