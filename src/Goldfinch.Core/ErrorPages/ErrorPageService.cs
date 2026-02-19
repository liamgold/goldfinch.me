using CMS.ContentEngine;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Kentico.Content.Web.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Core.ErrorPages;

public class ErrorPageService : IErrorPageService
{
    private readonly IContentRetriever _contentRetriever;

    public ErrorPageService(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }

    public async Task<ErrorPage?> GetErrorPageByCode(int errorCode)
    {
        var errorPages = await _contentRetriever.RetrievePages<ErrorPage>(
            RetrievePagesParameters.Default,
            query => query.Where(x => x.WhereEquals(nameof(ErrorPage.ErrorCode), errorCode)),
            cacheSettings: new RetrievalCacheSettings(
                cacheItemNameSuffix: $"{nameof(ErrorPage)}|code|{errorCode}",
                cacheExpiration: TimeSpan.FromMinutes(30)));

        return errorPages.FirstOrDefault();
    }

    public async Task<ErrorPage?> GetErrorPageById(int webPageItemID)
    {
        var errorPages = await _contentRetriever.RetrievePages<ErrorPage>(
            RetrievePagesParameters.Default,
            query => query.Where(x => x.WhereEquals(nameof(WebPageFields.WebPageItemID), webPageItemID)),
            cacheSettings: new RetrievalCacheSettings(
                cacheItemNameSuffix: $"{nameof(ErrorPage)}|id|{webPageItemID}",
                cacheExpiration: TimeSpan.FromMinutes(30)));

        return errorPages.FirstOrDefault();
    }
}
