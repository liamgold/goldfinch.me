namespace Goldfinch.Web.Components.Widgets.Image
{
    public class ImageWidgetViewModel
    {
        public ImageWidgetImageSet ImageSet { get; set; }

        public string Description { get; set; }
    }

    public class ImageWidgetImageSet
    {
        public string FullWidthUrl { get; set; }

        public string Width480Url { get; set; }

        public string Width800Url { get; set; }

        public string Width1000Url { get; set; }
    }
}