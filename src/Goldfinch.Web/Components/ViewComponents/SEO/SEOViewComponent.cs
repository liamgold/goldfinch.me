using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.SEO;
using Goldfinch.Core.SEO.Models;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Schema.NET;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goldfinch.Web.Components.ViewComponents.SEO;

public class SEOViewComponent : ViewComponent
{
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IContentRetriever _contentRetriever;
    private readonly BreadcrumbService _breadcrumbService;

    public SEOViewComponent(IWebPageDataContextRetriever webPageDataContextRetriever, IContentRetriever contentRetriever, BreadcrumbService breadcrumbService)
    {
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _contentRetriever = contentRetriever;
        _breadcrumbService = breadcrumbService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!_webPageDataContextRetriever.TryRetrieve(out var data))
        {
            return Content(string.Empty);
        }

        var page = data.WebPage;

        var seoPageFields = await _contentRetriever.RetrieveCurrentPage<SeoPageFields>();

        var schema = await GetSchema(page);

        var pageUrl = HttpContext.Request.GetEncodedUrl();

        if (page.ContentTypeName.Equals(BlogListing.CONTENT_TYPE_NAME))
        {
            // TODO: replace this later
            pageUrl = "https://www.goldfinch.me/blog";
        }

        var pageTitle = page.ContentTypeName.Equals(Home.CONTENT_TYPE_NAME)
            ? ".NET Developer · Liam Goldfinch"
            : $"{seoPageFields.MetaTitle} · Liam Goldfinch";

        var viewModel = new SEOViewModel
        {
            Title = pageTitle,
            Description = seoPageFields.MetaDescription,
            Url = pageUrl,
            ContentType = page.ContentTypeName,
            Schema = schema,
        };

        //if (page.ContentTypeName.Equals(BlogPost.CONTENT_TYPE_NAME))
        //{
        //    viewModel.Image = "TODO - replace with a real image";
        //}

        return View("~/Components/ViewComponents/SEO/SEO.cshtml", viewModel);
    }

    private async Task<string> GetSchema(RoutedWebPage page)
    {
        var jsonLdList = new List<string>();

        var website = new WebSite
        {
            Name = "Liam Goldfinch",
            Url = new Uri("https://www.goldfinch.me/")
        };

        jsonLdList.Add(website.ToHtmlEscapedString());

        var author = new Person
        {
            Name = "Liam Goldfinch",
            Url = new Uri("https://www.goldfinch.me/"),
            SameAs = new List<Uri>
            {
                new("https://github.com/liamgold/"),
                new("https://www.linkedin.com/in/liamgoldfinch/"),
                new("https://x.com/LiamGoldfinch")
            },
            HasOccupation = new Occupation
            {
                Name = "Principal Systems Developer",
                OccupationLocation = new City
                {
                    Name = "Leeds, UK"
                }
            },
            WorksFor = new Organization
            {
                Name = "IDHL"
            }
        };

        var blog = new Blog
        {
            Name = "Liam Goldfinch's Blog",
            Url = new Uri("https://www.goldfinch.me/blog"),
            Description = "A blog by Liam Goldfinch covering various topics related to Kentico, web development, and related technology",
            Author = author
        };

        jsonLdList.Add(blog.ToHtmlEscapedString());

        var breadcrumbs = await _breadcrumbService.GetBreadcrumbs(page);

        if (breadcrumbs.Count > 0)
        {
            var listItems = new List<IListItem>();

            foreach (var breadcrumb in breadcrumbs)
            {
                var listItem = new ListItem
                {
                    Position = breadcrumb.Position,
                    Name = breadcrumb.Name,
                    Item = new Thing
                    {
                        Id = new Uri(breadcrumb.Url),
                        Name = breadcrumb.Name,
                        Url = new Uri(breadcrumb.Url)
                    }
                };

                listItems.Add(listItem);
            }

            var breadcrumbList = new BreadcrumbList
            {
                ItemListElement = listItems,
            };

            jsonLdList.Add(breadcrumbList.ToHtmlEscapedString());
        }

        var jsonLdGraphContent = string.Join(",", jsonLdList);

        var jsonLdGraph = $@"
            {{
                ""@context"": ""https://schema.org"",
                ""@graph"": [
                    {jsonLdGraphContent}
                ]
            }}";

        return jsonLdGraph;
    }
}
