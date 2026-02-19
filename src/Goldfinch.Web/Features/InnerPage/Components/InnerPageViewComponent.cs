using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.InnerPage;

public class InnerPageViewComponent : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;

    public InnerPageViewComponent(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, InnerPageTemplateProperties props)
    {
        var innerPage = await _contentRetriever.RetrieveCurrentPage<Core.ContentTypes.InnerPage>();

        return View("~/Features/InnerPage/Components/InnerPage.cshtml", innerPage);
    }
}
