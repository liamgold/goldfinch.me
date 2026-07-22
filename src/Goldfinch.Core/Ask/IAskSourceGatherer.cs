using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Fetches the full body text (and URL) of the posts the selector picked, so the answer step has
/// real content to ground on. Reuses the same crawl + sanitise path as search indexing, since the
/// post body lives in Page Builder widgets rather than a stored field.
/// </summary>
public interface IAskSourceGatherer
{
    Task<IReadOnlyList<AskSourcePost>> GetSources(IReadOnlyList<int> webPageItemIds, CancellationToken cancellationToken = default);
}
