using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NexusExport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string saveToDirectory = "C:\\packages";

            SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

            string sourceValue = "https://something/repository/nuget-hosted/";
            PackageSource source = new PackageSource(sourceValue);
            source.Credentials = new PackageSourceCredential(sourceValue, "username", "password", true, null);

            SourceRepository repository = Repository.Factory.GetCoreV2(source);

            IEnumerable<IPackageSearchMetadata> packageSearchMetadatas = await SearchAsync(searchFilter, repository);

            await DownloadAsync(saveToDirectory, repository, packageSearchMetadatas);
        }

        private static async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(SearchFilter searchFilter, SourceRepository repository)
        {
            PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>();
            IEnumerable<IPackageSearchMetadata> packageSearchMetadatas = await resource.SearchAsync(string.Empty, searchFilter, 0, 1000, Logger.None, CancellationToken.None);
            return packageSearchMetadatas;
        }

        private static async Task DownloadAsync(string saveToDirectory, SourceRepository repository, IEnumerable<IPackageSearchMetadata> packageSearchMetadatas)
        {
            var downloadContext = new PackageDownloadContext(new SourceCacheContext());
            DownloadResource downloadResource = await repository.GetResourceAsync<DownloadResource>();

            foreach (var packageSearchMetadata in packageSearchMetadatas)
            {
                var versions = await packageSearchMetadata.GetVersionsAsync();
                foreach (var version in versions)
                {
                    var identity = version.PackageSearchMetadata.Identity;
                    DownloadResourceResult downloadResult = await downloadResource.GetDownloadResourceResultAsync(identity, downloadContext, saveToDirectory, Logger.None, CancellationToken.None);
                }
            }
        }
    }
}
