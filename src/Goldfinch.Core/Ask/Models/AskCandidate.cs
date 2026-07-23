namespace Goldfinch.Core.Ask.Models;

/// <summary>
/// A compact representation of a piece of site content used as a selection candidate for the "Ask"
/// feature — a blog post or a page (About, Public Speaking, …). Carries just enough for the model to
/// judge relevance (title + excerpt) plus a stable identifier to select by.
/// <para>
/// For blog posts, <see cref="Url"/> and <see cref="Body"/> are left null and resolved later from
/// the Lucene index only for the posts the model picks. For pages — which aren't in that index —
/// the body and URL are pre-fetched here and carried through, so the gatherer can use them directly.
/// </para>
/// </summary>
public class AskCandidate
{
    /// <summary>Stable identifier the model selects by; maps back to the source web page item.</summary>
    public required int WebPageItemID { get; set; }

    /// <summary>The content title.</summary>
    public required string Title { get; set; }

    /// <summary>The excerpt shown to the model when judging relevance.</summary>
    public required string Excerpt { get; set; }

    /// <summary>Absolute URL, pre-resolved for pages; null for blog posts (resolved from the index).</summary>
    public string? Url { get; set; }

    /// <summary>Plain-text body, pre-fetched for pages; null for blog posts (read from the index).</summary>
    public string? Body { get; set; }
}
