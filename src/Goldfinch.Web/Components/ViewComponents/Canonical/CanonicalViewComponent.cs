using Goldfinch.Core.SEO;
using Goldfinch.Core.SEO.Models;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Goldfinch.Web.Components.ViewComponents.Canonical;

public class CanonicalViewComponent : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;
    private readonly WebPageMetaService _metaService;

    public CanonicalViewComponent(IContentRetriever contentRetriever, WebPageMetaService metaService)
    {
        _contentRetriever = contentRetriever;
        _metaService = metaService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var meta = _metaService.GetMeta();

        var seoPageFields = await _contentRetriever.RetrieveCurrentPage<SeoPageFields>();

        var canonicalUrl = seoPageFields.SeoCanonicalUrl;

        if (string.IsNullOrEmpty(canonicalUrl))
        {
            var request = HttpContext.Request;
            var uri = new Uri(request.GetEncodedUrl());

            canonicalUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
        }

        var viewModel = new CanonicalViewModel
        {
            CanonicalUrl = canonicalUrl,
            NextUrl = meta.NextUrl,
            PreviousUrl = meta.PreviousUrl,
        };

        return View("~/Components/ViewComponents/Canonical/Canonical.cshtml", viewModel);
    }
}
