using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Goldfinch.Core.Search;

/// <summary>
/// Extracts plain text from a page's raw Page Builder JSON (the
/// <c>ContentItemCommonDataVisualBuilderWidgets</c> column) for word-counting, without needing the
/// page to be live/rendered. Walks every widget property generically rather than per widget type —
/// collects every string value found anywhere in the tree, strips HTML tags, and trims whitespace.
/// Picks up some non-prose noise (GUIDs, type identifiers, a code block's language tag) alongside
/// real content; negligible for a coarse reading-time estimate.
/// </summary>
public static class PageBuilderTextExtractor
{
    private static readonly Regex TagRegex = new("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static string ExtractText(string? visualBuilderWidgetsJson)
    {
        if (string.IsNullOrWhiteSpace(visualBuilderWidgetsJson))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        using var doc = JsonDocument.Parse(visualBuilderWidgetsJson);
        CollectStrings(doc.RootElement, sb);

        var withoutTags = TagRegex.Replace(sb.ToString(), " ");
        return WhitespaceRegex.Replace(withoutTags, " ").Trim();
    }

    private static void CollectStrings(JsonElement element, StringBuilder sb)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    CollectStrings(property.Value, sb);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectStrings(item, sb);
                }
                break;
            case JsonValueKind.String:
                sb.Append(' ').Append(element.GetString());
                break;
        }
    }
}
