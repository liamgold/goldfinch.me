using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.SEO.Models;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Components.ViewComponents.PageTitle;

public class PageTitleViewComponent : ViewComponent
{
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IContentRetriever _contentRetriever;

    public PageTitleViewComponent(IWebPageDataContextRetriever webPageDataContextRetriever, IContentRetriever contentRetriever)
    {
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var page = data.WebPage;

        var seoPage = await _contentRetriever.RetrieveCurrentPage<SeoPage>();

        var pageTitle = page.ContentTypeName.Equals(Home.CONTENT_TYPE_NAME)
            ? ".NET Developer · Liam Goldfinch"
            : $"{seoPage.MetaTitle} · Liam Goldfinch";

        var viewModel = new PageTitleViewModel
        {
            Title = pageTitle,
        };

        return View("~/Components/ViewComponents/PageTitle/PageTitle.cshtml", viewModel);
    }
}
