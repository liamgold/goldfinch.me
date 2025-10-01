namespace Goldfinch.Web.Components.Widgets.Image
{
    public class ImageWidgetViewModel
    {
        public required ImageWidgetImageSet ImageSet { get; set; }

        public string Description { get; set; } = string.Empty;
    }

    public class ImageWidgetImageSet
    {
        public required string FullWidthUrl { get; set; }

        public required string Width480Url { get; set; }

        public required string Width800Url { get; set; }

        public required string Width1000Url { get; set; }
    }
}