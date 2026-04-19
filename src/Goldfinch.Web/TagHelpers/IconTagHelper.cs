using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Goldfinch.Web.TagHelpers;

/// <summary>
/// Renders a small inline SVG icon from a fixed set. Usage:
/// <code>&lt;icon name="book" size="14" /&gt;</code>
/// </summary>
/// <remarks>
/// The set is deliberately small — mirror of the handoff's React icon map.
/// Add new icons here rather than inlining raw SVG in views.
/// </remarks>
[HtmlTargetElement("icon", Attributes = NameAttribute)]
public class IconTagHelper : TagHelper
{
    private const string NameAttribute = "name";

    [HtmlAttributeName(NameAttribute)]
    public string Name { get; set; } = string.Empty;

    [HtmlAttributeName("size")]
    public int Size { get; set; } = 14;

    [HtmlAttributeName("stroke")]
    public decimal Stroke { get; set; } = 1.6m;

    private static readonly Dictionary<string, string> Paths = new()
    {
        ["arrowL"]   = "<line x1=\"19\" y1=\"12\" x2=\"5\" y2=\"12\" /><polyline points=\"11 6 5 12 11 18\" />",
        ["arrowR"]   = "<line x1=\"5\" y1=\"12\" x2=\"19\" y2=\"12\" /><polyline points=\"13 6 19 12 13 18\" />",
        ["book"]     = "<path d=\"M4 5a2 2 0 0 1 2-2h12v16H6a2 2 0 0 0-2 2V5Z\" /><path d=\"M4 19h14\" />",
        ["clock"]    = "<circle cx=\"12\" cy=\"12\" r=\"9\" /><polyline points=\"12 7 12 12 15 14\" />",
        ["close"]    = "<line x1=\"5\" y1=\"5\" x2=\"19\" y2=\"19\" /><line x1=\"19\" y1=\"5\" x2=\"5\" y2=\"19\" />",
        ["ext"]      = "<path d=\"M14 4h6v6\" /><path d=\"M10 14 20 4\" /><path d=\"M20 14v5a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h5\" />",
        ["github"]   = "<path fill=\"currentColor\" stroke=\"none\" d=\"M12 2a10 10 0 0 0-3.16 19.49c.5.09.68-.22.68-.48v-1.7c-2.78.6-3.37-1.34-3.37-1.34-.45-1.16-1.11-1.47-1.11-1.47-.91-.62.07-.6.07-.6 1 .07 1.53 1.03 1.53 1.03.89 1.53 2.34 1.09 2.91.83.09-.65.35-1.09.63-1.34-2.22-.25-4.56-1.11-4.56-4.94 0-1.09.39-1.98 1.03-2.68-.1-.25-.45-1.27.1-2.64 0 0 .84-.27 2.75 1.02a9.56 9.56 0 0 1 5 0c1.91-1.29 2.75-1.02 2.75-1.02.55 1.37.2 2.39.1 2.64.64.7 1.03 1.59 1.03 2.68 0 3.84-2.34 4.69-4.57 4.94.36.31.68.92.68 1.85v2.74c0 .27.18.58.69.48A10 10 0 0 0 12 2Z\" />",
        ["grid"]     = "<rect x=\"4\" y=\"4\" width=\"7\" height=\"7\" /><rect x=\"13\" y=\"4\" width=\"7\" height=\"7\" /><rect x=\"4\" y=\"13\" width=\"7\" height=\"7\" /><rect x=\"13\" y=\"13\" width=\"7\" height=\"7\" />",
        ["house"]    = "<path d=\"M3 11 12 3l9 8\" /><path d=\"M5 10v10h14V10\" />",
        ["linkedin"] = "<path fill=\"currentColor\" stroke=\"none\" d=\"M4 4h3.8v3.8H4zM4 9.5h3.8V20H4zM10 9.5h3.6v1.5c.5-.9 1.7-1.8 3.5-1.8 3.7 0 4.4 2.4 4.4 5.5V20h-3.8v-4.5c0-1.1-.2-2.5-1.9-2.5S14 14.1 14 15.4V20h-3.8V9.5z\" />",
        ["list"]     = "<line x1=\"4\" y1=\"6\" x2=\"20\" y2=\"6\" /><line x1=\"4\" y1=\"12\" x2=\"20\" y2=\"12\" /><line x1=\"4\" y1=\"18\" x2=\"14\" y2=\"18\" />",
        ["loc"]      = "<path d=\"M12 21s-7-6.5-7-12a7 7 0 0 1 14 0c0 5.5-7 12-7 12Z\" /><circle cx=\"12\" cy=\"9\" r=\"2.5\" />",
        ["menu"]     = "<line x1=\"4\" y1=\"6\" x2=\"20\" y2=\"6\" /><line x1=\"4\" y1=\"12\" x2=\"20\" y2=\"12\" /><line x1=\"4\" y1=\"18\" x2=\"14\" y2=\"18\" />",
        ["mic"]      = "<rect x=\"9\" y=\"2\" width=\"6\" height=\"12\" rx=\"3\" /><path d=\"M5 11a7 7 0 0 0 14 0\" /><line x1=\"12\" y1=\"18\" x2=\"12\" y2=\"22\" />",
        ["pin"]      = "<path fill=\"currentColor\" stroke=\"none\" d=\"M12 2 10 9H5l5 5-2 8 4-5 4 5-2-8 5-5h-5z\" />",
        ["play"]     = "<path fill=\"currentColor\" stroke=\"none\" d=\"M8 5v14l11-7z\" />",
        ["rss"]      = "<path d=\"M4 11a9 9 0 0 1 9 9\" /><path d=\"M4 4a16 16 0 0 1 16 16\" /><circle cx=\"5\" cy=\"19\" r=\"1.4\" fill=\"currentColor\" />",
        ["search"]   = "<circle cx=\"11\" cy=\"11\" r=\"7\" /><path d=\"m20 20-3.5-3.5\" />",
        ["user"]     = "<circle cx=\"12\" cy=\"8\" r=\"4\" /><path d=\"M4 21a8 8 0 0 1 16 0\" />",
        ["x"]        = "<path fill=\"currentColor\" stroke=\"none\" d=\"M18 3h3l-7.3 8.3L22 21h-6.5l-4.7-6-5.3 6H2l7.7-8.8L2.2 3H9l4.3 5.6L18 3Z\" />",
    };

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Paths.TryGetValue(Name, out var paths))
        {
            // Unknown icon — drop the tag silently rather than render a broken SVG.
            output.SuppressOutput();
            return;
        }

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("width", Size);
        output.Attributes.SetAttribute("height", Size);
        output.Attributes.SetAttribute("viewBox", "0 0 24 24");
        output.Attributes.SetAttribute("fill", "none");
        output.Attributes.SetAttribute("stroke", "currentColor");
        output.Attributes.SetAttribute("stroke-width", Stroke.ToString(System.Globalization.CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("stroke-linecap", "round");
        output.Attributes.SetAttribute("stroke-linejoin", "round");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Content.SetHtmlContent(paths);
    }
}
