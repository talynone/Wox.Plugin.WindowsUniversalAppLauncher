using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreAppLauncher.Models
{
    using global::StoreAppLauncher.Helpers;

    public class ManifestInfo
    {
        public List<ManifestApplication> _apps = new List<ManifestApplication>();

        public NativeApiHelper.IAppxManifestProperties Properties { get; set; }

        public ManifestInfo()
        {
        }

        public string FullName { get; set; }

        public string ApplicationUserModelId { get; set; }
        

        public PackageArchitecture ProcessorArchitecture { get; set; }

        public List<ManifestApplication> Apps
        {
            get
            {
                return _apps;
            }
        }

        public string GetPropertyStringValue(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return NativeApiHelper.GetStringValue(Properties, name);
        }

        public bool GetPropertyBoolValue(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            return NativeApiHelper.GetBoolValue(Properties, name);
        }

        public string LoadResourceString(string resource)
        {
            return NativeApiHelper.LoadResourceString(FullName, resource);
        }
    }
}
