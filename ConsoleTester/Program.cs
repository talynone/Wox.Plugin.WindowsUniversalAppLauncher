using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ConsoleTester
{
    using System.Drawing.Imaging;
    using System.IO;

    using StoreAppLauncher.Models;

    class Program
    {                                                 
        static void Main(string[] args)
        {
            List<PackageInfoEx> storeAppList = null;

            try
            {
                storeAppList = StoreAppLauncher.StoreAppLauncher.GetAppListForCurrentUser().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                return;
            }            

            var blankIcons = storeAppList.Where(p => string.IsNullOrWhiteSpace(p.FullLogoPath)).ToList();
            var blankDisplayNames = storeAppList.Where(p => string.IsNullOrWhiteSpace(p.FullLogoPath)).ToList();
            var blankDisplayDescriptions = storeAppList.Where(p => string.IsNullOrWhiteSpace(p.FullLogoPath)).ToList();
            
            //var package = packagesList.FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Contains("TD5"));

            var duplicatePackageNames = storeAppList.GroupBy(p => p.FullName).Where(p => p.Count() > 1);
            var displayNames = storeAppList.Select(p => p.DisplayName).OrderBy(p=>p).ToList();

            foreach (var app in storeAppList)
            {                 
                Console.WriteLine("Display name: " + app.DisplayName);
                Console.WriteLine("Full Name: " + app.FullName);
                Console.WriteLine("Full Logo Path: " + app.FullLogoPath);
                Console.WriteLine("");
                                
                //CreatedCachedIcon(app);
            }

            var package = storeAppList.FirstOrDefault(p => p.DisplayName.ToLower().Contains("mail"));

            
            //var package = storeAppList.FirstOrDefault(p => p.FullName.ToLower().Contains("miracast"));

            var noAppId = storeAppList.Where(p => p.AppInfo.Id != "App").ToList();

            var noAppId2 = storeAppList.Where(p => string.IsNullOrEmpty(p.AppInfo.Id)).ToList();

            if (package != null)
            {
                //CreatedCachedIcon(package);
                var result = StoreAppLauncher.StoreAppLauncher.LaunchApp(package);                
            }
                
            Console.WriteLine();
            Console.ReadLine();            
        }

        
        public static void CreatedCachedIcon(PackageInfoEx packageInfo)
        {
            var cachedFolderName = Path.Combine(Directory.GetCurrentDirectory(), "cached_icons");

            if (!Directory.Exists(cachedFolderName))
            {
                Directory.CreateDirectory(cachedFolderName);
            }

            var image = StoreAppLauncher.ImageHelper.CreateIcon(packageInfo.FullLogoPath, packageInfo.AppInfo.BackgroundColor, 32, 32);

            if (image == null)
            {
                
            }
            else
            {
                var savedFileName = Path.Combine(cachedFolderName, packageInfo.FullName + ".png");
                image.Save(savedFileName, ImageFormat.Png);
            }

        }
    }
}
