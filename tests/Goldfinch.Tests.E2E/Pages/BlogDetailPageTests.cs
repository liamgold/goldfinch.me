using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class BlogDetailPageTests : PlaywrightTestBase
{
    private async Task<string?> GetFirstBlogPostUrl()
    {
        await Page!.GotoAsync($"{BaseUrl}/blog");
        var firstArticleLink = Page.Locator("article a").First;
        return await firstArticleLink.GetAttributeAsync("href");
    }

    [Fact]
    public async Task BlogDetailPage_LoadsSuccessfully()
    {
        // Arrange - Get the first blog post URL dynamically
        var blogPostUrl = await GetFirstBlogPostUrl();
        Assert.NotNull(blogPostUrl);

        // Act
        var response = await Page!.GotoAsync($"{BaseUrl}{blogPostUrl}");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected 200 status, got {response.Status}");
    }

    [Fact]
    public async Task BlogDetailPage_DisplaysTitle()
    {
        // Arrange - Get the first blog post URL dynamically
        var blogPostUrl = await GetFirstBlogPostUrl();
        Assert.NotNull(blogPostUrl);

        // Act
        await Page!.GotoAsync($"{BaseUrl}{blogPostUrl}");

        // Assert - Just verify h1 exists, don't check specific text
        var heading = Page.Locator("h1");
        await Expect(heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BlogDetailPage_DisplaysPublishDate()
    {
        // Arrange - Get the first blog post URL dynamically
        var blogPostUrl = await GetFirstBlogPostUrl();
        Assert.NotNull(blogPostUrl);

        // Act
        await Page!.GotoAsync($"{BaseUrl}{blogPostUrl}");

        // Assert - Just verify a time element exists
        var time = Page.Locator("time").First;
        await Expect(time).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BlogDetailPage_DisplaysContent()
    {
        // Arrange - Get the first blog post URL dynamically
        var blogPostUrl = await GetFirstBlogPostUrl();
        Assert.NotNull(blogPostUrl);

        // Act
        await Page!.GotoAsync($"{BaseUrl}{blogPostUrl}");

        // Assert
        var paragraphs = Page.Locator("p");
        var count = await paragraphs.CountAsync();
        Assert.True(count > 0, "Expected blog post to have content");
    }
}
