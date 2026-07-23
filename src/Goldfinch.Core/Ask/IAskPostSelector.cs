using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Chooses which content entries are most relevant to a reader's question by letting the model pick
/// from the full candidate list (title + excerpt). This is the "retrieval" step — done by the
/// model itself rather than a search index, which is viable at the current corpus size.
/// </summary>
public interface IAskPostSelector
{
    /// <summary>
    /// Returns the candidates the model judged relevant to the question (at most a small handful),
    /// or an empty list if none are relevant. Recent conversation history is supplied so follow-up
    /// questions ("what about…") can be resolved in context.
    /// </summary>
    Task<IReadOnlyList<AskCandidate>> SelectRelevantCandidates(
        string question,
        IReadOnlyList<AskTurn> history,
        CancellationToken cancellationToken = default);
}
