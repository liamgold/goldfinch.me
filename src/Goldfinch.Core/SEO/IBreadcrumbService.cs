using Goldfinch.Core.SEO.Models;
using Kentico.Content.Web.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goldfinch.Core.SEO;

/// <summary>
/// Builds breadcrumb navigation data for a given web page.
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Retrieves the ordered breadcrumb trail for the specified web page.
    /// </summary>
    /// <param name="routedWebPage">The current routed web page.</param>
    /// <returns>An ordered list of <see cref="Breadcrumb"/> items from root to the current page.</returns>
    Task<List<Breadcrumb>> GetBreadcrumbs(RoutedWebPage routedWebPage);
}
