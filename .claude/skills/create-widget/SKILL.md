---
name: create-widget
description: Goldfinch.me conventions for Page Builder widgets. Use when creating a new widget OR modifying an existing one under src/Goldfinch.Web/Components/Widgets/ (view component, properties, view model, Razor view, dropdown enums). These project rules override any generic Xperience widget guidance where they differ.
---

# Goldfinch widget conventions

Copy the closest existing widget in `src/Goldfinch.Web/Components/Widgets/` as a starting point, then apply the rules below. These rules take precedence over generic Xperience by Kentico widget guidance.

## Files

Each widget lives in `Components/Widgets/{Name}/`:

```
{Name}WidgetViewComponent.cs   # registration + data wiring
{Name}WidgetProperties.cs      # admin form fields (IWidgetProperties)
{Name}WidgetViewModel.cs       # data passed to the view
{Name}Widget.cshtml            # Razor view
{Name}WidgetEnums.cs           # only if the widget has dropdowns
```

## View component

- `[assembly: RegisterWidget(...)]` goes above the namespace declaration.
- Declare `public const string IDENTIFIER = "Goldfinch.{Name}";` and `public const string DISPLAY_NAME = "...";`, and use both in `RegisterWidget`. Never hardcode either string.
- `IconClass` must be a `KenticoIcons` constant (`Goldfinch.Web.Components.Shared`), never a raw icon string.
- Guard missing or empty required properties, and show a warning only in edit mode:

  ```csharp
  if (properties == null || string.IsNullOrWhiteSpace(properties.Heading))
  {
      return _pageBuilderDataContext.IsEditMode()
          ? WidgetPlaceholder.GetWarning(DISPLAY_NAME, "Add a heading in the widget properties.")
          : Content(string.Empty);
  }
  ```

  `WidgetPlaceholder` is in `Goldfinch.Web.Components.Widgets.Base`. `IsEditMode()` is an extension on `IPageBuilderDataContextRetriever` in `Goldfinch.Web.Extensions`.
- Widgets that select content items need a second guard after retrieval, with a hint such as "The selected asset could not be retrieved. Ensure it is published."
- Fetch selected content hub items with `IContentRetriever.RetrieveContentByGuids` directly in the view component. Don't add a service for this.

## Properties

- Always put at least one `[FormCategory]` on the class, even for a single group. Always set `Collapsible = true, IsCollapsed = false`. Space category `Order` values by 10 (0, 10, 20). Number field `Order` values sequentially within each group.
- Dropdowns use `DataProviderType = typeof(DropdownEnumOptionProvider<TEnum>)`. Never use the `Options = "value;Label\r\n..."` string form.
- The stored dropdown value is the kebab-case of the enum member name (`DarkBlue` → `"dark-blue"`). Defaults must use that kebab-case string. Use `[Description("Label")]` on members to set the display label.
- Exception: when stored values must match an external identifier (for example highlight.js language classes), name members as single words (`Csharp` → `"csharp"`, not `CSharp` → `"c-sharp"`).

## View

- Follow the root `AGENTS.md` front-end rules. Render content images with the `gf-image-asset` tag helper.
