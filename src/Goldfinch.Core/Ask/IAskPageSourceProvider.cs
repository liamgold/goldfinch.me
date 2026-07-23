using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Supplies the non-blog pages (About, Public Speaking, …) as "Ask" selection candidates, with their
/// body text and URL already resolved. Unlike blog posts these aren't in the Lucene index, so their
/// bodies are crawled + sanitised here and carried on the candidate. The result is cached, so the
/// crawl only runs when site content changes rather than on every question.
/// </summary>
public interface IAskPageSourceProvider
{
    Task<IReadOnlyList<AskCandidate>> GetPageCandidates(CancellationToken cancellationToken = default);
}
