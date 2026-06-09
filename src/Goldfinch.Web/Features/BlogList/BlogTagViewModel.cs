namespace Goldfinch.Web.Features.BlogList;

/// <summary>
/// Represents a single taxonomy tag for the blog filter chip row and per-post tag display.
/// </summary>
/// <param name="Slug">Tag code name — appears in the ?tag= query string.</param>
/// <param name="Label">Human-readable display label.</param>
/// <param name="Count">Number of published posts with this tag (0 when not relevant, e.g. per-post display).</param>
public sealed record BlogTagViewModel(string Slug, string Label, int Count);
