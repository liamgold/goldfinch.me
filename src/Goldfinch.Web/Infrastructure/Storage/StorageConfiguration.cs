using CMS.IO;
using Kentico.Xperience.AzureStorage;
using Kentico.Xperience.Lucene.Core.Store;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Goldfinch.Web.Infrastructure.Storage;

public static class StorageConfiguration
{
    // Built once and used for both the path registration and the CreateProviderForPath
    // comparison — the strings must be identical or the container override silently won't apply.
    private static readonly string _luceneIndexPath = $"~/{LuceneStorageConstants.LUCENE_INDEX_PATH}/";

    private const string AssetsContainerName = "assetscontainer";
    private const string LuceneContainerName = "lucene";

    /// <summary>
    ///     Configures automatic storage path mapping (Xperience 31.6.0+) for the Azure App Service
    ///     deployment. Replaces the legacy StorageInitializationModule and LuceneStorageModule.
    ///     Platform paths (~/assets/*) register themselves; the Lucene index path is custom and is
    ///     registered explicitly. Container names preserve the pre-migration blobs.
    /// </summary>
    /// <param name="serviceCollection">The service collection to configure the storage mapping within.</param>
    /// <param name="environment">The web hosting environment; mapping only activates in production.</param>
    public static IServiceCollection AddAppServiceStorage(this IServiceCollection serviceCollection, IWebHostEnvironment environment)
    {
        serviceCollection.AddStoragePathRegistration(_luceneIndexPath, PathType.SharedPersistent);

        return serviceCollection.AddAppServiceStoragePathMapping(options =>
        {
            // Mapping is unconditional by default — gate on production for parity with the old
            // modules (local dev keeps the plain filesystem, including the Lucene index under App_Data).
            options.IsMappingEnabled = environment.IsProduction();
            options.ContainerName = AssetsContainerName;

            options.CreateProviderForPath = pathRegistration =>
            {
                if (pathRegistration.RegisteredPath == _luceneIndexPath)
                {
                    return AzureStorageProvider.Create(LuceneContainerName, publicExternalFolderObject: false);
                }

                // null → default provider (assetscontainer for SharedPersistent paths)
                return null;
            };
        });
    }
}
