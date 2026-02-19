using Goldfinch.Core.MediaAssets;
using Goldfinch.Web.Components.Shared;
using Goldfinch.Web.Components.Widgets.Base;
using Goldfinch.Web.Components.Widgets.Video;
using Goldfinch.Web.Extensions;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWidget(
    identifier: VideoWidgetViewComponent.IDENTIFIER,
    viewComponentType: typeof(VideoWidgetViewComponent),
    name: VideoWidgetViewComponent.DISPLAY_NAME,
    propertiesType: typeof(VideoWidgetProperties),
    Description = "Displays a video.",
    IconClass = KenticoIcons.MEDIA_PLAYER)]

namespace Goldfinch.Web.Components.Widgets.Video;

public class VideoWidgetViewComponent : ViewComponent
{
    public const string IDENTIFIER = "Goldfinch.VideoWidget";
    public const string DISPLAY_NAME = "Video";
    private const string ViewName = "~/Components/Widgets/Video/VideoWidget.cshtml";

    private readonly IMediaAssetService _mediaAssetService;
    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContext;

    public VideoWidgetViewComponent(
        IMediaAssetService mediaAssetService,
        IPageBuilderDataContextRetriever pageBuilderDataContext)
    {
        _mediaAssetService = mediaAssetService;
        _pageBuilderDataContext = pageBuilderDataContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(VideoWidgetProperties properties)
    {
        if (properties == null || !properties.SelectedAssets.Any())
        {
            return _pageBuilderDataContext.IsEditMode()
                ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Select a video asset in the widget properties.")
                : Content(string.Empty);
        }

        var asset = properties.SelectedAssets.FirstOrDefault();

        if (asset != null)
        {
            var mediaFile = await _mediaAssetService.GetMediaAssetContent(asset.Identifier);
            if (mediaFile != null)
            {
                var viewModel = new VideoWidgetViewModel
                {
                    VideoUrl = mediaFile.MediaAssetContentAsset.Url,
                    Description = mediaFile.MediaAssetContentShortDescription,
                    PosterImage = mediaFile.MediaAssetContentPosterImage?.Url ?? string.Empty,
                };

                return View(ViewName, viewModel);
            }
        }

        return _pageBuilderDataContext.IsEditMode()
            ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "The selected asset could not be retrieved. Ensure it is published.")
            : Content(string.Empty);
    }
}
