using Goldfinch.Web.Features.BlogDetail;
using System.Collections.Generic;

namespace Goldfinch.Web.Components.ViewComponents.LatestBlogPosts
{
    public class LatestBlogPostsViewModel
    {
        public string Title { get; set; }

        public ICollection<BlogPostViewModel> BlogPosts { get; set; }
    }
}