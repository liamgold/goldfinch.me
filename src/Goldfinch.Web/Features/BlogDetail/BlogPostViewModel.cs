using CMS.Websites;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.BlogList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.BlogDetail;

public class BlogPostViewModel
{
    public required string Title { get; set; }

    public required string Summary { get; set; }

    /// <summary>
    /// On-page standfirst / GEO overview. Falls back to the short description when the
    /// BlogPostExcerpt field is empty. Used only by the detail page — the card and meta
    /// description keep using Summary (BaseContentShortDescription).
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;

    public required string Url { get; set; }

    public DateTime BlogPostDate { get; set; }

    /// <summary>Terminal-style filename shown on the card chrome, e.g. "my-cool-post.md".</summary>
    public string Filename { get; set; } = "post.md";

    public string Schema { get; set; } = string.Empty;

    /// <summary>Tags assigned to this post, for display in list view rows.</summary>
    public IReadOnlyList<BlogTagViewModel> Tags { get; set; } = [];

    /// <summary>
    /// Reading time in minutes — see <see cref="BlogPostExtensions.GetEffectiveReadingMinutes"/>.
    /// </summary>
    public int ReadingMinutes { get; set; }

    public static async Task<BlogPostViewModel> GetViewModelAsync(
        BlogPost blogPost,
        IWebPageUrlRetriever pageUrlRetriever)
    {
        var url = (await pageUrlRetriever.Retrieve(blogPost)).RelativePath;

        return new BlogPostViewModel
        {
            Title = blogPost.BaseContentTitle,
            Summary = blogPost.BaseContentShortDescription,
            Excerpt = string.IsNullOrWhiteSpace(blogPost.BlogPostExcerpt)
                ? blogPost.BaseContentShortDescription
                : blogPost.BlogPostExcerpt,
            BlogPostDate = blogPost.BlogPostDate,
            Url = url,
            Filename = FilenameFromUrl(url),
            ReadingMinutes = blogPost.GetEffectiveReadingMinutes(),
        };
    }

    /// <summary>
    /// Derives a "{slug}.md" label from the page's relative URL. Pulls the last
    /// path segment — so <c>/blog/my-cool-post</c> becomes <c>my-cool-post.md</c>.
    /// Matches Kentico's canonical slug (which the editor can override in the CMS)
    /// rather than re-slugifying the title, so the card chrome always agrees with
    /// the post URL.
    /// </summary>
    public static string FilenameFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "post.md";
        var lastSegment = url.TrimEnd('/').Split('/').LastOrDefault();
        if (string.IsNullOrWhiteSpace(lastSegment)) return "post.md";
        return $"{lastSegment}.md";
    }
}
