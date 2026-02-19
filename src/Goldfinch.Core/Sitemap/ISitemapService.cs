using Sidio.Sitemap.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goldfinch.Core.Sitemap;

/// <summary>
/// Builds the XML sitemap for the site.
/// </summary>
public interface ISitemapService
{
    /// <summary>
    /// Retrieves all sitemap nodes representing published pages on the site.
    /// </summary>
    /// <returns>A list of <see cref="SitemapNode"/> entries.</returns>
    Task<List<SitemapNode>> GetSitemap();
}
