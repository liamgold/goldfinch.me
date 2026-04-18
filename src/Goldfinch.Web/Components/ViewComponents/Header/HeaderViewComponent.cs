using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Components.ViewComponents.Header;

public class HeaderViewComponent : ViewComponent
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderViewComponent(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IViewComponentResult Invoke()
    {
        var path = _httpContextAccessor.HttpContext?.Request.Path.Value ?? "/";

        var items = new List<NavigationItem>
        {
            new() { Label = "home",     FileLabel = "index.md",    Url = "/",                Icon = "house", IsActive = IsHome(path) },
            new() { Label = "blog",     FileLabel = "blog.md",     Url = "/blog",            Icon = "book",  IsActive = path.StartsWith("/blog",            System.StringComparison.OrdinalIgnoreCase) },
            new() { Label = "about",    FileLabel = "about.md",    Url = "/about",           Icon = "user",  IsActive = path.StartsWith("/about",           System.StringComparison.OrdinalIgnoreCase) },
            new() { Label = "speaking", FileLabel = "speaking.md", Url = "/public-speaking", Icon = "mic",   IsActive = path.StartsWith("/public-speaking", System.StringComparison.OrdinalIgnoreCase) },
        };

        return View("~/Components/ViewComponents/Header/Header.cshtml", new HeaderViewModel { NavigationItems = items });
    }

    private static bool IsHome(string path) => string.IsNullOrEmpty(path) || path == "/";
}
