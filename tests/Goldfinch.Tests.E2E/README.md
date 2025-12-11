# Goldfinch.me E2E Tests

End-to-end tests for the Goldfinch.me website using Playwright and xUnit.

## Prerequisites

- .NET 10.0 SDK
- Playwright browsers (installed automatically on first run)

## Running Tests Locally

### 1. Start the application

```bash
cd src/Goldfinch.Web
dotnet run
```

The application will start at `https://localhost:52623`

### 2. Run the tests (in a separate terminal)

```bash
# Run all tests
dotnet test tests/Goldfinch.Tests.E2E/Goldfinch.Tests.E2E.csproj

# Run only smoke tests
dotnet test tests/Goldfinch.Tests.E2E/Goldfinch.Tests.E2E.csproj --filter "Category=Smoke"
```

### 3. First-time setup

On first run, you'll need to install Playwright browsers:

```bash
cd tests/Goldfinch.Tests.E2E
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

## Test Configuration

Tests use the `BASE_URL` environment variable to determine which site to test:

- **Default**: `https://localhost:52623` (local development)
- **Custom**: Set `BASE_URL` environment variable

```bash
# Test against production
$env:BASE_URL="https://www.goldfinch.me"
dotnet test tests/Goldfinch.Tests.E2E/Goldfinch.Tests.E2E.csproj
```

## Test Structure

```
tests/Goldfinch.Tests.E2E/
├── Pages/                      # Page-specific tests
│   ├── HomePageTests.cs
│   ├── BlogListPageTests.cs
│   ├── BlogDetailPageTests.cs
│   ├── SpeakingPageTests.cs
│   └── AboutPageTests.cs
├── PlaywrightTestBase.cs       # Base class for all tests
└── README.md
```

## CI/CD Integration

Tests are automatically run in the GitHub Actions workflow before deployment to production. See `.github/workflows/e2e-tests.yml` for the full workflow configuration.

The workflow:
1. Builds the application
2. Starts the application in the background
3. Runs smoke tests against `localhost:52623`
4. Stops the application
5. Deploys to production (only if tests pass)

## Writing New Tests

1. Create a new test class inheriting from `PlaywrightTestBase`
2. Add the `[Trait("Category", "Smoke")]` attribute for smoke tests
3. Use the `Page` property to interact with the browser
4. Use Playwright assertions: `await Expect(element).ToBeVisibleAsync()`

Example:

```csharp
using Microsoft.Playwright;

namespace Goldfinch.Tests.E2E.Pages;

[Trait("Category", "Smoke")]
public class NewPageTests : PlaywrightTestBase
{
    [Fact]
    public async Task NewPage_LoadsSuccessfully()
    {
        var response = await Page!.GotoAsync($"{BaseUrl}/new-page");
        Assert.NotNull(response);
        Assert.True(response.Ok);
    }
}
```

## Troubleshooting

### Tests fail with "browser not found"

Install Playwright browsers:
```bash
pwsh tests/Goldfinch.Tests.E2E/bin/Debug/net10.0/playwright.ps1 install
```

### Tests fail with SSL certificate errors

The tests are configured to ignore HTTPS errors for local development (`IgnoreHTTPSErrors: true`).

### Application not responding

Make sure the application is running on `https://localhost:52623` before running tests.
