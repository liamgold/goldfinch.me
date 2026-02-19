using Goldfinch.Core.Sitemap;
using Microsoft.AspNetCore.Mvc;
using Sidio.Sitemap.AspNetCore;
using Sidio.Sitemap.Core;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.SEO;

public class SitemapController : Controller
{
    private readonly ISitemapService _sitemapService;

    public SitemapController(ISitemapService sitemapService)
    {
        _sitemapService = sitemapService;
    }

    public async Task<IActionResult> IndexAsync()
    {
        var nodes = await _sitemapService.GetSitemap();

        return new SitemapResult(new Sitemap(nodes));
    }
}
