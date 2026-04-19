using Goldfinch.Web.Components.Sections.ContentSection;
using Goldfinch.Web.Components.Shared;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

[assembly: RegisterSection(
    identifier: ContentSectionViewComponent.IDENTIFIER,
    viewComponentType: typeof(ContentSectionViewComponent),
    name: "Content Section",
    propertiesType: typeof(ContentSectionProperties),
    Description = "A flexible container section for widgets with optional section ID for anchor links.",
    IconClass = KenticoIcons.SQUARE)]

namespace Goldfinch.Web.Components.Sections.ContentSection;

public class ContentSectionViewComponent : ViewComponent
{
    public const string IDENTIFIER = "Goldfinch.ContentSection";

    private const string ViewName = "~/Components/Sections/ContentSection/ContentSection.cshtml";

    public IViewComponentResult Invoke(ComponentViewModel<ContentSectionProperties> sectionProperties)
    {
        var props = new ContentSectionViewModel
        {
            SectionID = sectionProperties.Properties.SectionID,
            SectionName = sectionProperties.Properties.SectionName,
        };

        return View(ViewName, props);
    }
}
