using Goldfinch.Core.SEO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Goldfinch.Web.Components.ViewComponents.Canonical;

public class CanonicalViewComponent : ViewComponent
{
    private readonly WebPageMetaService _metaService;

    public CanonicalViewComponent(WebPageMetaService metaService)
    {
        _metaService = metaService;
    }

    public IViewComponentResult Invoke()
    {
        var meta = _metaService.GetMeta();

        var canonicalUrl = meta.CanonicalUrl;

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
