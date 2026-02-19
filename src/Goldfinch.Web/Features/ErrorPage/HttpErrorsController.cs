using CMS.Websites.Routing;
using Goldfinch.Core.ErrorPages;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.ErrorPage
{
    public class HttpErrorsController : Controller
    {
        private readonly IErrorPageService _errorPageService;
        private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
        private readonly IWebsiteChannelContext _websiteChannelContext;

        public HttpErrorsController(
            IErrorPageService errorPageService,
            IWebPageDataContextInitializer webPageDataContextInitializer,
            IPreferredLanguageRetriever preferredLanguageRetriever,
            IWebsiteChannelContext websiteChannelContext)
        {
            _errorPageService = errorPageService;
            _webPageDataContextInitializer = webPageDataContextInitializer;
            _preferredLanguageRetriever = preferredLanguageRetriever;
            _websiteChannelContext = websiteChannelContext;
        }

        public async Task<IActionResult> ErrorAsync(int code)
        {
            if (code != 404 && code != 500)
            {
                return StatusCode(code);
            }

            var errorPage = await _errorPageService.GetErrorPageByCode(code);

            if (errorPage == null)
            {
                return StatusCode(code);
            }

            var languageName = _preferredLanguageRetriever.Get();

            _webPageDataContextInitializer.Initialize(new RoutedWebPage
            {
                WebPageItemGUID = errorPage.SystemFields.WebPageItemGUID,
                WebPageItemID = errorPage.SystemFields.WebPageItemID,
                ContentTypeName = Core.ContentTypes.ErrorPage.CONTENT_TYPE_NAME,
                LanguageName = languageName,
                WebsiteChannelID = errorPage.SystemFields.WebPageItemWebsiteChannelId,
                WebsiteChannelName = _websiteChannelContext.WebsiteChannelName,
                ContentTypeID = errorPage.SystemFields.ContentItemContentTypeID,
            });

            return new TemplateResult(errorPage);
        }
    }
}
