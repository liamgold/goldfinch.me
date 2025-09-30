using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Goldfinch.Tests.E2E;

/// <summary>
/// Base class for Playwright tests with common setup and configuration
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }

    /// <summary>
    /// Base URL for testing - defaults to localhost, can be overridden via environment variable
    /// </summary>
    protected string BaseUrl => Environment.GetEnvironmentVariable("BASE_URL") ?? "https://localhost:52623";

    /// <summary>
    /// Initialize Playwright browser and context
    /// </summary>
    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true // For local dev with self-signed certs
        });
        Page = await Context.NewPageAsync();
    }

    /// <summary>
    /// Cleanup Playwright resources
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Page != null)
            await Page.CloseAsync();
        if (Context != null)
            await Context.CloseAsync();
        if (_browser != null)
            await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
