using Goldfinch.Core;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.Search;
using Goldfinch.Web.Components.Sections.ContentSection;
using Goldfinch.Web.Infrastructure;
using Goldfinch.Web.Infrastructure.Ai;
using Goldfinch.Web.Infrastructure.StaticFiles;
using Goldfinch.Web.Infrastructure.Storage;
using Goldfinch.Web.Middleware;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.ManagementApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sidio.Sitemap.AspNetCore;
using Sidio.Sitemap.Core.Services;
using StackExchange.Profiling;
using XperienceCommunity.CSP;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

// Enable desired Kentico Xperience features
builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(new PageBuilderOptions
    {
        ContentTypeNames =
        [
            BlogListing.CONTENT_TYPE_NAME,
            BlogPost.CONTENT_TYPE_NAME,
            ErrorPage.CONTENT_TYPE_NAME,
            Home.CONTENT_TYPE_NAME,
            InnerPage.CONTENT_TYPE_NAME,
            PublicSpeakingPage.CONTENT_TYPE_NAME,
        ],
        DefaultSectionIdentifier = ContentSectionViewComponent.IDENTIFIER,
        RegisterDefaultSection = false,
    });

    features.UseWebPageRouting();
});

builder.Services.AddAuthentication();
// services.AddAuthorization();

builder.Services.AddControllersWithViews();

builder.Services.Configure<AdminLocalizationOptions>(options =>
{
    options.DefaultCultureCode = "en-GB";
    options.SupportedCultures =
    [
        new AdminCulture
        {
            CultureCode = "en-GB",
            DisplayName = "English (United Kingdom)",
        },
    ];
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
    /*
     * Must be proper case for token validation
     * https://github.com/dotnet/aspnetcore/issues/40285#issuecomment-1047694858
     */
    options.LowercaseQueryStrings = false;
});

builder.Services.AddCoreServices();

// "Ask" AI Q&A feature — binds Azure OpenAI options and registers the chat client.
builder.Services.AddAskFeature(builder.Configuration);

// Honour Cloudflare / App Service forwarded headers (real visitor IP + HTTPS scheme).
builder.Services.AddForwardedHeadersConfiguration();

// Storage path mapping — Azure Blob in production (assets + Lucene index), local filesystem otherwise.
builder.Services.AddAppServiceStorage(env);

// Lucene search — registers the blog indexing strategy used by the BlogPosts index.
builder.Services.AddKenticoLucene(luceneBuilder =>
{
    luceneBuilder.IncludeDefaultStrategy = false;
    luceneBuilder.RegisterStrategy<BlogPostSearchIndexingStrategy>(BlogSearchConstants.INDEX_NAME);
});

builder.Services.AddXperienceCommunityCspManagement();

builder.Services.Configure<ContentSecurityPolicyOptions>(builder.Configuration.GetSection("ContentSecurityPolicy"));

if (env.IsDevelopment())
{
    builder.Services.AddKenticoMiniProfiler(options =>
    {
        options.PopupRenderPosition = RenderPosition.BottomLeft;
    });

    // The Management API is a local-development-only tool (per Kentico's guidance) and must never
    // be enabled in production, so it is registered only under the IsDevelopment() guard. The
    // matching UseKenticoManagementApi() middleware below is guarded the same way.
    builder.Services.AddKenticoManagementApi(options =>
    {
        options.Secret = "HelloThisIsMySuperSuperSecretApiKey";
    });
}
else
{
    builder.Services.Configure<AdminIdentityOptions>(options =>
    {
        options.AuthenticationOptions.MultiFactorAuthenticationOptions.Enabled = true;
        options.AuthenticationOptions.MultiFactorAuthenticationOptions.ApplicationName = "Goldfinch.me";
    });
}

builder.Services
    .AddHttpContextAccessor()
    .AddDefaultSitemapServices<HttpContextBaseUrlProvider>();

builder.Services.AddTrailingSlash(builder.Configuration);

builder.Services.AddCustomStaticFilesConfiguration(builder.Configuration);

var app = builder.Build();

app.InitKentico();

// Must run before anything that reads the client IP or request scheme (rate limiter, redirects).
app.UseForwardedHeaders();

app.UseStaticFiles();
// app.MapStaticAssets();

app.UseAuthentication();

if (builder.Environment.IsDevelopment())
{
    app.UseKenticoManagementApi();
}

app.UseKentico();

app.UseAuthorization();

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMiniProfiler();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseCookiePolicy();

app.UseCors();

app.UseTrailingSlashMiddleware();

app.UseXperienceCommunityCspManagement();

app.UseSecurityHeadersMiddleware();

if (!env.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/error/500");
}

// Per-IP rate limiting for the public /api/ask endpoint.
app.UseAskRateLimit();

app.Kentico().MapRoutes();

app.MapControllerRoute(
    name: "sitemap",
    pattern: "sitemap.xml",
    defaults: new { controller = "Sitemap", action = "Index" }
);

app.MapControllerRoute(
    name: "rss",
    pattern: "rss",
    defaults: new { controller = "RSSFeed", action = "Index" }
);

// Canonical RSS URL — /rss.xml (matches <link rel="alternate"> and docs/design-handoff/routes.md).
app.MapControllerRoute(
    name: "rss-xml",
    pattern: "rss.xml",
    defaults: new { controller = "RSSFeed", action = "Index" }
);

// Legacy path-based pagination (/blog/2) — 301s to the canonical /blog?page=N form.
// Pagination is query-string-only; this route only exists so old links + search
// results don't 404.
app.MapControllerRoute(
    name: "blog-paged-legacy",
    pattern: "blog/{pageIndex:int}",
    defaults: new { controller = "BlogList", action = "PagedRedirect" }
);

app.MapControllerRoute(
    name: "error",
    pattern: "error/{code:int}",
    defaults: new { controller = "HttpErrors", action = "Error" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}"
);

app.Run();