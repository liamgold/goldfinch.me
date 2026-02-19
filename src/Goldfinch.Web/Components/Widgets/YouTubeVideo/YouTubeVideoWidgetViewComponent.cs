using Goldfinch.Web.Components.Shared;
using Goldfinch.Web.Components.Widgets.Base;
using Goldfinch.Web.Components.Widgets.YouTubeVideo;
using Goldfinch.Web.Extensions;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWidget(
    identifier: YouTubeVideoWidgetViewComponent.IDENTIFIER,
    viewComponentType: typeof(YouTubeVideoWidgetViewComponent),
    name: YouTubeVideoWidgetViewComponent.DISPLAY_NAME,
    propertiesType: typeof(YouTubeVideoWidgetProperties),
    Description = "Displays a YouTube video.",
    IconClass = KenticoIcons.MEDIA_PLAYER)]

namespace Goldfinch.Web.Components.Widgets.YouTubeVideo;

public class YouTubeVideoWidgetViewComponent : ViewComponent
{
    public const string IDENTIFIER = "Goldfinch.YouTubeVideoWidget";
    public const string DISPLAY_NAME = "YouTube Video";
    private const string ViewName = "~/Components/Widgets/YouTubeVideo/YouTubeVideoWidget.cshtml";

    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContext;

    public YouTubeVideoWidgetViewComponent(IPageBuilderDataContextRetriever pageBuilderDataContext)
    {
        _pageBuilderDataContext = pageBuilderDataContext;
    }

    public IViewComponentResult Invoke(YouTubeVideoWidgetProperties properties)
    {
        if (properties == null || string.IsNullOrWhiteSpace(properties.VideoId))
        {
            return _pageBuilderDataContext.IsEditMode()
                ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Enter a YouTube Video ID in the widget properties.")
                : Content(string.Empty);
        }

        var viewModel = new YouTubeVideoWidgetViewModel
        {
            VideoId = properties.VideoId,
        };

        return View(ViewName, viewModel);
    }
}
