using System;

namespace Goldfinch.Core.Search;

/// <summary>
/// A single blog post hit returned by <see cref="ILuceneBlogSearchService"/>.
/// </summary>
public class BlogSearchResult
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Url { get; init; }
    public required string Date { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
    public int ReadingMinutes { get; init; }

    /// <summary>Raw title with matched terms wrapped in literal <c>&lt;mark&gt;</c> tags; null if no match. The client re-escapes everything except <c>&lt;mark&gt;</c>.</summary>
    public string? HighlightedTitle { get; init; }

    /// <summary>Raw summary with matched terms wrapped in literal <c>&lt;mark&gt;</c> tags; null if no match. The client re-escapes everything except <c>&lt;mark&gt;</c>.</summary>
    public string? HighlightedSummary { get; init; }
}
