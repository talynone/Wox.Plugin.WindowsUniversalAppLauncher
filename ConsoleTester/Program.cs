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

            //var package = packagesList.FirstOrDefault(p => p.DisplayName != null && p.DisplayName.Contains("TD5"));

            var package = storeAppList.FirstOrDefault(p => p.DisplayName.Contains("Netflix"));

            if (package != null)
            {
                StoreAppLauncher.StoreAppLauncher.LaunchApp(package);                
            }
                
            Console.WriteLine();
            Console.ReadLine();            
        }
    }
}
