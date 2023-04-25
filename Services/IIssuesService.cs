using Scrummy.Models;

namespace Scrummy.Services;

public interface IIssuesService
{
    Task<IEnumerable<Issue>> GetIssues();
}