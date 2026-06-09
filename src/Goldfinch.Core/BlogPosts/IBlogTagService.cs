using CMS.ContentEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

/// <summary>
/// Provides tag/taxonomy data for the blog, backed by the BlogTags taxonomy.
/// </summary>
public interface IBlogTagService
{
    /// <summary>
    /// Resolves a tag slug (the tag code name, as used in ?tag= URLs) to its GUID.
    /// </summary>
    /// <param name="slug">The tag code name, matched case-insensitively.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tag GUID, or <c>null</c> if no tag with that name exists.</returns>
    Task<Guid?> ResolveTagSlugToGuid(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tags in the BlogTags taxonomy together with a count of how many
    /// published posts carry each tag. Tags with no posts are excluded.
    /// </summary>
    /// <param name="languageName">The language to retrieve tag titles in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tags paired with their post counts, in taxonomy order.</returns>
    Task<IReadOnlyList<(Tag Tag, int PostCount)>> GetTagsWithPostCounts(string languageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a set of tag GUIDs (e.g. from a post's tag references) to full tag data.
    /// </summary>
    /// <param name="tagGuids">The tag GUIDs to resolve.</param>
    /// <param name="languageName">The language to retrieve tag titles in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching tags.</returns>
    Task<IEnumerable<Tag>> GetTagsByGuids(IEnumerable<Guid> tagGuids, string languageName, CancellationToken cancellationToken = default);
}
