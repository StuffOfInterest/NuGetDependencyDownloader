using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Protocol;
using NuGet.Versioning;
using NuGet.Packaging.Signing;

namespace NuGetDependencyDownloader
{
    public static class NuGetManager
    {
        private static readonly Logger Logger = new Logger();
        private static readonly PackageSource PackageSource = new PackageSource("https://api.nuget.org/v3/index.json");
        private static readonly SourceCacheContext SourceCacheContext = new SourceCacheContext();
        private static readonly string RootPath = $"{Directory.GetCurrentDirectory()}\\Download";
        private static readonly string PackagesPath = RootPath;

        private static readonly ISettings ThisSettings =
            Settings.LoadDefaultSettings(RootPath, null, new MachineWideSettings());

        private static readonly NuGetProject ThisProject = new FolderNuGetProject(RootPath);
        private static SourceRepository SourceRepository { get; }
        private static ISourceRepositoryProvider SourceRepositoryProvider { get; }
        private static NuGetPackageManager PackageManager { get; }
        private static SourceRepository SourceRepositories { get; }

        static NuGetManager()
        {
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            SourceRepository = new SourceRepository(PackageSource, providers);

            SourceRepositoryProvider = new SourceRepositoryProvider(new PackageSourceProvider(ThisSettings), providers);

            PackageManager =
                new NuGetPackageManager(SourceRepositoryProvider, ThisSettings, PackagesPath)
                {
                    PackagesFolderNuGetProject = (FolderNuGetProject) ThisProject
                };


            SourceRepositories = SourceRepositoryProvider.CreateRepository(PackageSource); // See part 2
        }


        public static async Task Downloader(string id, string version, bool includePrerelease,
            CancellationToken downloadCancelToken)
        {
            Console.WriteLine($@"Download directory: {Directory.GetCurrentDirectory()}\Download");

            const bool allowUnlisted = false;

            var resolutionContext = new ResolutionContext(
                DependencyBehavior.HighestMinor, includePrerelease, allowUnlisted, VersionConstraints.None);

            var packageMetadataResource =
                await SourceRepository.GetResourceAsync<PackageMetadataResource>(downloadCancelToken);

            var searchMetadata = await packageMetadataResource.GetMetadataAsync(id, includePrerelease, true,
                SourceCacheContext, Logger, downloadCancelToken);

            if (searchMetadata.Count() == 0)
            {
                Console.WriteLine("None was found");
                return;
            }

            IPackageSearchMetadata package;
            try
            {
                var nugetVersion = NuGetVersion.Parse(version);
                package = searchMetadata.First(a => a.Identity.Version == nugetVersion);
            }
            catch (ArgumentException)
            {
                package = searchMetadata.MaxBy(a => a.Identity.Version).First();
            }

            Console.WriteLine($@"Start downloading {package.Identity.Id} {package.Identity.Version}");

            var identity = new PackageIdentity(package.Identity.Id, package.Identity.Version);

            var projectContext = new EmptyNuGetProjectContext
            {
                OperationId = new Guid(),
                ActionType = NuGetActionType.Install,
                PackageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Nupkg,
                    XmlDocFileSaveMode.Skip,
                    ClientPolicyContext.GetClientPolicy(ThisSettings, Logger),
                    Logger
                )
            };

            var downloadContext = new PackageDownloadContext(SourceCacheContext, PackagesPath, true);

            await PackageManager.InstallPackageAsync(PackageManager.PackagesFolderNuGetProject,
                identity, resolutionContext, projectContext, downloadContext, SourceRepositories,
                Array.Empty<SourceRepository>(), // This is a list of secondary source respositories, probably empty
                downloadCancelToken);
            if (!downloadCancelToken.IsCancellationRequested)
            {
                Console.WriteLine($@"{package.Identity.Id} {package.Identity.Version} downloaded");
            }
        }
    }
}