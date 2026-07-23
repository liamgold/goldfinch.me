using System.Collections.Generic;
using System.Threading.Tasks;
using Goldfinch.Core.Ask.Models;

namespace Goldfinch.Core.Ask;

/// <summary>
/// Supplies the content the "Ask" feature reasons over: the compact candidate list the model
/// selects from (blog posts plus pages such as About and Public Speaking), and the full body text
/// of the entries it picks.
/// </summary>
public interface IAskContentService
{
    /// <summary>
    /// Returns the full set of selection candidates (title + excerpt + identifier): every published
    /// blog post, newest first, followed by the site's content pages. At the current corpus size
    /// this fits comfortably in a single prompt, so no retrieval index is needed.
    /// </summary>
    Task<IReadOnlyList<AskCandidate>> GetCandidates();
}
