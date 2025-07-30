using CMS.Websites;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Features.BlogDetail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.BlogList
{
    public class BlogListViewModel
    {
        public string Title { get; private set; }

        public string Url { get; private set; }

        public string PreviousUrl { get; set; }

        public string NextUrl { get; set; }

        public int PageIndex { get; private set; }

        public int PageCount { get; private set; }

        public ICollection<BlogPostViewModel> BlogPosts { get; set; } = [];

        public string Schema { get; set; }

        public static async Task<BlogListViewModel> GetViewModelAsync(BlogListing blogListing, IWebPageUrlRetriever pageUrlRetriever, int pageIndex, int pageCount)
        {
            return new BlogListViewModel
            {
                Title = blogListing.DocumentName,
                Url = (await pageUrlRetriever.Retrieve(blogListing)).RelativePath,
                PageIndex = pageIndex,
                PageCount = pageCount,
            };
        }
    }
}
