namespace Scrummy.Analysis;

public interface IAnalysisResult
{
    string RelatedTo { get; }

    string Text { get; }

    Severity Severity { get; }
}