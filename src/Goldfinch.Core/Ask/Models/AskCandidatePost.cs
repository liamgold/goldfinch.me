namespace Goldfinch.Core.Ask.Models;

/// <summary>
/// A compact representation of a blog post used as a selection candidate for the "Ask" feature.
/// Carries just enough for the model to judge relevance (title + excerpt) plus a stable identifier
/// to select by. The full body and URL are resolved later, only for the posts the model picks.
/// </summary>
public class AskCandidatePost
{
    /// <summary>Stable identifier the model selects by; maps back to the source <c>BlogPost</c>.</summary>
    public required int WebPageItemID { get; set; }

    /// <summary>The post title.</summary>
    public required string Title { get; set; }

    /// <summary>The effective excerpt (explicit excerpt, falling back to the summary).</summary>
    public required string Excerpt { get; set; }
}
