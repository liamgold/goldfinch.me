using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.SEO;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Components.ViewComponents.PageTitle;

public class PageTitleViewComponent : ViewComponent
{
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly WebPageMetaService _metaService;

    public PageTitleViewComponent(IWebPageDataContextRetriever webPageDataContextRetriever, WebPageMetaService metaService)
    {
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _metaService = metaService;
    }

    public IViewComponentResult Invoke()
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var page = data.WebPage;

        var meta = _metaService.GetMeta();

        var pageTitle = page.ContentTypeName.Equals(Home.CONTENT_TYPE_NAME)
            ? ".NET Developer · Liam Goldfinch"
            : $"{meta.Title} · Liam Goldfinch";

        var viewModel = new PageTitleViewModel
        {
            Title = pageTitle,
        };

        return View("~/Components/ViewComponents/PageTitle/PageTitle.cshtml", viewModel);
    }
}
