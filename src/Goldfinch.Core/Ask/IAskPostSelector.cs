using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Chooses which blog posts are most relevant to a reader's question by letting the model pick
/// from the full candidate list (title + excerpt). This is the "retrieval" step — done by the
/// model itself rather than a search index, which is viable at the current corpus size.
/// </summary>
public interface IAskPostSelector
{
    /// <summary>
    /// Returns the <c>WebPageItemID</c>s of the posts the model judged relevant to the question
    /// (at most a small handful), or an empty list if none are relevant. Recent conversation
    /// history is supplied so follow-up questions ("what about…") can be resolved in context.
    /// </summary>
    Task<IReadOnlyList<int>> SelectRelevantPostIds(
        string question,
        IReadOnlyList<AskTurn> history,
        CancellationToken cancellationToken = default);
}
