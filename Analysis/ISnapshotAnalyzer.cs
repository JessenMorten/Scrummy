using Scrummy.Models;

namespace Scrummy.Analysis;

public interface ISnapshotAnalyzer
{
    Option<IAnalysisResult> Analyze(Snapshot previous, Snapshot current);
}