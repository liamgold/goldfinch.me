using Goldfinch.Core.ErrorPages;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.ErrorPage;

public class ErrorPageViewComponent : ViewComponent
{
    private readonly IErrorPageService _errorPageService;

    public ErrorPageViewComponent(IErrorPageService errorPageService)
    {
        _errorPageService = errorPageService;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, ErrorPageTemplateProperties props)
    {
        var errorPage = await _errorPageService.GetErrorPageById(page.WebPageItemID);

        return View("~/Features/ErrorPage/Components/ErrorPage.cshtml", errorPage);
    }
}
