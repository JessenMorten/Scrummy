using Scrummy.Models;

namespace Scrummy.Analysis;

public interface IIssueAnalyzer
{
    Option<IAnalysisResult> Analyze(Option<Issue> previous, Option<Issue> current);
}
