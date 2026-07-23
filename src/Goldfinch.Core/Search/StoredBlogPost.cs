namespace Goldfinch.Core.Search;

/// <summary>
/// A blog post's stored fields read back from the Lucene index — title, URL, and the full
/// sanitised body. Used by the "Ask" feature to ground answers without re-crawling the page.
/// </summary>
public class StoredBlogPost
{
    public required int WebPageItemID { get; set; }

    public required string Title { get; set; }

    public required string Url { get; set; }

    public required string Body { get; set; }
}
