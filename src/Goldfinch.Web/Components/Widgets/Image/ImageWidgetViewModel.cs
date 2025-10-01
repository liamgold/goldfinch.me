using CMS.ContentEngine;

namespace Goldfinch.Web.Components.Widgets.Image
{
    public class ImageWidgetViewModel
    {
        public ContentItemAsset? ContentItemAsset { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}