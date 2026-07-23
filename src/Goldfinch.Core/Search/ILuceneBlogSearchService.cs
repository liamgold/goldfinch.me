using System.Collections.Generic;

namespace Goldfinch.Core.Search;

/// <summary>
/// Queries the <c>BlogPosts</c> Lucene index for the live search endpoint, ranking matches
/// title &gt; summary &gt; body and generating <c>&lt;mark&gt;</c> highlights.
/// </summary>
public interface ILuceneBlogSearchService
{
    /// <summary>
    /// Searches blog posts, optionally scoped to a single tag slug.
    /// </summary>
    /// <param name="query">The trimmed, non-empty search query.</param>
    /// <param name="tag">Optional tag slug to scope results to.</param>
    /// <param name="limit">Maximum number of post hits to return.</param>
    IReadOnlyList<BlogSearchResult> SearchPosts(string query, string? tag, int limit);

    /// <summary>
    /// Reads back the stored title, URL, and full body for the given posts from the index (used by
    /// the "Ask" feature to ground answers without re-crawling). Posts not present in the index are
    /// omitted.
    /// </summary>
    /// <param name="webPageItemIds">The web page item IDs to fetch.</param>
    IReadOnlyList<StoredBlogPost> GetStoredPosts(IReadOnlyList<int> webPageItemIds);
}
