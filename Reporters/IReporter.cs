using Scrummy.Models;

namespace Scrummy.Reporters;

public interface IReporter
{
    IEnumerable<Observation> GetObservations(IEnumerable<Issue> previous, IEnumerable<Issue> current);
}