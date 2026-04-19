using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Goldfinch.Web.Components.Sections.ContentSection;

[FormCategory(Label = "General", Order = 0, Collapsible = true, IsCollapsed = false)]
public class ContentSectionProperties : ISectionProperties
{
    [TextInputComponent(Label = "Section ID", Order = 1, ExplanationText = "Optional unique identifier for this section (e.g., 'about-us'). Used for anchor links and navigation.")]
    public string SectionID { get; set; } = string.Empty;

    [TextInputComponent(Label = "Section Name", Order = 2, ExplanationText = "Used as the display name for the section (e.g. when use within a sidebar).")]
    public string SectionName { get; set; } = string.Empty;
}
