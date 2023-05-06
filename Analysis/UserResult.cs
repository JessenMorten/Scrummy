using Scrummy.Models;

namespace Scrummy.Analysis;

public class UserResult : IAnalysisResult
{
    public string RelatedTo { get; init; }

    public string Text { get; init; }

    public Severity Severity { get; init; }

    public UserResult(User user, string text, Severity severity)
    {
        RelatedTo = user.FullName;
        Text = text;
        Severity = severity;
    }
}