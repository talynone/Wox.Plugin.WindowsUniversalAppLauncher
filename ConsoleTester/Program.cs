using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ConsoleTester
{     
    class Program
    {                                                 
        static void Main(string[] args)
        {
            var storeAppList = StoreAppLauncher.StoreAppLauncher.GetAppListForCurrentUser().ToList();

            var blankIcons = storeAppList.Where(p => string.IsNullOrWhiteSpace(p.FullLogoPath)).ToList();
            var blankDisplayNames = storeAppList.Where(p => string.IsNullOrWhiteSpace(p.FullLogoPath)).ToList();
            var blankDisplayDescriptions = storeAppList.Where(p => string.IsNullOrWhiteSpace(p.FullLogoPath)).ToList();

            //var package = packagesList.FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Contains("TD5"));

            var package = storeAppList.FirstOrDefault(p => p.DisplayName.ToLower().Contains("hearts"));

            if (package != null)
            {
               var result = StoreAppLauncher.StoreAppLauncher.LaunchApp(package);                
            }
                
            Console.WriteLine();
            Console.ReadLine();            
        }
    }
}
