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

    /// <summary>Title with matched terms wrapped in <c>&lt;mark&gt;</c> (HTML-escaped); null if no match.</summary>
    public string? HighlightedTitle { get; init; }

    /// <summary>Summary with matched terms wrapped in <c>&lt;mark&gt;</c> (HTML-escaped); null if no match.</summary>
    public string? HighlightedSummary { get; init; }
}
