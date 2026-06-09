using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class BlogDetailPageTests : PlaywrightTestBase
{
    private async Task<string?> GetFirstBlogPostUrl()
    {
        await Page!.GotoAsync($"{BaseUrl}/blog");
        // Post cards are <a class="post-card"> elements directly — there is no wrapping <article>
        var firstArticleLink = Page.Locator("a.post-card").First;
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

    [Fact]
    public async Task BlogDetailPage_DisplaysTagsRow()
    {
        // Arrange - All current posts are tagged, so the newest post has a tags row
        var blogPostUrl = await GetFirstBlogPostUrl();
        Assert.NotNull(blogPostUrl);

        // Act
        await Page!.GotoAsync($"{BaseUrl}{blogPostUrl}");

        // Assert - Tags row with at least one chip linking to the filtered archive
        var tagsRow = Page.Locator(".post-tags-row");
        await Expect(tagsRow).ToBeVisibleAsync();

        var firstChip = tagsRow.Locator("a.tag-chip").First;
        var href = await firstChip.GetAttributeAsync("href");
        Assert.NotNull(href);
        Assert.Contains("/blog?tag=", href);
    }

    [Fact]
    public async Task BlogDetailPage_TagChipNavigatesToFilteredList()
    {
        // Arrange
        var blogPostUrl = await GetFirstBlogPostUrl();
        Assert.NotNull(blogPostUrl);
        await Page!.GotoAsync($"{BaseUrl}{blogPostUrl}");

        // Act - Click the first tag chip on the post
        var firstChip = Page.Locator(".post-tags-row a.tag-chip").First;
        await firstChip.ClickAsync();
        await Page.WaitForURLAsync(url => url.Contains("/blog?tag="));

        // Assert - Lands on the archive with that tag active and matching posts shown
        var activeChip = Page.Locator(".tag-chip-row .tag-chip[aria-current='true']");
        await Expect(activeChip).ToBeVisibleAsync();

        var posts = await Page.Locator("a.post-card").CountAsync();
        Assert.True(posts > 0, "Expected the filtered archive to show at least the post we came from");
    }
}
