using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class BlogListPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task BlogListPage_LoadsSuccessfully()
    {
        // Arrange & Act
        var response = await Page!.GotoAsync($"{BaseUrl}/blog");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected 200 status, got {response.Status}");
    }

    [Fact]
    public async Task BlogListPage_DisplaysHeading()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/blog");

        // Assert - Just verify h1 exists, don't check specific text
        var heading = Page.Locator("h1");
        await Expect(heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BlogListPage_DisplaysBlogPosts()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/blog");

        // Assert - Post cards are <a class="post-card"> elements, not <article> elements
        var articles = Page.Locator("a.post-card");
        var count = await articles.CountAsync();
        Assert.True(count > 0, "Expected at least one blog post to be displayed");
    }

    [Fact]
    public async Task BlogListPage_HasPagination()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/blog");

        // Assert - The header nav always exists, so target the blog pagination specifically
        var pagination = Page.Locator("nav.blog-pagination");
        await Expect(pagination).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BlogListPage_DisplaysTagChipRow()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/blog");

        // Assert - "All posts" chip plus at least one real tag chip
        var chips = Page.Locator(".tag-chip-row .tag-chip");
        var count = await chips.CountAsync();
        Assert.True(count >= 2, $"Expected the All posts chip plus at least one tag chip, got {count}");
    }

    [Fact]
    public async Task BlogListPage_TagChipFiltersPosts()
    {
        // Arrange
        await Page!.GotoAsync($"{BaseUrl}/blog");
        var unfilteredCount = await Page.Locator("a.post-card").CountAsync();

        // Act - Click the first tag chip after "All posts"
        var tagChip = Page.Locator(".tag-chip-row .tag-chip").Nth(1);
        await tagChip.ClickAsync();
        await Page.WaitForURLAsync(url => url.Contains("?tag="));

        // Assert - The chip is active and only matching posts are shown
        var activeChip = Page.Locator(".tag-chip-row .tag-chip[aria-current='true']");
        await Expect(activeChip).ToBeVisibleAsync();

        var filteredCount = await Page.Locator("a.post-card").CountAsync();
        Assert.True(filteredCount > 0, "Expected at least one post for the selected tag");
        Assert.True(filteredCount <= unfilteredCount, "Filtered count should not exceed unfiltered count");
    }

    [Fact]
    public async Task BlogListPage_UnknownTagShowsEmptyState()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/blog?tag=nonexistent-tag");

        // Assert - Empty state renders, no posts and no error
        var emptyState = Page.Locator(".empty-state");
        await Expect(emptyState).ToBeVisibleAsync();

        var posts = await Page.Locator("a.post-card").CountAsync();
        Assert.Equal(0, posts);
    }

    [Fact]
    public async Task BlogListPage_TagChipPreservesSearchTerm()
    {
        // Arrange - Active search, then switch tag
        await Page!.GotoAsync($"{BaseUrl}/blog?q=kentico");

        // Act
        var tagChip = Page.Locator(".tag-chip-row .tag-chip").Nth(1);
        await tagChip.ClickAsync();
        await Page.WaitForURLAsync(url => url.Contains("tag="));

        // Assert - The search term survives the tag switch
        Assert.Contains("q=kentico", Page.Url);
    }
}
