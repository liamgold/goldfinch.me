using Goldfinch.Core.PublicSpeaking;
using Goldfinch.Core.SEO;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.PublicSpeakingPage;

public class PublicSpeakingPageViewComponent : ViewComponent
{
    private readonly IPublicSpeakingService _publicSpeakingService;

    public PublicSpeakingPageViewComponent(IPublicSpeakingService publicSpeakingService)
    {
        _publicSpeakingService = publicSpeakingService;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, PublicSpeakingPageTemplateProperties props)
    {
        var publicSpeakingModel = await _publicSpeakingService.GetPublicSpeakingPage(page.WebPageItemID);

        if (publicSpeakingModel == null || publicSpeakingModel.Page == null)
        {
            return Content(string.Empty);
        }

        return View("~/Features/PublicSpeakingPage/Components/PublicSpeakingPage.cshtml", publicSpeakingModel);
    }
}
