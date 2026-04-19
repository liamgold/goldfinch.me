using System.Collections.Generic;

namespace Goldfinch.Web.Components.ViewComponents.Header
{
    public class HeaderViewModel
    {
        public IReadOnlyList<NavigationItem> NavigationItems { get; init; } = [];
    }

    public class NavigationItem
    {
        public required string Label { get; init; }

        /// <summary>File-style label shown as the tab text (e.g. "blog.tsx").</summary>
        public required string FileLabel { get; init; }

        public required string Url { get; init; }

        public required string Icon { get; init; }

        public bool IsActive { get; init; }
    }
}
