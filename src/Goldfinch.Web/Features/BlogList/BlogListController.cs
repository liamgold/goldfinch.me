using CMS.Websites;
using CMS.Websites.Routing;
using Goldfinch.Core.BlogListings;
using Goldfinch.Core.BlogPosts;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.SEO.Constants;
using Goldfinch.Web.Features.BlogDetail;
using Goldfinch.Web.Features.BlogList;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Schema.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWebPageRoute(
    contentTypeName: BlogListing.CONTENT_TYPE_NAME,
    controllerType: typeof(BlogListController),
    ActionName = nameof(BlogListController.Index)
)]
namespace Goldfinch.Web.Features.BlogList
{
    public class BlogListController : Controller
    {
        private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
        private readonly IWebPageUrlRetriever _webPageUrlRetriever;
        private readonly BlogListingRepository _blogListingRepository;
        private readonly BlogPostRepository _blogPostRepository;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
        private readonly IWebsiteChannelContext _websiteChannelContext;

        public BlogListController(
            IWebPageDataContextInitializer webPageDataContextInitializer,
            IWebPageUrlRetriever webPageUrlRetriever,
            BlogListingRepository blogListingRepository,
            BlogPostRepository blogPostRepository,
            IPreferredLanguageRetriever preferredLanguageRetriever,
            IWebsiteChannelContext websiteChannelContext)
        {
            _webPageDataContextInitializer = webPageDataContextInitializer;
            _webPageUrlRetriever = webPageUrlRetriever;
            _blogListingRepository = blogListingRepository;
            _blogPostRepository = blogPostRepository;
            _preferredLanguageRetriever = preferredLanguageRetriever;
            _websiteChannelContext = websiteChannelContext;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            var pageCount = await _blogPostRepository.GetBlogPageCount();

            if (pageIndex > pageCount || pageIndex <= 0)
            {
                return NotFound();
            }

            var blogListing = await _blogListingRepository.GetBlogListing();

            if (blogListing == null)
            {
                return NotFound();
            }

            var languageName = _preferredLanguageRetriever.Get();

            _webPageDataContextInitializer.Initialize(new RoutedWebPage
            {
                WebPageItemGUID = blogListing.SystemFields.WebPageItemGUID,
                WebPageItemID = blogListing.SystemFields.WebPageItemID,
                ContentTypeName = BlogListing.CONTENT_TYPE_NAME,
                LanguageName = languageName,
                WebsiteChannelID = blogListing.SystemFields.WebPageItemWebsiteChannelId,
                WebsiteChannelName = _websiteChannelContext.WebsiteChannelName,
                ContentTypeID = blogListing.SystemFields.ContentItemContentTypeID,
            });

            var viewModel = await BlogListViewModel.GetViewModelAsync(blogListing, _webPageUrlRetriever, pageIndex, pageCount);

            if (viewModel.PageIndex != 1)
            {
                viewModel.PreviousUrl = viewModel.PageIndex - 1 == 1 ? $"{viewModel.Url}" : $"{viewModel.Url}/{viewModel.PageIndex - 1}";
            }

            viewModel.NextUrl = viewModel.PageIndex == viewModel.PageCount ? string.Empty : $"{viewModel.Url}/{viewModel.PageIndex + 1}";

            if (!string.IsNullOrWhiteSpace(viewModel.PreviousUrl))
            {
                viewModel.PreviousUrl = $"https://www.goldfinch.me{Url.Content(viewModel.PreviousUrl)}";
            }

            if (!string.IsNullOrWhiteSpace(viewModel.NextUrl))
            {
                viewModel.NextUrl = $"https://www.goldfinch.me{Url.Content(viewModel.NextUrl)}";
            }

            ViewData[SEOConstants.NEXT_URL_KEY] = viewModel.NextUrl;
            ViewData[SEOConstants.PREVIOUS_URL_KEY] = viewModel.PreviousUrl;

            var blogPosts = await _blogPostRepository.GetBlogPosts(pageIndex);

            foreach (var blogPost in blogPosts)
            {
                var vm = await BlogPostViewModel.GetViewModelAsync(blogPost, _webPageUrlRetriever);
                viewModel.BlogPosts.Add(vm);
            }

            viewModel.Schema = GetSchema(viewModel);

            return View("~/Features/BlogList/Listing.cshtml", viewModel);
        }

        private string GetSchema(BlogListViewModel viewModel)
        {
            var blogPosts = new List<BlogPosting>();

            foreach (var blogPost in viewModel.BlogPosts)
            {
                blogPosts.Add(new BlogPosting
                {
                    Headline = blogPost.Title,
                    Url = new Uri($"https://www.goldfinch.me{Url.Content(blogPost.Url)}"),
                    DatePublished = blogPost.BlogPostDate,
                    Description = blogPost.Summary,
                });
            }

            var itemListElements = blogPosts.Select(post => new ListItem
            {
                Item = post
            }).ToList<IListItem>();

            var mainEntity = new ItemList
            {
                ItemListElement = itemListElements
            };

            var currentUrl = new Uri($"https://www.goldfinch.me{Url.Content(viewModel.Url)}");

            if (viewModel.PageIndex > 1)
            {
                currentUrl = new Uri($"{currentUrl}/{viewModel.PageIndex}");
            }

            var webPage = new WebPage
            {
                Id = currentUrl,
                Url = currentUrl,
                Name = $"Liam Goldfinch's Blog - Page {viewModel.PageIndex}",
                IsPartOf = new Blog
                {
                    Id = new Uri("https://www.goldfinch.me/blog"),
                },
                MainEntity = mainEntity
            };

            if (!string.IsNullOrWhiteSpace(viewModel.PreviousUrl) || !string.IsNullOrWhiteSpace(viewModel.NextUrl))
            {
                var hasPart = new List<ICreativeWork>();

                if (!string.IsNullOrWhiteSpace(viewModel.PreviousUrl))
                {
                    hasPart.Add(new WebPage
                    {
                        Url = new Uri(viewModel.PreviousUrl),
                        Name = "Previous page"
                    });
                }

                if (!string.IsNullOrWhiteSpace(viewModel.NextUrl))
                {
                    hasPart.Add(new WebPage
                    {
                        Url = new Uri(viewModel.NextUrl),
                        Name = "Next page"
                    });
                }

                if (hasPart.Count > 0)
                {
                    webPage.HasPart = hasPart;
                }
            }

            var jsonLd = webPage.ToHtmlEscapedString();

            return jsonLd;
        }
    }
}