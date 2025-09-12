using Goldfinch.Core.InnerPages;
using Goldfinch.Core.SEO;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.InnerPage;

public class InnerPageViewComponent : ViewComponent
{
    private readonly WebPageMetaService _metaService;
    private readonly InnerPageRepository _innerPageRepository;

    public InnerPageViewComponent(WebPageMetaService metaService, InnerPageRepository innerPageRepository)
    {
        _metaService = metaService;
        _innerPageRepository = innerPageRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, InnerPageTemplateProperties props)
    {
        var innerPage = await _innerPageRepository.GetInnerPage(page.WebPageItemID);

        _metaService.SetMeta(new Meta(
            Title: innerPage.BaseContentTitle,
            Description: string.Empty,
            WebPageItemGUID: Guid.Empty,
            CanonicalUrl: string.Empty,
            NextUrl: string.Empty,
            PreviousUrl: string.Empty)
        );

        return View("~/Features/InnerPage/Components/InnerPage.cshtml", innerPage);
    }
}
