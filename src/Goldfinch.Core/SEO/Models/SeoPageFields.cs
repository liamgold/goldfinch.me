using Goldfinch.Core.ContentTypes;

namespace Goldfinch.Core.SEO.Models;

public class SeoPageFields : ISeoFields, IBaseContentFields
{
    /// <summary>
    /// SeoTitle.
    /// </summary>
    public required string SeoTitle { get; set; }


    /// <summary>
    /// SeoShortDescription.
    /// </summary>
    public required string SeoShortDescription { get; set; }


    /// <summary>
    /// SeoCanonicalUrl.
    /// </summary>
    public required string SeoCanonicalUrl { get; set; }


    /// <summary>
    /// BaseContentTitle.
    /// </summary>
    public required string BaseContentTitle { get; set; }


    /// <summary>
    /// BaseContentShortDescription.
    /// </summary>
    public required string BaseContentShortDescription { get; set; }


    public string MetaTitle => string.IsNullOrWhiteSpace(SeoTitle) ? BaseContentTitle : SeoTitle;

    public string MetaDescription => string.IsNullOrWhiteSpace(SeoShortDescription) ? BaseContentShortDescription : SeoShortDescription;
}
