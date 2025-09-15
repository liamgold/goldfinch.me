namespace Goldfinch.Core.SEO;

public class WebPageMetaService
{
    private Meta meta = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public Meta GetMeta()
    {
        return meta;
    }

    public void SetMeta(Meta meta) => this.meta = meta;
}

public record Meta(string Title, string Description, string CanonicalUrl, string NextUrl, string PreviousUrl);