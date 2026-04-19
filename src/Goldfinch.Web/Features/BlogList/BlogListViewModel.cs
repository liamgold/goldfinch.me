using System.Collections.Generic;
using System.Threading.Tasks;
using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.BlogDetail;

namespace Goldfinch.Web.Features.BlogList;

public class BlogListViewModel
{
    public required string Title { get; set; }

    public required string Url { get; set; }

    public string PreviousUrl { get; set; } = string.Empty;

    public string NextUrl { get; set; } = string.Empty;

    public int PageIndex { get; set; }

    public int PageCount { get; set; }

    public int TotalCount { get; set; }

    public int PageStart { get; set; }

    public int PageEnd { get; set; }

    public string? ActiveTag { get; set; }

    public string? Query { get; set; }

    /// <summary>grid | list.</summary>
    public string View { get; set; } = "grid";

    public ICollection<BlogPostViewModel> BlogPosts { get; set; } = [];

    public string Schema { get; set; } = string.Empty;

    public static async Task<BlogListViewModel> GetViewModelAsync(
        BlogListing blogListing,
        IWebPageUrlRetriever pageUrlRetriever,
        int pageIndex,
        int pageCount)
    {
        return new BlogListViewModel
        {
            Title = blogListing.BaseContentTitle,
            Url = (await pageUrlRetriever.Retrieve(blogListing)).RelativePath,
            PageIndex = pageIndex,
            PageCount = pageCount,
        };
    }
}
