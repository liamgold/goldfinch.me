using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class SpeakingPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task SpeakingPage_LoadsSuccessfully()
    {
        // Arrange & Act
        var response = await Page!.GotoAsync($"{BaseUrl}/public-speaking");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected 200 status, got {response.Status}");
    }

    [Fact]
    public async Task SpeakingPage_DisplaysPageTitle()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/public-speaking");

        // Assert
        var heading = Page.GetByRole(AriaRole.Heading, new() { Name = "Public Speaking" });
        await Expect(heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SpeakingPage_DisplaysSpeakingEngagements()
    {
        // Arrange & Act
        await Page!.GotoAsync($"{BaseUrl}/public-speaking");

        // Assert
        var articles = Page.Locator("article");
        var count = await articles.CountAsync();
        Assert.True(count > 0, "Expected at least one speaking engagement to be displayed");
    }
}
