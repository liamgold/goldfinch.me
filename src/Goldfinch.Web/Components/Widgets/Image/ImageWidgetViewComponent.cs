using Goldfinch.Core.ContentTypes;
using Goldfinch.Web.Components.Shared;
using Goldfinch.Web.Components.Widgets.Base;
using Goldfinch.Web.Components.Widgets.Image;
using Goldfinch.Web.Extensions;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWidget(
    identifier: ImageWidgetViewComponent.IDENTIFIER,
    viewComponentType: typeof(ImageWidgetViewComponent),
    name: ImageWidgetViewComponent.DISPLAY_NAME,
    propertiesType: typeof(ImageWidgetProperties),
    Description = "Displays an image.",
    IconClass = KenticoIcons.PICTURE)]

namespace Goldfinch.Web.Components.Widgets.Image;

public class ImageWidgetViewComponent : ViewComponent
{
    public const string IDENTIFIER = "Goldfinch.ImageWidget";
    public const string DISPLAY_NAME = "Image";
    private const string ViewName = "~/Components/Widgets/Image/ImageWidget.cshtml";

    private readonly IContentRetriever _contentRetriever;
    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContext;

    public ImageWidgetViewComponent(
        IContentRetriever contentRetriever,
        IPageBuilderDataContextRetriever pageBuilderDataContext)
    {
        _contentRetriever = contentRetriever;
        _pageBuilderDataContext = pageBuilderDataContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(ImageWidgetProperties properties)
    {
        if (properties == null || !properties.SelectedAssets.Any())
        {
            return _pageBuilderDataContext.IsEditMode()
                ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Select an image asset in the widget properties.")
                : Content(string.Empty);
        }

        var asset = properties.SelectedAssets.FirstOrDefault();

        if (asset != null)
        {
            var results = await _contentRetriever.RetrieveContentByGuids<MediaAssetContent>(
                new List<System.Guid> { asset.Identifier },
                new RetrieveContentParameters { LinkedItemsMaxLevel = 1 });

            var mediaFile = results.FirstOrDefault();
            if (mediaFile != null)
            {
                var viewModel = new ImageWidgetViewModel
                {
                    ContentItemAsset = mediaFile.MediaAssetContentAsset,
                    Description = mediaFile.MediaAssetContentShortDescription,
                };

                return View(ViewName, viewModel);
            }
        }

        return _pageBuilderDataContext.IsEditMode()
            ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "The selected asset could not be retrieved. Ensure it is published.")
            : Content(string.Empty);
    }
}
