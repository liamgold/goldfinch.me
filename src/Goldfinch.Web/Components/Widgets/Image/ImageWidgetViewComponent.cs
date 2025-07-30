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
            var viewModel = new ImageWidgetViewModel
            {
                ImageSet = null,
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

                        var imageSet = new ImageWidgetImageSet
                        {
                            FullWidthUrl = url,
                            Width480Url = $"{url}&maxsidesize=480&format=webp",
                            Width800Url = $"{url}&maxsidesize=800&format=webp",
                            Width1000Url = $"{url}&maxsidesize=1000&format=webp",
                        };

                        viewModel = new ImageWidgetViewModel
                        {
                            ImageSet = imageSet,
                            Description = mediaFile.MediaAssetContentShortDescription,
                        };

                        return View(ViewName, viewModel);
                    }
                }
            }

            return View(ViewName, viewModel);
        }
    }
}