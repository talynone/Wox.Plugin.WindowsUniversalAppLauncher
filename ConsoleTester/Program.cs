using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ConsoleTester
{
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

            foreach (var app in storeAppList)
            {                 
                Console.WriteLine("Display name: " + app.DisplayName);
                Console.WriteLine("Full Name: " + app.FullName);
                Console.WriteLine("Full Logo Path: " + app.FullLogoPath);
                Console.WriteLine("");
            }

            var package = storeAppList.FirstOrDefault(p => p.DisplayName.ToLower().Contains("hearts"));

            if (package != null)
            {
               //var result = StoreAppLauncher.StoreAppLauncher.LaunchApp(package);                
            }
                
            Console.WriteLine();
            Console.ReadLine();            
        }
    }
}
