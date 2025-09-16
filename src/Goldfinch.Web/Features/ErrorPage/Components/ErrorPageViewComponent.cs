using Goldfinch.Core.ErrorPages;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.ErrorPage;

public class ErrorPageViewComponent : ViewComponent
{
    private readonly ErrorPageRepository _errorPageRepository;

    public ErrorPageViewComponent(ErrorPageRepository errorPageRepository)
    {
        _errorPageRepository = errorPageRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, ErrorPageTemplateProperties props)
    {
        var errorPage = await _errorPageRepository.GetErrorPage(page.WebPageItemID);

        return View("~/Features/ErrorPage/Components/ErrorPage.cshtml", errorPage);
    }
}
