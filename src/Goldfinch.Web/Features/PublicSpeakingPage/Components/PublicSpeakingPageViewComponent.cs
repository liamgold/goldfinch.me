using Goldfinch.Core.PublicSpeaking;
using Goldfinch.Core.SEO;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.PublicSpeakingPage;

public class PublicSpeakingPageViewComponent : ViewComponent
{
    private readonly PublicSpeakingRepository _publicSpeakingRepository;

    public PublicSpeakingPageViewComponent(PublicSpeakingRepository publicSpeakingRepository)
    {
        _publicSpeakingRepository = publicSpeakingRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, PublicSpeakingPageTemplateProperties props)
    {
        var publicSpeakingModel = await _publicSpeakingRepository.GetPublicSpeakingPage(page.WebPageItemID);

        if (publicSpeakingModel == null || publicSpeakingModel.Page == null)
        {
            return Content(string.Empty);
        }

        return View("~/Features/PublicSpeakingPage/Components/PublicSpeakingPage.cshtml", publicSpeakingModel);
    }
}
