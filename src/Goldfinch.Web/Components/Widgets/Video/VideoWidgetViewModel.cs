namespace Goldfinch.Web.Components.Widgets.Video
{
    public class VideoWidgetViewModel
    {
        public required string VideoUrl { get; set; }

        public string PosterImage { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}