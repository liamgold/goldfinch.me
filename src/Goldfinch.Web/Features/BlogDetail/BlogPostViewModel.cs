using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using System;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.BlogDetail;

public class BlogPostViewModel
{
    public string Title { get; private set; }

    public string Summary { get; private set; }

    public string Url { get; private set; }

    public DateTime BlogPostDate { get; private set; }

    public string Schema { get; set; }

    public static async Task<BlogPostViewModel> GetViewModelAsync(BlogPost blogPost, IWebPageUrlRetriever pageUrlRetriever)
    {
        return new BlogPostViewModel
        {
            Title = blogPost.DocumentName,
            Summary = blogPost.BlogPostSummary,
            BlogPostDate = blogPost.BlogPostDate,
            Url = (await pageUrlRetriever.Retrieve(blogPost)).RelativePath,
        };
    }
}