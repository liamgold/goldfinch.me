namespace Goldfinch.Core.Search;

/// <summary>
/// Shared constants for the blog Lucene index — the index/strategy name (must match the index
/// created in the admin Search application) and the Lucene document field names used by both the
/// indexing strategy and the query service.
/// </summary>
public static class BlogSearchConstants
{
    /// <summary>
    /// Code name of the index + registered strategy. Must match the index created in the admin UI.
    /// </summary>
    public const string INDEX_NAME = "BlogPosts";

    public const string FIELD_TITLE = "Title";
    public const string FIELD_SUMMARY = "Summary";
    public const string FIELD_CONTENT = "Content";
    public const string FIELD_TAGS = "Tags";
    public const string FIELD_DATE = "Date";
    public const string FIELD_READING_MINUTES = "ReadingMinutes";

    /// <summary>Web page item ID — stored so the Ask feature can retrieve a post's body by ID.</summary>
    public const string FIELD_WEBPAGE_ITEM_ID = "WebPageItemID";

    /// <summary>Average adult reading speed (words per minute) used to estimate reading time.</summary>
    public const int WORDS_PER_MINUTE = 200;
}
