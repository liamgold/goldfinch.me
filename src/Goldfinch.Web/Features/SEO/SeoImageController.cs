using Goldfinch.Core.BlogPosts;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Goldfinch.Web.Features.SEO
{
    [Route("seo-image")]
    public class SeoImageController : Controller
    {
        private readonly BlogPostRepository _blogPostRepository;

        private readonly Color _backgroundColour = Color.ParseHex("#18181B");
        private readonly Color _titleColour = Color.ParseHex("#FDE047");
        private readonly Color _footerColour = Color.ParseHex("#FEF9C3");

        private readonly Font _titleFont = SystemFonts.CreateFont("Arial", 60, FontStyle.Bold);
        private readonly Font _footerFont = SystemFonts.CreateFont("Arial", 36, FontStyle.Regular);

        public SeoImageController(BlogPostRepository blogPostRepository)
        {
            _blogPostRepository = blogPostRepository;
        }

        [HttpGet("{WebPageItemGUID:guid}/card.jpg")]
        public async Task<IActionResult> GenerateImageAsync(Guid webPageItemGUID)
        {
            var allBlogPosts = await _blogPostRepository.GetAllBlogPosts();
            var currentPage = allBlogPosts.FirstOrDefault(x => x.SystemFields.WebPageItemGUID.Equals(webPageItemGUID));

            if (currentPage is null)
            {
                return NotFound();
            }

            using var image = new Image<Rgba32>(1200, 675, _backgroundColour);

            // Add Blog Post title
            var textOptions = new RichTextOptions(_titleFont)
            {
                WrappingLength = image.Width - (50 * 2),
                Origin = new PointF(50, (image.Height / 2) - 100),
                TextAlignment = TextAlignment.Center,
            };
            image.Mutate(ctx => ctx.DrawText(textOptions, currentPage.DocumentName, _titleColour));

            // Add Footer text
            textOptions = new RichTextOptions(_footerFont)
            {
                WrappingLength = image.Width - (50 * 2),
                Origin = new PointF(50, image.Height - 75),
                TextAlignment = TextAlignment.End,
            };
            image.Mutate(ctx => ctx.DrawText(textOptions, "Liam Goldfinch", _footerColour));

            // Add Border
            var pen = Pens.Solid(_titleColour, 25);
            image.Mutate(ctx => ctx.Draw(pen, new RectangularPolygon(0, 0, image.Width, image.Height).AsClosedPath()));

            using var memoryStream = new MemoryStream();

            image.SaveAsJpeg(memoryStream);
            var imageBytes = memoryStream.ToArray();

            return File(imageBytes, "image/jpeg");
        }
    }
}