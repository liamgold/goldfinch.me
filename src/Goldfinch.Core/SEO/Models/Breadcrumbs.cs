namespace Goldfinch.Core.SEO.Models
{
    public class Breadcrumb
    {
        public required string Name { get; set; }

        public required string Url { get; set; }

        public int Position { get; set; }
    }
}
