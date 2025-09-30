using Goldfinch.Core.MediaAssets;
using Goldfinch.Web.Components.Widgets.Video;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWidget(VideoWidgetViewComponent.IDENTIFIER, typeof(VideoWidgetViewComponent), "Video", typeof(VideoWidgetProperties), Description = "Displays a Video.", IconClass = "icon-rectangle-paragraph")]
namespace Goldfinch.Web.Components.Widgets.Video
{
    public class VideoWidgetViewComponent : ViewComponent
    {
        public const string IDENTIFIER = "Goldfinch.VideoWidget";

        private readonly string ViewName = "~/Components/Widgets/Video/VideoWidget.cshtml";

        private readonly MediaAssetRepository _mediaAssetRepository;

        public VideoWidgetViewComponent(MediaAssetRepository mediaFileRepository)
        {
            _mediaAssetRepository = mediaFileRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(VideoWidgetProperties properties)
        {
            var viewModel = new VideoWidgetViewModel
            {
                VideoUrl = string.Empty,
            };

            if (properties.SelectedAssets.Any())
            {
                var asset = properties.SelectedAssets.FirstOrDefault();

                if (asset != null)
                {
                    var mediaFile = await _mediaAssetRepository.GetMediaAssetContent(asset.Identifier);
                    if (mediaFile != null)
                    {
                        var url = mediaFile.MediaAssetContentAsset.Url;

                        viewModel = new VideoWidgetViewModel
                        {
                            VideoUrl = url,
                            Description = mediaFile.MediaAssetContentShortDescription,
                            PosterImage = mediaFile.MediaAssetContentPosterImage?.Url ?? string.Empty,
                        };

                        return View(ViewName, viewModel);
                    }
                }
            }

            return View(ViewName, viewModel);
        }
    }
}