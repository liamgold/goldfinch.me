using Goldfinch.Core.ErrorPages;
using Goldfinch.Core.SEO;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.ErrorPage;

public class ErrorPageViewComponent : ViewComponent
{
    private readonly WebPageMetaService _metaService;
    private readonly ErrorPageRepository _errorPageRepository;

    public ErrorPageViewComponent(WebPageMetaService metaService, ErrorPageRepository errorPageRepository)
    {
        _metaService = metaService;
        _errorPageRepository = errorPageRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, ErrorPageTemplateProperties props)
    {
        var errorPage = await _errorPageRepository.GetErrorPage(page.WebPageItemID);

        _metaService.SetMeta(new Meta(
            Title: errorPage.BaseContentTitle,
            Description: string.Empty,
            CanonicalUrl: string.Empty,
            NextUrl: string.Empty,
            PreviousUrl: string.Empty)
        );

        return View("~/Features/ErrorPage/Components/ErrorPage.cshtml", errorPage);
    }
}
