using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.Search;

namespace Goldfinch.Core.BlogPosts;

public static class BlogPostExtensions
{
    /// <summary>
    /// The reading time to display for this post: <see cref="BlogPost.BlogPostReadingMinutes"/> if
    /// it's been set, falling back to a summary-based estimate for posts that haven't been given
    /// one yet (e.g. published before the field existed).
    /// </summary>
    public static int GetEffectiveReadingMinutes(this BlogPost post) =>
        post.BlogPostReadingMinutes > 0
            ? post.BlogPostReadingMinutes
            : ReadingTimeEstimator.EstimateMinutes(post.BaseContentShortDescription);

    /// <summary>
    /// The short excerpt to represent this post: <see cref="BlogPost.BlogPostExcerpt"/> when set,
    /// falling back to <see cref="BlogPost.BaseContentShortDescription"/> (the summary) for posts
    /// without an explicit excerpt. Returns an empty string when neither is populated.
    /// </summary>
    public static string GetEffectiveExcerpt(this BlogPost post) =>
        !string.IsNullOrWhiteSpace(post.BlogPostExcerpt)
            ? post.BlogPostExcerpt
            : post.BaseContentShortDescription ?? string.Empty;
}
