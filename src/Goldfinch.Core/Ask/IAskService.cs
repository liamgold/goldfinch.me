using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Orchestrates the "Ask" flow end to end: select relevant posts, gather their content, and
/// generate a grounded, cited answer to the reader's question — carrying prior conversation turns
/// so follow-up questions stay in context.
/// </summary>
public interface IAskService
{
    Task<AskResult> Ask(
        string question,
        IReadOnlyList<AskTurn> history,
        CancellationToken cancellationToken = default);
}
