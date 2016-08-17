using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreAppLauncher.Helpers
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Xml.Linq;

    using Windows.UI.Xaml.Media.Imaging;


    using global::StoreAppLauncher.Models;

    public static class NativeApiManifestHelpers
    {
        public const int STGM_SHARE_DENY_NONE = 0x40;

        private static ManifestInfo GetAppInfoFromManifest(string manifestFilePath)
        {
            if (File.Exists(manifestFilePath))
            {
                var factory = (NativeApiHelper.IAppxFactory)new NativeApiHelper.AppxFactory();

                IStream strm;

                NativeApiHelper.SHCreateStreamOnFileEx(
                    manifestFilePath,
                    STGM_SHARE_DENY_NONE,
                    0,
                    false,
                    IntPtr.Zero,
                    out strm);

                if (strm != null)
                {
                    var manifestInfo = new ManifestInfo();

                    var reader = factory.CreateManifestReader(strm);
                    var properties = reader.GetProperties();

                    //                    object o = properties;
                    //                    var propertyNames = o.GetType().GetProperties().Select(p => p.Name).ToList();
                    //
                    //
                    //                    // Make sure it implements IDispatch.
                    //                    var supportDispatch = DispatchUtility.ImplementsIDispatch(reader.GetProperties());
                    //                    
                    //                    

                    manifestInfo.Properties = properties;

                    var apps = reader.GetApplications();

                    while (apps.GetHasCurrent())
                    {
                        var app = apps.GetCurrent();
                        var manifestApplication = new ManifestApplication(app);

                        manifestApplication.AppListEntry = NativeApiHelper.GetStringValue(app, "AppListEntry");
                        manifestApplication.Description = NativeApiHelper.GetStringValue(app, "Description");
                        manifestApplication.DisplayName = NativeApiHelper.GetStringValue(app, "DisplayName");
                        manifestApplication.EntryPoint = NativeApiHelper.GetStringValue(app, "EntryPoint");
                        manifestApplication.Executable = NativeApiHelper.GetStringValue(app, "Executable");
                        manifestApplication.Id = NativeApiHelper.GetStringValue(app, "Id");
                        manifestApplication.Logo = NativeApiHelper.GetStringValue(app, "Logo");
                        manifestApplication.SmallLogo = NativeApiHelper.GetStringValue(app, "SmallLogo");
                        manifestApplication.StartPage = NativeApiHelper.GetStringValue(app, "StartPage");
                        manifestApplication.Square150x150Logo = NativeApiHelper.GetStringValue(app, "Square150x150Logo");
                        manifestApplication.Square30x30Logo = NativeApiHelper.GetStringValue(app, "Square30x30Logo");
                        manifestApplication.BackgroundColor = NativeApiHelper.GetStringValue(app, "BackgroundColor");
                        manifestApplication.ForegroundText = NativeApiHelper.GetStringValue(app, "ForegroundText");
                        manifestApplication.WideLogo = NativeApiHelper.GetStringValue(app, "WideLogo");
                        manifestApplication.Wide310x310Logo = NativeApiHelper.GetStringValue(app, "Wide310x310Logo");
                        manifestApplication.ShortName = NativeApiHelper.GetStringValue(app, "ShortName");
                        manifestApplication.Square310x310Logo = NativeApiHelper.GetStringValue(app, "Square310x310Logo");
                        manifestApplication.Square70x70Logo = NativeApiHelper.GetStringValue(app, "Square70x70Logo");
                        manifestApplication.MinWidth = NativeApiHelper.GetStringValue(app, "MinWidth");
                        manifestInfo.Apps.Add(manifestApplication);
                        apps.MoveNext();
                    }
                    Marshal.ReleaseComObject(strm);

                    return manifestInfo;
                }
                else
                {
                    Debug.WriteLine("Call to SHCreateStreamOnFileEx failed on : " + manifestFilePath);
                }
            }
            else
            {
                Debug.WriteLine("Manifest File Missing: " + manifestFilePath);
            }

            return null;
        }


        public enum FindLogoScaleStrategy
        {
            Highest = 0,

            NeareastToCustomScale = 1,
        }


        public static string FindLogoImagePath(
            string path,
            string resourceName,
            FindLogoScaleStrategy findLogoScaleStrategy,
            int scaleValue = 100)
        {
            var isValidFindStrategy = Enum.IsDefined(typeof(FindLogoScaleStrategy), findLogoScaleStrategy);

            if (!isValidFindStrategy)
            {
                throw new ArgumentException("Invalid find logo strategy." + findLogoScaleStrategy);
            }

            if (string.IsNullOrWhiteSpace(resourceName)) return null;

            //if (path.ToLower().Contains("project"))

            if (path.ToLower().Contains("blockedin"))
            {
                var blah = "connected";
            }

            var logoSizes = new List<int>();

            const string fileNameScaleToken = ".scale-";

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resourceName);
            string fileExtension = Path.GetExtension(resourceName);

            var folderPath = Path.Combine(path, Path.GetDirectoryName(resourceName));

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var files = Directory.EnumerateFiles(
                System.IO.Path.Combine(path, folderPath),
                fileNameWithoutExtension + fileNameScaleToken + "*" + fileExtension);

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                int pos = fileName.ToLower().IndexOf(fileNameScaleToken) + fileNameScaleToken.Length;
                string sizeText = fileName.Substring(pos);
                int size;
                if (int.TryParse(sizeText, out size))
                {
                    logoSizes.Add(size);
                }
            }

            if (logoSizes.Count > 0)
            {
                int desiredScale = scaleValue;

                if (findLogoScaleStrategy == FindLogoScaleStrategy.Highest)
                {
                    desiredScale = logoSizes.Max();
                }
                else if (findLogoScaleStrategy == FindLogoScaleStrategy.NeareastToCustomScale)
                {
                    desiredScale =
                        logoSizes.Aggregate((x, y) => Math.Abs(x - scaleValue) < Math.Abs(y - scaleValue) ? x : y);
                }

                var sizedPath = Path.Combine(
                    path,
                    Path.GetDirectoryName(resourceName),
                    fileNameWithoutExtension + fileNameScaleToken + desiredScale + fileExtension);

                if (File.Exists(sizedPath))
                {
                    return sizedPath;
                }
            }

            const string folderNameScaleToken = "scale-";

            var folders = Directory.EnumerateDirectories(folderPath).Where(p => p.Contains("scale-")).ToList();

            if (folders.Any())
            {
                var folderSizes = new List<int>();

                foreach (var folder in folders)
                {
                    int pos = folder.ToLower().IndexOf(folderNameScaleToken) + folderNameScaleToken.Length;
                    string sizeText = folder.Substring(pos);
                    int size;

                    if (int.TryParse(sizeText, out size))
                    {
                        folderSizes.Add(size);
                    }
                }

                if (folderSizes.Count > 0)
                {
                    int desiredScale = scaleValue;

                    if (findLogoScaleStrategy == FindLogoScaleStrategy.Highest)
                    {
                        desiredScale = folderSizes.Max();
                    }
                    else if (findLogoScaleStrategy == FindLogoScaleStrategy.NeareastToCustomScale)
                    {
                        desiredScale =
                            folderSizes.Aggregate((x, y) => Math.Abs(x - scaleValue) < Math.Abs(y - scaleValue) ? x : y);
                    }

                    var sizedFolderPath = Path.Combine(
                        folderPath,
                        folderNameScaleToken + desiredScale,
                        fileNameWithoutExtension + fileExtension);

                    if (File.Exists(sizedFolderPath))
                    {
                        return sizedFolderPath;
                    }
                }
            }


            var finalPath = Path.Combine(path, resourceName);

            if (File.Exists(finalPath))
            {
                return finalPath;
            }

            return null;
        }

        public static string GetBestLogoPath(
            ManifestInfo manifestInfo,
            ManifestApplication manifestApplication,
            string installedLocationPath)
        {
            var findStrategy = FindLogoScaleStrategy.NeareastToCustomScale;
            string logoPath;


            // Try to find logo based on package logo value
            var mainPackageLogo = manifestInfo.GetPropertyStringValue("Logo");
            logoPath = FindLogoImagePath(installedLocationPath, mainPackageLogo, findStrategy);
            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                return logoPath;
            }

            // TODO: More testing if this is even necessary
            // Try to find logo based on application logo value
            logoPath = FindLogoImagePath(installedLocationPath, manifestApplication.Logo, findStrategy);
            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                return logoPath;
            }

            return "";
        }

        public static string GetModernAppLogo(string installedLocationPath)
        {
            // get folder where actual app resides
            //var exePath = installedLocationPath;
            var dir = installedLocationPath;
            var manifestPath = System.IO.Path.Combine(dir, "AppxManifest.xml");

            if (File.Exists(manifestPath))
            {
                // this is manifest file
                string pathToLogo;
                using (var fs = File.OpenRead(manifestPath))
                {
                    var manifest = XDocument.Load(fs);

                    //TODO fix for Windows 8
                    const string ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                    // rude parsing - take more care here
                    pathToLogo = manifest.Root.Element(XName.Get("Properties", ns)).Element(XName.Get("Logo", ns)).Value;
                }
                // now here it is tricky again - there are several files that match logo, for example
                // black, white, contrast white. Here we choose first, but you might do differently
                string finalLogo = null;
                // serach for all files that match file name in Logo element but with any suffix (like "Logo.black.png, Logo.white.png etc)
                foreach (
                    var logoFile in
                    Directory.GetFiles(
                        System.IO.Path.Combine(dir, System.IO.Path.GetDirectoryName(pathToLogo)),
                        System.IO.Path.GetFileNameWithoutExtension(pathToLogo) + "*" +
                        System.IO.Path.GetExtension(pathToLogo)))
                {
                    finalLogo = logoFile;
                    break;
                }

                return finalLogo;

                //                if (System.IO.File.Exists(finalLogo))
                //                {
                //                    using (var fs = File.OpenRead(finalLogo))
                //                    {
                //                        var img = new BitmapImage()
                //                        {
                //                        };
                //                        img.BeginInit();
                //                        img.StreamSource = fs;
                //                        img.CacheOption = BitmapCacheOption.OnLoad;
                //                        img.EndInit();
                //                        return img;
                //                    }
                //                }
            }
            return null;
        }

        public static IEnumerable<PackageInfoEx> ToPackageInfoEx(
            IEnumerable<Windows.ApplicationModel.Package> packages,
            bool processLogo = true)
        {
            //var packageTest = packages.FirstOrDefault(p => p.Id.Name.ToLower().Contains("reader"));

            //            if (packageTest != null)
            //            {
            //                var debugOut = "Blah!";
            //            }

            foreach (var package in packages)
            {
                if (package.Id.Name.ToLower().Contains("reader"))
                {
                    var debugOut = "Blah!";
                }

                // We don't care about framework
                // packages, these packages are libraries
                // not apps
                if (package.IsFramework)
                {
                    continue;
                }

                string installedLocationPath = null;

                try
                {
                    installedLocationPath = package.InstalledLocation.Path;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Look up of install location failed. " + package.Id.Name);
                    continue;                    
                }
              
                var manifestPath = Path.Combine(installedLocationPath, "AppxManifest.xml");
                var manifestInfo = GetAppInfoFromManifest(manifestPath);
                
                if (manifestInfo.Apps != null)
                {
                    foreach (var application in manifestInfo.Apps)
                    {
                        // Configured to not display on start menu
                        // so we skip theses
                        if (application?.AppListEntry == "none")
                        {
                            continue;                            
                        }

                        var packageInfoEx = new PackageInfoEx();
                        
                        var fullName = package.Id.FullName;

//                        if (installedLocationPath.ToLower().Contains("windowscommunicationsapps"))
//                        {
//                            var blah = "connected";
//                        }

                        var displayName = NativeApiHelper.LoadResourceString(fullName, application.DisplayName);

                        // Can't get display name, probably not 
                        // an app we care about
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            Debug.WriteLine(manifestPath);
                            continue;
                        }
                        
                        packageInfoEx.DisplayName = displayName;

                        var description = NativeApiHelper.LoadResourceString(fullName, application.Description);

                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            packageInfoEx.Description = description;
                        }
                      
                        var logoPath = GetBestLogoPath(manifestInfo, application, installedLocationPath);
                        packageInfoEx.FullLogoPath = logoPath;

//                        package.Description = package.GetPropertyStringValue("Description");
//                        package.DisplayName = package.GetPropertyStringValue("DisplayName");
//                        package.Logo = package.GetPropertyStringValue("Logo");
//                        package.PublisherDisplayName = package.GetPropertyStringValue("PublisherDisplayName");
//                        package.IsFramework = package.GetPropertyBoolValue("Framework");

                        packageInfoEx.FullName = fullName;
                        
                        yield return packageInfoEx;
                    }
                }
                else
                {
                    Debug.WriteLine("Manifest has no apps defined: " + manifestPath);
                }
            }
        }
    }
}
