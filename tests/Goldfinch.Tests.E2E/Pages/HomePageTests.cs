using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class HomePageTests : PlaywrightTestBase
{
    [Fact]
    public async Task HomePage_LoadsSuccessfully()
    {
        // Arrange & Act
        var response = await Page!.GotoAsync(BaseUrl);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected 200 status, got {response.Status}");
    }

    [Fact]
    public async Task HomePage_HasTitle()
    {
        // Arrange & Act
        await Page!.GotoAsync(BaseUrl);

        // Assert - Just verify a title exists, don't check specific text
        var title = await Page.TitleAsync();
        Assert.False(string.IsNullOrWhiteSpace(title), "Page should have a title");
    }

    [Fact]
    public async Task HomePage_DisplaysMainHeading()
    {
        // Arrange & Act
        await Page!.GotoAsync(BaseUrl);

        // Assert - Just verify h1 exists, don't check specific text
        var heading = Page.Locator("h1");
        await Expect(heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task HomePage_HasNavigation()
    {
        // Arrange & Act
        await Page!.GotoAsync(BaseUrl);

        // Assert - Check navigation exists with expected links
        var nav = Page.GetByRole(AriaRole.Navigation);
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "Home", Exact = true })).ToBeVisibleAsync();
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "Blog", Exact = true })).ToBeVisibleAsync();
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "About", Exact = true })).ToBeVisibleAsync();
        await Expect(nav.GetByRole(AriaRole.Link, new() { Name = "Speaking", Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task HomePage_DisplaysBlogPosts()
    {
        // Arrange & Act
        await Page!.GotoAsync(BaseUrl);

        // Assert - Just check that articles exist, don't check specific content
        var articles = Page.Locator("article");
        var count = await articles.CountAsync();
        Assert.True(count > 0, "Home page should display at least one blog post");
    }

    [Fact]
    public async Task HomePage_NoConsoleErrors()
    {
        // Arrange
        var consoleErrors = new List<string>();
        Page!.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        Assert.Empty(consoleErrors);
    }
}
