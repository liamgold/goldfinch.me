using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;

namespace Goldfinch.Core.Search;

/// <summary>
/// Converts a rendered HTML page into clean, indexable plain text. Prefers the post body
/// container (<c>.post-body</c>) and falls back to <c>&lt;main&gt;</c> then <c>&lt;body&gt;</c>,
/// stripping scripts, styles, and anything marked <c>data-ktc-search-exclude</c>.
/// </summary>
public class WebScraperHtmlSanitizer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public virtual string SanitizeHtmlDocument(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return string.Empty;
        }

        var parser = new HtmlParser();
        var doc = parser.ParseDocument(htmlContent);

        var body = doc.Body;
        if (body is null)
        {
            return string.Empty;
        }

        // Index the post body when present, otherwise the main content, otherwise the whole body.
        var root = body.QuerySelector(".post-body")
            ?? body.QuerySelector("main")
            ?? body;

        foreach (var element in root.QuerySelectorAll("script, style, [data-ktc-search-exclude]").ToList())
        {
            element.Remove();
        }

        var textContent = root.TextContent ?? string.Empty;

        return WhitespaceRegex.Replace(textContent, " ").Trim();
    }
}
