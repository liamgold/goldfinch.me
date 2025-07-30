using Goldfinch.Core.Sitemap;
using Microsoft.AspNetCore.Mvc;
using Sidio.Sitemap.AspNetCore;
using Sidio.Sitemap.Core;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.SEO;

public class SitemapController : Controller
{
    private readonly SitemapRepository _sitemapRepository;

    public SitemapController(SitemapRepository sitemapRepository)
    {
        _sitemapRepository = sitemapRepository;
    }

    public async Task<IActionResult> IndexAsync()
    {
        var nodes = await _sitemapRepository.GetSitemap();

        return new SitemapResult(new Sitemap(nodes));
    }
}
