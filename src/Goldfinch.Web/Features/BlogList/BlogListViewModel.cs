using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.BlogDetail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.BlogList
{
    public class BlogListViewModel
    {
        public required string Title { get; set; }

        public required string Url { get; set; }

        public string PreviousUrl { get; set; } = string.Empty;

        public string NextUrl { get; set; } = string.Empty;

        public int PageIndex { get; set; }

        public int PageCount { get; set; }

        public ICollection<BlogPostViewModel> BlogPosts { get; set; } = [];

        public string Schema { get; set; } = string.Empty;

        public static async Task<BlogListViewModel> GetViewModelAsync(BlogListing blogListing, IWebPageUrlRetriever pageUrlRetriever, int pageIndex, int pageCount)
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
}
