using Goldfinch.Core.HomePages;
using Goldfinch.Core.SEO;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewComponent : ViewComponent
{
    private readonly WebPageMetaService _metaService;
    private readonly HomeRepository _homeRepository;

    public HomePageViewComponent(WebPageMetaService metaService, HomeRepository homeRepository)
    {
        _metaService = metaService;
        _homeRepository = homeRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, HomePageTemplateProperties props)
    {
        var home = await _homeRepository.GetHome();

        _metaService.SetMeta(new Meta(
            NextUrl: string.Empty,
            PreviousUrl: string.Empty)
        );

        return View("~/Features/Home/Components/HomePage.cshtml", home);
    }
}
