using CMS.ContentEngine;
using Goldfinch.Core.ContentTypes;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;

namespace Goldfinch.Web.Components.Widgets.Image;

[FormCategory(Label = "Content", Order = 0, Collapsible = true, IsCollapsed = false)]
public class ImageWidgetProperties : IWidgetProperties
{
    [ContentItemSelectorComponent(MediaAssetContent.CONTENT_TYPE_NAME, Label = "Asset", Order = 1, MaximumItems = 1, AllowContentItemCreation = true)]
    public IEnumerable<ContentItemReference> SelectedAssets { get; set; } = [];
}