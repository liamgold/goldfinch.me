using Goldfinch.Core.MediaAssets;
using Goldfinch.Web.Components.Widgets.Image;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWidget(ImageWidgetViewComponent.IDENTIFIER, typeof(ImageWidgetViewComponent), "Image", typeof(ImageWidgetProperties), Description = "Displays an image.", IconClass = "icon-rectangle-paragraph")]
namespace Goldfinch.Web.Components.Widgets.Image
{
    public class ImageWidgetViewComponent : ViewComponent
    {
        public const string IDENTIFIER = "Goldfinch.ImageWidget";

        private readonly string ViewName = "~/Components/Widgets/Image/ImageWidget.cshtml";

        private readonly MediaAssetRepository _mediaAssetRepository;

        public ImageWidgetViewComponent(MediaAssetRepository mediaFileRepository)
        {
            _mediaAssetRepository = mediaFileRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(ImageWidgetProperties properties)
        {
            var viewModel = new ImageWidgetViewModel();

            if (properties.SelectedAssets.Any())
            {
                var asset = properties.SelectedAssets.FirstOrDefault();

                if (asset != null)
                {
                    var mediaFile = await _mediaAssetRepository.GetMediaAssetContent(asset.Identifier);
                    if (mediaFile != null)
                    {
                        viewModel = new ImageWidgetViewModel
                        {
                            ContentItemAsset = mediaFile.MediaAssetContentAsset,
                            Description = mediaFile.MediaAssetContentShortDescription,
                        };
                    }
                }
            }

            return View(ViewName, viewModel);
        }
    }
}