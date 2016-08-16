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

                NativeApiHelper.SHCreateStreamOnFileEx(manifestFilePath, STGM_SHARE_DENY_NONE, 0, false, IntPtr.Zero, out strm);

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
        

        public static string FindClosestTo100ScaleQualifiedImagePath(string path, string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName)) return null;

            //if (path.ToLower().Contains("project"))
            
            if (path.ToLower().Contains("blockedin"))
            {
                var blah = "connected";
            }

            var logoSizes = new List<int>();

            const string scaleToken = ".scale-";            

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(resourceName);
            string fileExtension = System.IO.Path.GetExtension(resourceName);

            var folderPath = System.IO.Path.Combine(path, System.IO.Path.GetDirectoryName(resourceName));

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var files = Directory.EnumerateFiles(
                System.IO.Path.Combine(path, folderPath),
                fileNameWithoutExtension + scaleToken + "*" + fileExtension);

            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                int pos = fileName.ToLower().IndexOf(scaleToken) + scaleToken.Length;
                string sizeText = fileName.Substring(pos);
                int size;
                if (int.TryParse(sizeText, out size))
                {
                    logoSizes.Add(size);
                }
            }

            if (logoSizes.Count > 0)
            {
                int closestToValue = 100;
                int closestScale = logoSizes.Aggregate((x, y) => Math.Abs(x - closestToValue) < Math.Abs(y - closestToValue) ? x : y);

                var sizedPath = System.IO.Path.Combine(
                    path,
                    System.IO.Path.GetDirectoryName(resourceName),
                    fileNameWithoutExtension + scaleToken + closestScale + fileExtension);

                if (File.Exists(sizedPath))
                {
                    return sizedPath;
                }
            }

            const string scaleFolderToken = "scale-";            

            var folders = Directory.EnumerateDirectories(folderPath).Where(p=>p.Contains("scale-")).ToList();

            if (folders.Any())
            {
                var folderSizes = new List<int>();

                foreach (var folder in folders)
                {
                    int pos = folder.ToLower().IndexOf(scaleFolderToken) + scaleFolderToken.Length;
                    string sizeText = folder.Substring(pos);
                    int size;

                    if (int.TryParse(sizeText, out size))
                    {
                        folderSizes.Add(size);
                    }
                }

                if (folderSizes.Count > 0)
                {
                    int closestToValue = 100;
                    int closestScale = folderSizes.Aggregate((x, y) => Math.Abs(x - closestToValue) < Math.Abs(y - closestToValue) ? x : y);

                    var sizedFolderPath = System.IO.Path.Combine(
                        folderPath,
                        scaleFolderToken + closestScale,
                        fileNameWithoutExtension + fileExtension);

                    if (File.Exists(sizedFolderPath))
                    {
                        return sizedFolderPath;
                    }
                }                                                
            }
            

            var finalPath = System.IO.Path.Combine(path, resourceName);

            if (File.Exists(finalPath))
            {
                return finalPath;                
            }

            return null;
        }

        //public static string GetBestLogoPath(Windows.ApplicationModel.Package package, ManifestInfo manifestInfo,string mainPackageLogoPath,string mainAppLogoPath)
        public static string GetBestLogoPath(ManifestInfo manifestInfo, ManifestApplication manifestApplication, string installedLocationPath)
        {
            var logoPath = FindClosestTo100ScaleQualifiedImagePath(installedLocationPath, manifestApplication.Logo);
            
            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                return logoPath;
                
            }
            
            var mainPackageLogo = manifestInfo.GetPropertyStringValue("Logo");
            
            logoPath = FindClosestTo100ScaleQualifiedImagePath(installedLocationPath, mainPackageLogo);

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
                foreach (var logoFile in Directory.GetFiles(System.IO.Path.Combine(dir, System.IO.Path.GetDirectoryName(pathToLogo)),
                    System.IO.Path.GetFileNameWithoutExtension(pathToLogo) + "*" + System.IO.Path.GetExtension(pathToLogo)))
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

        public static string FindHighestScaleQualifiedImagePath(string path, string resourceName)
        {
            if (resourceName == null) throw new ArgumentNullException("resourceName");

            const string scaleToken = ".scale-";
            var sizes = new List<int>();
            string name = System.IO.Path.GetFileNameWithoutExtension(resourceName);
            string ext = System.IO.Path.GetExtension(resourceName);

            var folderPath = System.IO.Path.Combine(path, System.IO.Path.GetDirectoryName(resourceName));

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var files = Directory.EnumerateFiles(
                System.IO.Path.Combine(path, folderPath),
                name + scaleToken + "*" + ext);

            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                int pos = fileName.IndexOf(scaleToken) + scaleToken.Length;
                string sizeText = fileName.Substring(pos);
                int size;
                if (int.TryParse(sizeText, out size))
                {
                    sizes.Add(size);
                }
            }
            if (sizes.Count == 0) return null;

            sizes.Sort();
            return System.IO.Path.Combine(
                path,
                System.IO.Path.GetDirectoryName(resourceName),
                name + scaleToken + sizes.Last() + ext);
        }

        

        public static IEnumerable<PackageInfoEx> ToPackageInfoEx(IEnumerable<Windows.ApplicationModel.Package> packages, bool processLogo = true)
        {

            var packageTest = packages.FirstOrDefault(p => p.Id.Name.ToLower().Contains("camera"));

            if (packageTest != null)
            {
                var debugOut = "Blah!";
            }

            foreach (var package in packages)
            {                
                // We don't care about framework
                // packages, these packages are libraries
                // not apps
                if (package.IsFramework)
                {
                    continue;
                }

                
                var installedLocationPath = package.InstalledLocation.Path;                
                var manifestPath = Path.Combine(installedLocationPath, "AppxManifest.xml");
                var manifestInfo = GetAppInfoFromManifest(manifestPath);
                
                if (manifestInfo.Apps != null)
                {
                    foreach (var application in manifestInfo.Apps)
                    {
                        // Configured not display on start menu
                        if (application?.AppListEntry == "none")
                        {
                            continue;                            
                        }

                        var packageInfoEx = new PackageInfoEx();
                        
                        var fullName = package.Id.FullName;

                        if (installedLocationPath.ToLower().Contains("windowscommunicationsapps"))
                        {
                            var blah = "connected";
                        }

                        var displayName = NativeApiHelper.LoadResourceString(fullName, application.DisplayName);

                        // Can't get display name, probably not 
                        // an app we care about
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
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
