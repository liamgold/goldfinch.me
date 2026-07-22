using System.Collections.Generic;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Supplies the content the "Ask" feature reasons over: the compact candidate list the model
/// selects from, and (later) the full body text of the posts it picks.
/// </summary>
public interface IAskContentService
{
    /// <summary>
    /// Returns every published blog post as a compact selection candidate (title + effective
    /// excerpt + identifier), ordered newest first. This is the full set the model chooses from —
    /// at the current corpus size it fits comfortably in a single prompt, so no retrieval index
    /// is needed.
    /// </summary>
    Task<IReadOnlyList<AskCandidatePost>> GetCandidates();
}
