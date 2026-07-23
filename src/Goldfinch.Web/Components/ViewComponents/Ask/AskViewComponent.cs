using Goldfinch.Core.Ask;
using Microsoft.AspNetCore.Mvc;

namespace Goldfinch.Web.Components.ViewComponents.Ask;

/// <summary>
/// Renders the site-wide "Ask" modal, but only when the feature is configured (an Azure OpenAI
/// deployment is set). Otherwise it renders nothing, so the feature simply isn't present rather
/// than showing a broken control.
/// </summary>
public class AskViewComponent : ViewComponent
{
    private const string ViewName = "~/Components/ViewComponents/Ask/Ask.cshtml";

    private readonly IAskChatClient _chatClient;

    public AskViewComponent(IAskChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public IViewComponentResult Invoke()
    {
        if (!_chatClient.IsConfigured)
        {
            return Content(string.Empty);
        }

        return View(ViewName);
    }
}
