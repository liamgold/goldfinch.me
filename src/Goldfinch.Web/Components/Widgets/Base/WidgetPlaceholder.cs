using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Goldfinch.Web.Components.Widgets.Base;

/// <summary>
/// Renders a styled warning message in Page Builder edit mode when a widget has not been configured.
/// </summary>
public static class WidgetPlaceholder
{
    public static IViewComponentResult GetWarning(string widgetName, string message) =>
        new HtmlContentViewComponentResult(new HtmlString(
            $"""
            <div style="border-left:4px solid #f9a825;border-radius:4px;overflow:hidden;font-family:sans-serif;">
                <div style="padding:10px 14px;background:#f9a825;display:flex;align-items:center;gap:8px;">
                    <span style="font-size:16px;">&#9888;</span>
                    <strong style="font-size:14px;color:#3e2600;">{widgetName}</strong>
                </div>
                <div style="padding:18px 14px;background:#fff8e1;font-size:13px;color:#5d4037;line-height:1.5;">
                    {message}
                </div>
            </div>
            """));
}
