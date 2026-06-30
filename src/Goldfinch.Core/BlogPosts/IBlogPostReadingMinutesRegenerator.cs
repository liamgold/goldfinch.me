using System.Threading;
using System.Threading.Tasks;

namespace Goldfinch.Core.BlogPosts;

/// <summary>
/// Recomputes <see cref="ContentTypes.BlogPost.BlogPostReadingMinutes"/> from a post's current
/// Page Builder content, for the admin "Regenerate" form component.
/// </summary>
public interface IBlogPostReadingMinutesRegenerator
{
    /// <summary>
    /// Computes the reading-minutes estimate for the given content item's latest version (the
    /// draft being edited, if one exists, otherwise the published version).
    /// </summary>
    Task<int> RegenerateAsync(int contentItemId, string languageName, CancellationToken cancellationToken = default);
}
