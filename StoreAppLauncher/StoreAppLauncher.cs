using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreAppLauncher
{
    using Windows.Management.Deployment;    

    using global::StoreAppLauncher.Helpers;
    using global::StoreAppLauncher.Models;

    public static class StoreAppLauncher
    {                
        public static IEnumerable<PackageInfoEx> GetAppListForCurrentUser()
        {
            var packageManager = new PackageManager();

            var packages = packageManager.FindPackagesForUser(string.Empty);

            var packagesInfos = NativeApiManifestHelpers.ToPackageInfoEx(packages);

            return packagesInfos;
        }

        public static uint LaunchApp(PackageInfoEx package)
        {
            return NativeApiHelper.LaunchApp(package.FullName);
        }
    }
}
