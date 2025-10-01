using CMS.ContentEngine;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Goldfinch.Web.TagHelpers;

[HtmlTargetElement("img", Attributes = ATTRIBUTE_IMAGE)]
public class ImageAssetTagHelper : TagHelper
{
    public const string ATTRIBUTE_IMAGE = "gf-image-asset";
    private const int MAX_DISPLAY_SIZE = 1000;

    private static readonly Dictionary<string, int> standardVariants = new()
    {
        { "480Width", 480 },
        { "800Width", 800 },
        { "1000Width", 1000 }
    };

    [HtmlAttributeName(ATTRIBUTE_IMAGE)]
    public ContentItemAsset? Image { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Image is null)
        {
            output.SuppressOutput();
            return;
        }

        // Set the src to original image URL as fallback
        output.Attributes.SetAttribute("src", Image.Url);

        // Generate srcset and sizes for responsive images
        if (Image.Metadata.Width.HasValue)
        {
            string srcset = GenerateSrcSet(Image, Image.Metadata.Width.Value);
            if (!string.IsNullOrEmpty(srcset))
            {
                output.Attributes.SetAttribute("srcset", srcset);

                // Set sizes attribute - cap at original image width to prevent upscaling
                if (!output.Attributes.ContainsName("sizes"))
                {
                    int maxSize = System.Math.Min(MAX_DISPLAY_SIZE, Image.Metadata.Width.Value);
                    output.Attributes.SetAttribute("sizes", $"(max-width: 600px) 480px, (max-width: 1000px) 800px, {maxSize}px");
                }
            }
        }
    }

    private static string GenerateSrcSet(ContentItemAsset asset, int originalWidth)
    {
        if (asset.VariantUrls is null || asset.Metadata.Variants is null)
        {
            return string.Empty;
        }

        var srcsetParts = new List<string>();

        // Process variants in order, skipping those larger than the original
        foreach (var (variantName, maxWidth) in standardVariants.OrderBy(kvp => kvp.Value))
        {
            // Skip variants that would be larger than the original image
            if (maxWidth >= originalWidth)
            {
                continue;
            }

            // Check if this variant exists and get its metadata
            if (asset.VariantUrls.TryGetValue(variantName, out string? variantUrl) &&
                asset.Metadata.Variants.TryGetValue(variantName, out var variantMetadata))
            {
                // Use actual width from Xperience metadata, capped at maxWidth to prevent unexpected upscaling
                int actualWidth = System.Math.Min(variantMetadata.Width ?? maxWidth, maxWidth);
                srcsetParts.Add($"{variantUrl} {actualWidth}w");
            }
        }

        // Always include the original image as the largest option
        srcsetParts.Add($"{asset.Url} {originalWidth}w");

        return string.Join(", ", srcsetParts);
    }
}
