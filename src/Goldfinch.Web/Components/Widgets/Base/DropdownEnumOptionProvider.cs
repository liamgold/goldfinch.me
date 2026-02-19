using Kentico.Xperience.Admin.Base.FormAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Goldfinch.Web.Components.Widgets.Base;

/// <summary>
/// Generic dropdown option provider that maps an enum to Kentico admin dropdown items.
/// Enum member names are converted to kebab-case for stored values (e.g. ArrowLeft → arrow-left).
/// Use <see cref="DescriptionAttribute"/> on enum members to customise the displayed label.
/// </summary>
public class DropdownEnumOptionProvider<T> : IDropDownOptionsProvider where T : struct, Enum
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
    {
        var items = Enum.GetValues<T>().Select(enumValue =>
        {
            var memberInfo = typeof(T).GetMember(enumValue.ToString()).First();
            var description = memberInfo
                .GetCustomAttribute<DescriptionAttribute>()
                ?.Description;

            return new DropDownOptionItem
            {
                Value = ToKebabCase(enumValue.ToString()),
                Text = description ?? SplitPascalCase(enumValue.ToString()),
            };
        });

        return Task.FromResult(items.AsEnumerable());
    }

    /// <summary>Converts "ArrowLeft" to "arrow-left".</summary>
    private static string ToKebabCase(string value) =>
        string.Concat(value.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? $"-{char.ToLower(c)}" : char.ToLower(c).ToString()));

    /// <summary>Converts "ArrowLeft" to "Arrow Left" as a fallback label.</summary>
    private static string SplitPascalCase(string value) =>
        string.Concat(value.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? $" {c}" : c.ToString()));
}
