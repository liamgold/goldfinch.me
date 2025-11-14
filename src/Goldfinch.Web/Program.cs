using Goldfinch.Core;
using Goldfinch.Core.ContentTypes;
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
    });

    features.UseWebPageRouting();
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

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

builder.Services.AddXperienceCommunityCspManagement();

builder.Services.Configure<ContentSecurityPolicyOptions>(builder.Configuration.GetSection("ContentSecurityPolicy"));

if (env.IsDevelopment())
{
    builder.Services.AddKenticoMiniProfiler(options =>
    {
        options.PopupRenderPosition = RenderPosition.BottomLeft;
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

// Only enables the management API in the local development environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddKenticoManagementApi(options =>
    {
        // Sets the secret required to authenticate all requests to the management API endpoints
        // Must be at least 32 characters
        options.Secret = "ze4oDiR6wFef36h5HLOIwnf3RjsrmoZs";
    });
}

var app = builder.Build();

app.InitKentico();

app.UseStaticFiles();
// app.MapStaticAssets();

app.UseKentico();

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMiniProfiler();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseCookiePolicy();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

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

app.MapControllerRoute(
    name: "blog",
    pattern: "blog/{pageIndex:int}",
    defaults: new { controller = "BlogList", action = "Index" }
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