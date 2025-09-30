namespace Goldfinch.Web.Components.ViewComponents.SEO
{
    public class SEOViewModel
    {
        public required string Title { get; set; }

        public required string Description { get; set; }

        public required string ContentType { get; set; }

        public string Image { get; set; } = string.Empty;

        public required string Url { get; set; }

        public string Schema { get; set; } = string.Empty;
    }
}
