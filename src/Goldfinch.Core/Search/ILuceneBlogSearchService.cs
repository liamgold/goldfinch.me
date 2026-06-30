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
    /// Looks up the reading-minutes value stored for a post by its absolute URL. Returns
    /// <c>null</c> if the post hasn't been indexed yet (e.g. published since the last reindex) —
    /// callers should fall back to <see cref="ReadingTimeEstimator"/> in that case.
    /// </summary>
    /// <param name="url">The post's absolute, root-relative URL (e.g. <c>/blog/my-post</c>).</param>
    int? GetReadingMinutes(string url);
}
