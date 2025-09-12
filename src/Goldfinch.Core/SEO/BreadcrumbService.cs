using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.SEO.Models;
using Goldfinch.Core.WebPage;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.SEO;

public class BreadcrumbService(WebPageQueryTools tools) : WebPageRepository(tools)
{
    public async Task<List<Breadcrumb>> GetBreadcrumbs(RoutedWebPage routedWebPage)
    {
        return await ProgressiveCache.LoadAsync(async cs =>
        {
            cs.CacheDependency = CacheHelper.GetCacheDependency(
            [
                $"webpageitem|byid|{routedWebPage.WebPageItemID}",
            ]);

            var breadcrumbs = await GetBreadcrumbsInternal(routedWebPage);

            return breadcrumbs;
        }, new CacheSettings(1440, nameof(BreadcrumbService), nameof(GetBreadcrumbs), routedWebPage.WebPageItemID));
    }

    private async Task<List<Breadcrumb>> GetBreadcrumbsInternal(RoutedWebPage routedWebPage)
    {
        if (routedWebPage.ContentTypeName.Equals(Home.CONTENT_TYPE_NAME) || routedWebPage.ContentTypeName.Equals(ErrorPage.CONTENT_TYPE_NAME))
        {
            return [];
        }

        // Current webpage
        var queryBuilder = new ContentItemQueryBuilder()
            .ForContentTypes(parameters =>
            {
                parameters.ForWebsite([routedWebPage.WebPageItemGUID]);
            });

        var result = await Executor.GetMappedWebPageResult<IWebPageFieldsSource>(queryBuilder);

        var webpage = result.FirstOrDefault();

        if (webpage is null)
        {
            return [];
        }

        var breadcrumbs = new List<Breadcrumb>();

        // Get ancestors of the current webpage
        var path = webpage.SystemFields.WebPageItemTreePath;
        var parentPaths = GetTreePathsOnPath(path, false, true).ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        queryBuilder = new ContentItemQueryBuilder()
            .ForContentTypes(parameters =>
            {
                parameters.ForWebsite(WebsiteChannelContext.WebsiteChannelName)
                    .WithContentTypeFields();
            })
            .Parameters(parameters =>
            {
                parameters.Where(where => where.WhereIn(nameof(IWebPageFieldsSource.SystemFields.WebPageItemTreePath), parentPaths));
            });

        var ancestorPages = await Executor.GetMappedWebPageResult<IWebPageFieldsSource>(queryBuilder);

        var position = 2;
        foreach (var ancestorPage in ancestorPages.OrderBy(x => x.SystemFields.WebPageUrlPath.Length))
        {
            var url = await UrlRetriever.Retrieve(ancestorPage);

            var documentName = string.Empty;

            if (ancestorPage is IBaseContentFields baseContent)
            {
                documentName = baseContent.BaseContentTitle;
            }

            breadcrumbs.Add(new Breadcrumb
            {
                Name = documentName,
                Url = $"https://www.goldfinch.me{url.RelativePath.TrimStart('~')}",
                Position = position++,
            });
         }

        breadcrumbs.Insert(0, new()
        {
            Name = "Home",
            Url = "https://www.goldfinch.me/",
            Position = 1,
        });

        return breadcrumbs;
    }

    /// <summary>
    /// TAKEN FROM: TreePathUtils.cs in XbyK Source.
    /// Gets list of tree paths for all nodes on given path.
    /// </summary>
    /// <param name="treePath">Node alias path</param>
    /// <param name="includeRoot">Indicates if root path should be included</param>
    /// <param name="includeCurrent">Indicates if path of current node should be included</param>
    private static List<string> GetTreePathsOnPath(string treePath, bool includeRoot, bool includeCurrent)
    {
        var paths = new List<string>();

        if (string.IsNullOrEmpty(treePath))
        {
            return paths;
        }

        if (!includeCurrent)
        {
            treePath = DataHelper.GetParentPath(treePath);
        }

        do
        {
            // Add the path
            if ((treePath != "/") || includeRoot)
            {
                paths.Add(treePath);
            }

            // If root, end building the list
            if (treePath == "/")
            {
                break;
            }

            treePath = DataHelper.GetParentPath(treePath);
        } while (true);

        return paths;
    }
}