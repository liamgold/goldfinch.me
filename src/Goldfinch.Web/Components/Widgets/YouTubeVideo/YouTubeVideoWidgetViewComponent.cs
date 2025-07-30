using Goldfinch.Web.Components.Widgets.YouTubeVideo;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

[assembly: RegisterWidget(YouTubeVideoWidgetViewComponent.IDENTIFIER, typeof(YouTubeVideoWidgetViewComponent), "YouTube Video", typeof(YouTubeVideoWidgetProperties), Description = "Displays a YouTube video.", IconClass = "icon-rectangle-paragraph")]
namespace Goldfinch.Web.Components.Widgets.YouTubeVideo
{
    public class YouTubeVideoWidgetViewComponent : ViewComponent
    {
        public const string IDENTIFIER = "Goldfinch.YouTubeVideoWidget";

        private readonly string ViewName = "~/Components/Widgets/YouTubeVideo/YouTubeVideoWidget.cshtml";

        public YouTubeVideoWidgetViewComponent()
        {
        }

        public ViewViewComponentResult Invoke(YouTubeVideoWidgetProperties properties)
        {
            var viewModel = new YouTubeVideoWidgetViewModel
            {
                VideoId = properties.VideoId,
            };

            return View(ViewName, viewModel);
        }
    }
}