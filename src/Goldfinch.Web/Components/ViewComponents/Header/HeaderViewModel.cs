using System.Collections.Generic;

namespace Goldfinch.Web.Components.ViewComponents.Header
{
    public class HeaderViewModel
    {
        public ICollection<NavigationItem> NavigationItems { get; set; } = [];
    }

    public class NavigationItem
    {
        public required string Title { get; set; }

        public required string Url { get; set; }

        public bool IsActive { get; set; }
    }
}
