using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using Goldfinch.Web;
using Kentico.Xperience.AzureStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

[assembly: RegisterModule(typeof(StorageInitializationModule))]

namespace Goldfinch.Web;

public class StorageInitializationModule : Module
{
    private IWebHostEnvironment _environment;

    /// <summary>
    /// Gets the web hosting environment information.
    /// </summary>
    public IWebHostEnvironment Environment
    {
        get
        {
            return _environment ??= Service.Resolve<IWebHostEnvironment>();
        }
    }

    public StorageInitializationModule()
         : base(nameof(StorageInitializationModule))
    {
    }

    protected override void OnInit()
    {
        base.OnInit();

        if (!Environment.IsProduction())
        {
            return;
        }

        // Creates a new StorageProvider instance for Azure Blob storage
        var assetsProvider = AzureStorageProvider.Create();

        // Specifies the target container, the provider ensures its existence in the storage account
        assetsProvider.CustomRootPath = "assetscontainer";

        // Makes the 'assetscontainer' container publicly accessible
        assetsProvider.PublicExternalFolderObject = false;

        // Maps the local directory to the storage provider
        StorageHelper.MapStoragePath("~/assets", assetsProvider);
    }
}