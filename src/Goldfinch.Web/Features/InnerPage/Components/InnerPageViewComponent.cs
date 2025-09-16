using Goldfinch.Core.InnerPages;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.InnerPage;

public class InnerPageViewComponent : ViewComponent
{
    private readonly InnerPageRepository _innerPageRepository;

    public InnerPageViewComponent(InnerPageRepository innerPageRepository)
    {
        _innerPageRepository = innerPageRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(RoutedWebPage page, InnerPageTemplateProperties props)
    {
        var innerPage = await _innerPageRepository.GetInnerPage(page.WebPageItemID);

        return View("~/Features/InnerPage/Components/InnerPage.cshtml", innerPage);
    }
}
