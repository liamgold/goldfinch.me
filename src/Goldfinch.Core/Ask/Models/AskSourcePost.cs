namespace Goldfinch.Core.Ask.Models;

/// <summary>
/// A selected blog post with its full body text and resolved URL — the grounding material the
/// answer step reads from and cites.
/// </summary>
public class AskSourcePost
{
    public required int WebPageItemID { get; set; }

    public required string Title { get; set; }

    /// <summary>Absolute URL of the post, for citation links. May be empty if resolution failed.</summary>
    public required string Url { get; set; }

    /// <summary>Plain-text body of the post (crawled and sanitised).</summary>
    public required string Body { get; set; }
}
