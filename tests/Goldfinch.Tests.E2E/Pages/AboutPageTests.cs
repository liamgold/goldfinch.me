using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class AboutPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task AboutPage_LoadsSuccessfully()
    {
        // Arrange & Act
        var response = await Page!.GotoAsync($"{BaseUrl}/about");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected 200 status, got {response.Status}");
    }

    [Fact]
    public async Task AboutPage_DisplaysPageTitle()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/about");

        // Assert
        var heading = Page.GetByRole(AriaRole.Heading, new() { Name = "About" });
        await Expect(heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AboutPage_DisplaysContent()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/about");

        // Assert
        var paragraphs = Page.Locator("p");
        var count = await paragraphs.CountAsync();
        Assert.True(count > 0, "Expected about page to have content");
    }
}
