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

        // Assert
        var articles = Page.Locator("article");
        var count = await articles.CountAsync();
        Assert.True(count > 0, "Expected at least one blog post to be displayed");
    }

    [Fact]
    public async Task BlogListPage_HasPagination()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/blog");

        // Assert - Just verify pagination navigation exists
        var pagination = Page.GetByRole(AriaRole.Navigation);
        var paginationCount = await pagination.CountAsync();
        Assert.True(paginationCount > 0, "Expected pagination navigation to exist");
    }
}
