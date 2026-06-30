using System;
using System.Threading;
using System.Threading.Tasks;
using Goldfinch.Admin.FormComponents.ReadingMinutes;
using Goldfinch.Core.BlogPosts;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(
    ReadingMinutesFormComponent.IDENTIFIER,
    typeof(ReadingMinutesFormComponent),
    "Reading time (with regenerate)")]

namespace Goldfinch.Admin.FormComponents.ReadingMinutes;

/// <summary>
/// Number input for <c>BlogPostReadingMinutes</c> with a "Regenerate" button that recomputes the
/// value from the post's current Page Builder content (see <see cref="IBlogPostReadingMinutesRegenerator"/>),
/// so editors don't have to estimate the number by hand.
/// </summary>
public class ReadingMinutesFormComponent : FormComponent<ReadingMinutesClientProperties, int>
{
    public const string IDENTIFIER = "Goldfinch.ReadingMinutesFormComponent";

    private readonly IBlogPostReadingMinutesRegenerator _regenerator;

    public ReadingMinutesFormComponent(IBlogPostReadingMinutesRegenerator regenerator)
    {
        _regenerator = regenerator;
    }

    public override string ClientComponentName => "@goldfinch/web-admin/ReadingMinutes";

    [FormComponentCommand]
    public async Task<ICommandResponse<int>> Regenerate(CancellationToken cancellationToken)
    {
        if (FormContext is not IContentItemFormContextBase context)
        {
            throw new InvalidOperationException("The reading-minutes form component can only be used in a content item form context.");
        }

        var minutes = await _regenerator.RegenerateAsync(context.ItemId, context.LanguageName, cancellationToken);

        return ResponseFrom(minutes);
    }
}
