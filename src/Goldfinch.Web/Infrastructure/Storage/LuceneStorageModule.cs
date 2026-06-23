using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using Goldfinch.Web.Infrastructure.Storage;
using Kentico.Xperience.AzureStorage;
using Kentico.Xperience.Lucene.Core.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

[assembly: RegisterModule(typeof(LuceneStorageModule))]

namespace Goldfinch.Web.Infrastructure.Storage;

/// <summary>
/// Maps the Lucene search index storage path to Azure Blob Storage in production, mirroring
/// <see cref="StorageInitializationModule"/>. In development the indexes fall back to the local
/// file system under <c>App_Data/LuceneSearch</c>. Storing the index in Blob persists it across
/// deployments and shares it across instances; the integration also auto-disables web-farm index
/// sync when it detects external storage.
/// </summary>
public class LuceneStorageModule : Module
{
    /// <summary>
    /// Container name within Azure Blob Storage for Lucene indexes.
    /// </summary>
    private const string ContainerName = "lucene";

    private IWebHostEnvironment? _environment;

    public LuceneStorageModule()
         : base(nameof(LuceneStorageModule))
    {
    }

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

    protected override void OnInit()
    {
        base.OnInit();

        if (!Environment.IsProduction())
        {
            return;
        }

        // Creates a new StorageProvider instance for Azure Blob storage
        var luceneProvider = AzureStorageProvider.Create();

        // Specifies the target container, the provider ensures its existence in the storage account
        luceneProvider.CustomRootPath = ContainerName;

        // Keeps the Lucene index container private
        luceneProvider.PublicExternalFolderObject = false;

        // Redirects all CMS.IO operations under the Lucene index path to Azure Blob storage
        StorageHelper.MapStoragePath($"~/{LuceneStorageConstants.LUCENE_INDEX_PATH}/", luceneProvider);
    }
}
