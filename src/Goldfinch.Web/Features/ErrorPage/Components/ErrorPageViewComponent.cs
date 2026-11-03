using Goldfinch.Core.ErrorPages;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.ErrorPage;

public class ErrorPageViewComponent : ViewComponent
{
    // Long or junk paths (bot scans) would break the arcade screen layout.
    private const int MaxPathLength = 80;

    private readonly IErrorPageService _errorPageService;

    public ErrorPageViewComponent(IErrorPageService errorPageService)
    {
        _errorPageService = errorPageService;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, ErrorPageTemplateProperties props)
    {
        var errorPage = await _errorPageService.GetErrorPageById(page.WebPageItemID);

        if (errorPage is null)
        {
            return Content(string.Empty);
        }

        var requestedPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;

        if (requestedPath?.Length > MaxPathLength)
        {
            requestedPath = requestedPath[..MaxPathLength] + "…";
        }

        return View("~/Features/ErrorPage/Components/ErrorPage.cshtml", new ErrorPageViewModel(errorPage, requestedPath));
    }
}
