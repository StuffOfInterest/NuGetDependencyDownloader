using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuGetDependencyDownloader
{
    // Good examples
    // http://blog.nuget.org/20130520/Play-with-packages.html

    public class Utility
    {
        public static IList<IPackage> Packages { get; set; } = new List<IPackage>();

        public static IQueryable<IPackage> GetPackages(string packageId, bool includePreRelease)
        {
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
            IQueryable<IPackage> packages = repo.FindPackagesById(packageId).AsQueryable();

            if (!includePreRelease)
            {
                packages = packages.Where(item => (item.IsReleaseVersion() == true));
            }

            return packages;
        }

        public static IPackage GetLatestPackage(string packageId, bool includePrerelease)
        {
            IQueryable<IPackage> packages = Utility.GetPackages(packageId, includePrerelease);

            if (!includePrerelease)
            {
                packages = packages
                    .Where(item => item.IsReleaseVersion() == true)
                    .Where(o => o.IsLatestVersion);
            }

            var latest = packages.OrderByDescending(o => o.Version).FirstOrDefault();

            return latest;
        }

        public static IPackage GetRangedPackageVersion(IQueryable<IPackage> packages, IVersionSpec versionSpec)
        {
            if (versionSpec.MinVersion != null)
            {
                if (versionSpec.IsMinInclusive)
                {
                    packages = packages.Where(o => o.Version >= versionSpec.MinVersion);
                }
                else
                {
                    packages = packages.Where(o => o.Version > versionSpec.MinVersion);
                }
            }

            if (versionSpec.MaxVersion != null)
            {
                if (versionSpec.IsMaxInclusive)
                {
                    packages = packages.Where(o => o.Version <= versionSpec.MaxVersion);
                }
                else
                {
                    packages = packages.Where(o => o.Version < versionSpec.MaxVersion);
                }
            }

            return packages
                .OrderByDescending(o => o.Version)
                .FirstOrDefault();
        }

        public static bool IsPackageKnown(IPackage package)
        {
            return Packages.Any(o => o.Title == package.Title && o.Version == package.Version);
        }
    }
}
