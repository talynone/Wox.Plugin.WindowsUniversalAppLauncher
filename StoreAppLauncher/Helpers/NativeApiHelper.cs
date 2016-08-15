using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreAppLauncher.Helpers
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    

    public static class NativeApiHelper
    {        
        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        public static string LoadIndirectStringEx(string resource, string resourceKey)
        {
            var outBuffer = new StringBuilder(1024);

            string source = string.Format("@{{{0}? {1}}}", resource, resourceKey);
            
            int result = SHLoadIndirectString(source, outBuffer, outBuffer.Capacity, IntPtr.Zero);

            return outBuffer.ToString();
        }

        [DllImport("kernel32")]
        static extern int OpenPackageInfoByFullName([MarshalAs(UnmanagedType.LPWStr)] string fullName, uint reserved, out IntPtr packageInfo);

        [DllImport("kernel32")]
        static extern int GetPackageApplicationIds(IntPtr pir, ref int bufferLength, byte[] buffer, out int count);

        [DllImport("kernel32")]
        static extern int ClosePackageInfo(IntPtr pir);


        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int SHCreateStreamOnFileEx(
        string fileName,
        int grfMode,
        int attributes,
        bool create,
        IntPtr reserved,
        out IStream stream);

        [Guid("5842a140-ff9f-4166-8f5c-62f5b7b0c781"), ComImport]
        public class AppxFactory
        {
        }

        [Guid("BEB94909-E451-438B-B5A7-D79E767B75D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppxFactory
        {
            void _VtblGap0_2(); // skip 2 methods

            IAppxManifestReader CreateManifestReader(IStream inputStream);
        }

        [Guid("4E1BD148-55A0-4480-A3D1-15544710637C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppxManifestReader
        {
            void _VtblGap0_1(); // skip 1 method

            IAppxManifestProperties GetProperties();

            void _VtblGap1_5(); // skip 5 methods

            IAppxManifestApplicationsEnumerator GetApplications();
        }

        [Guid("9EB8A55A-F04B-4D0D-808D-686185D4847A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppxManifestApplicationsEnumerator
        {
            IAppxManifestApplication GetCurrent();

            bool GetHasCurrent();

            bool MoveNext();
        }

        [Guid("5DA89BF4-3773-46BE-B650-7E744863B7E8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppxManifestApplication
        {
            [PreserveSig]
            int GetStringValue(
                [MarshalAs(UnmanagedType.LPWStr)] string name,
                [MarshalAs(UnmanagedType.LPWStr)] out string vaue);
        }        

        [Guid("03FAF64D-F26F-4B2C-AAF7-8FE7789B8BCA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppxManifestProperties
        {
            [PreserveSig]
            int GetBoolValue([MarshalAs(UnmanagedType.LPWStr)] string name, out bool value);

            [PreserveSig]
            int GetStringValue(
                [MarshalAs(UnmanagedType.LPWStr)] string name,
                [MarshalAs(UnmanagedType.LPWStr)] out string vaue);
        }

        public static string GetStringValue(IAppxManifestApplication app, string name)
        {
            string value;
            app.GetStringValue(name, out value);
            return value;
        }

        public static string GetStringValue(IAppxManifestProperties props, string name)
        {
            if (props == null) return null;

            string value;
            props.GetStringValue(name, out value);
            return value;
        }

        public static bool GetBoolValue(IAppxManifestProperties props, string name)
        {
            bool value;
            props.GetBoolValue(name, out value);
            return value;
        }

        public static string LoadResourceString(string packageFullName, string resource)
        {
            if (packageFullName == null) throw new ArgumentNullException("packageFullName");

            if (string.IsNullOrWhiteSpace(resource)) return null;

            const string resourceScheme = "ms-resource:";
            if (!resource.StartsWith(resourceScheme)) return resource;

            string part = resource.Substring(resourceScheme.Length);
            string url;

            if (part.StartsWith("/"))
            {
                url = resourceScheme + "//" + part;
            }
            else
            {
                url = resourceScheme + "///resources/" + part;
            }

            string source = string.Format("@{{{0}? {1}}}", packageFullName, url);
            var sb = new StringBuilder(1024);
            int i = SHLoadIndirectString(source, sb, sb.Capacity, IntPtr.Zero);
            if (i != 0) return null;

            return sb.ToString();
        }

        public static uint LaunchApp(string packageFullName, string arguments = null)
        {
            IntPtr pir = IntPtr.Zero;
            try
            {
                int error = OpenPackageInfoByFullName(packageFullName, 0, out pir);
                Debug.Assert(error == 0);
                if (error != 0)
                    throw new Win32Exception(error);

                int length = 0, count;
                GetPackageApplicationIds(pir, ref length, null, out count);

                var buffer = new byte[length];
                error = GetPackageApplicationIds(pir, ref length, buffer, out count);
                Debug.Assert(error == 0);
                if (error != 0)
                    throw new Win32Exception(error);

                var appUserModelId = Encoding.Unicode.GetString(buffer, IntPtr.Size * count, length - IntPtr.Size * count);

                var activation = (IApplicationActivationManager)new ApplicationActivationManager();
                uint pid;
                int hr = activation.ActivateApplication(appUserModelId, arguments ?? string.Empty, ActivateOptions.NoErrorUI, out pid);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
                return pid;
            }
            finally
            {
                if (pir != IntPtr.Zero)
                    ClosePackageInfo(pir);
            }

        }
    }
}
