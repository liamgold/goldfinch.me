using Goldfinch.Core.ContentTypes;

namespace Goldfinch.Core.SEO.Models;

public class SeoPage : ISeoFields, IBaseContentFields
{
    /// <summary>
    /// SeoTitle.
    /// </summary>
    public string SeoTitle { get; set; }


    /// <summary>
    /// SeoShortDescription.
    /// </summary>
    public string SeoShortDescription { get; set; }


    /// <summary>
    /// SeoCanonicalUrl.
    /// </summary>
    public string SeoCanonicalUrl { get; set; }


    /// <summary>
    /// BaseContentTitle.
    /// </summary>
    public string BaseContentTitle { get; set; }


    /// <summary>
    /// BaseContentShortDescription.
    /// </summary>
    public string BaseContentShortDescription { get; set; }
}
