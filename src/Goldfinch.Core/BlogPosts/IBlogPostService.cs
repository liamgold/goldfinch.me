using Goldfinch.Core.ContentTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

/// <summary>
/// Provides access to blog post content items.
/// </summary>
public interface IBlogPostService
{
    /// <summary>
    /// Retrieves a single blog post by its web page item ID.
    /// </summary>
    /// <param name="webPageItemID">The web page item ID of the blog post.</param>
    /// <returns>The matching <see cref="BlogPost"/>, or <c>null</c> if not found.</returns>
    Task<BlogPost?> GetBlogPost(int webPageItemID);

    /// <summary>
    /// Retrieves all published blog posts ordered by date, for use in "latest posts" displays.
    /// </summary>
    /// <returns>A collection of blog posts.</returns>
    Task<IEnumerable<BlogPost>> GetLatestBlogPosts();

    /// <summary>
    /// Retrieves a paginated set of blog posts.
    /// </summary>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <returns>A collection of blog posts for the requested page.</returns>
    Task<IEnumerable<BlogPost>> GetBlogPosts(int pageIndex);

    /// <summary>
    /// Returns the total number of pages of blog posts based on the configured page size.
    /// </summary>
    /// <returns>The total page count.</returns>
    Task<int> GetBlogPageCount();
}
