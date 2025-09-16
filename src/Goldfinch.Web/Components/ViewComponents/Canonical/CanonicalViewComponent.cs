using Goldfinch.Core.SEO.Constants;
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

    public CanonicalViewComponent(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var seoPageFields = await _contentRetriever.RetrieveCurrentPage<SeoPageFields>();

        var canonicalUrl = seoPageFields.SeoCanonicalUrl;

        if (string.IsNullOrEmpty(canonicalUrl))
        {
            var request = HttpContext.Request;
            var uri = new Uri(request.GetEncodedUrl());

            canonicalUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
        }

        var nextUrl = ViewContext.ViewData[SEOConstants.NEXT_URL_KEY] as string ?? string.Empty;
        var previousUrl = ViewContext.ViewData[SEOConstants.PREVIOUS_URL_KEY] as string ?? string.Empty;

        var viewModel = new CanonicalViewModel
        {
            CanonicalUrl = canonicalUrl,
            NextUrl = nextUrl,
            PreviousUrl = previousUrl,
        };

        return View("~/Components/ViewComponents/Canonical/Canonical.cshtml", viewModel);
    }
}
