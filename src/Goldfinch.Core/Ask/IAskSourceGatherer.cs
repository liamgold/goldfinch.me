using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Resolves the selected candidates into full sources (body + URL) for the answer step to ground on.
/// Blog-post bodies are read back from the Lucene index (captured at index time); page candidates
/// already carry their crawled body, so those are used directly.
/// </summary>
public interface IAskSourceGatherer
{
    Task<IReadOnlyList<AskSourcePost>> GetSources(IReadOnlyList<AskCandidate> candidates, CancellationToken cancellationToken = default);
}
