using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Goldfinch.Web.Components.ViewComponents.Header;

public class HeaderViewComponent : ViewComponent
{
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;

    public HeaderViewComponent(IWebPageDataContextRetriever webPageDataContextRetriever)
    {
        _webPageDataContextRetriever = webPageDataContextRetriever;
    }

    public IViewComponentResult Invoke()
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var page = data.WebPage;

        // TODO: Change this later... https://roadmap.kentico.com/c/234-advanced-navigation-modelling
        //var navigationItems = new List<NavigationItem>()
        //{
        //    new()
        //    {
        //        Title = "Home",
        //        IsActive = page.WebPageItemID == 1,
        //        Url = "/",
        //    },
        //    new()
        //    {
        //        Title = "Blog",
        //        IsActive = page.WebPageItemID == 2,
        //        Url = "/blog",
        //    },
        //    new()
        //    {
        //        Title = "About",
        //        IsActive = page.WebPageItemID == 3,
        //        Url = "/about",
        //    },
        //    new()
        //    {
        //        Title = "Speaking",
        //        IsActive = page.WebPageItemID == 51,
        //        Url = "/public-speaking",
        //    }
        //};

        var viewModel = new HeaderViewModel
        {
            //NavigationItems = navigationItems,
        };

        return View("~/Components/ViewComponents/Header/Header.cshtml", viewModel);
    }
}
