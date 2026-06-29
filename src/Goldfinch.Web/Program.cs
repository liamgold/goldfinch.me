using Goldfinch.Core;
using Goldfinch.Core.ContentTypes;
using Goldfinch.Core.Search;
using Goldfinch.Web.Components.Sections.ContentSection;
using Goldfinch.Web.Infrastructure.StaticFiles;
using Goldfinch.Web.Middleware;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Kentico.Xperience.Admin.Base;
#if DEBUG
using Kentico.Xperience.ManagementApi;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sidio.Sitemap.AspNetCore;
using Sidio.Sitemap.Core.Services;
using StackExchange.Profiling;
using XperienceCommunity.CSP;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var config = builder.Configuration;

config.AddJsonFile("connectionstrings.json", optional: true);

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

// Lucene search — registers the blog indexing strategy used by the BlogPosts index.
// Index storage maps to Azure Blob in production via LuceneStorageModule; local filesystem otherwise.
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

#if DEBUG
    // The Management API is a local-development-only tool and must never ship to production
    // (per Kentico's own guidance). The Kentico.Xperience.ManagementApi package is referenced
    // only in Debug builds (see Goldfinch.Web.csproj), so this call, the using directive, and the
    // UseKenticoManagementApi() middleware below are all compiled out of Release builds. This keeps
    // the assembly — and its auto-initialising ManagementApiModule — out of the production
    // deployment entirely, which also avoids the 31.6.0-preview startup crash where the module
    // resolves IOpenApiReferenceRegister even when the API was never enabled.
    builder.Services.AddKenticoManagementApi(options =>
    {
        options.Secret = "HelloThisIsMySuperSuperSecretApiKey";
    });
#endif
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

app.UseStaticFiles();
// app.MapStaticAssets();

app.UseAuthentication();

#if DEBUG
if (builder.Environment.IsDevelopment())
{
    app.UseKenticoManagementApi();
}
#endif

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