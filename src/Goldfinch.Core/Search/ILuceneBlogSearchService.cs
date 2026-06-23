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
}
