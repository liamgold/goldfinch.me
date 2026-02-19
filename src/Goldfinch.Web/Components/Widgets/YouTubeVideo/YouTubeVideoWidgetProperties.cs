using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Goldfinch.Web.Components.Widgets.YouTubeVideo
{
    [FormCategory(Label = "Content", Order = 0, Collapsible = true, IsCollapsed = false)]
    public class YouTubeVideoWidgetProperties : IWidgetProperties
    {
        [TextInputComponent(Label = "YouTube Video ID", Order = 1)]
        public string VideoId { get; set; } = string.Empty;
    }
}