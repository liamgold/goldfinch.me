using Goldfinch.Core.ContentTypes;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewComponent : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;

    public HomePageViewComponent(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, HomePageTemplateProperties props)
    {
        var home = await _contentRetriever.RetrieveCurrentPage<Home>();

        return View("~/Features/Home/Components/HomePage.cshtml", home);
    }
}
