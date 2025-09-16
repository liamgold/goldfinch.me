using Goldfinch.Core.HomePages;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.Home;

public class HomePageViewComponent : ViewComponent
{
    private readonly HomeRepository _homeRepository;

    public HomePageViewComponent(HomeRepository homeRepository)
    {
        _homeRepository = homeRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, HomePageTemplateProperties props)
    {
        var home = await _homeRepository.GetHome();

        return View("~/Features/Home/Components/HomePage.cshtml", home);
    }
}
