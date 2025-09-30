using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using System;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.BlogDetail;

public class BlogPostViewModel
{
    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required string Url { get; set; }

    public DateTime BlogPostDate { get; set; }

    public string Schema { get; set; } = string.Empty;

    public static async Task<BlogPostViewModel> GetViewModelAsync(BlogPost blogPost, IWebPageUrlRetriever pageUrlRetriever)
    {
        return new BlogPostViewModel
        {
            Title = blogPost.BaseContentTitle,
            Summary = blogPost.BaseContentShortDescription,
            BlogPostDate = blogPost.BlogPostDate,
            Url = (await pageUrlRetriever.Retrieve(blogPost)).RelativePath,
        };
    }
}