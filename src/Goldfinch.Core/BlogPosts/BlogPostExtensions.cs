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
}
