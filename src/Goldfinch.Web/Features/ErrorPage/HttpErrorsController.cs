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
        private readonly ErrorPageRepository _errorPageRepository;
        private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
        private readonly IWebsiteChannelContext _websiteChannelContext;

        public HttpErrorsController(ErrorPageRepository errorPageRepository, IWebPageDataContextInitializer webPageDataContextInitializer, IPreferredLanguageRetriever preferredLanguageRetriever, IWebsiteChannelContext websiteChannelContext)
        {
            _errorPageRepository = errorPageRepository;
            _webPageDataContextInitializer = webPageDataContextInitializer;
            _preferredLanguageRetriever = preferredLanguageRetriever;
            _websiteChannelContext = websiteChannelContext;
        }

        public async Task<IActionResult> ErrorAsync(int code)
        {
            Core.ContentTypes.ErrorPage errorPage;

            switch (code)
            {
                case 404:
                    errorPage = await _errorPageRepository.Get404Page();
                    break;

                case 500:
                    errorPage = await _errorPageRepository.Get500Page();
                    break;

                default:
                    return StatusCode(code);
            }

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
