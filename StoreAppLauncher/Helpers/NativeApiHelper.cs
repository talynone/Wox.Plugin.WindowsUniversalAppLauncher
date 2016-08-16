using System;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace StoreAppLauncher.Helpers
{        
    public static class NativeApiHelper
    {        
        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        [DllImport("kernel32")]
        static extern int OpenPackageInfoByFullName([MarshalAs(UnmanagedType.LPWStr)] string fullName, uint reserved, out IntPtr packageInfo);

        [DllImport("kernel32")]
        static extern int GetPackageApplicationIds(IntPtr pir, ref int bufferLength, byte[] buffer, out int count);

        [DllImport("kernel32")]
        static extern int ClosePackageInfo(IntPtr pir);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int SHCreateStreamOnFileEx(string fileName,int grfMode,int attributes,bool create,IntPtr reserved,out IStream stream);

        [Guid("5842a140-ff9f-4166-8f5c-62f5b7b0c781"), ComImport]
        public class AppxFactory {}

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

        public static string LoadIndirectStringEx(string resource, string resourceKey)
        {
            var outBuffer = new StringBuilder(1024);

            string source = string.Format("@{{{0}? {1}}}", resource, resourceKey);

            int loadIndirectStringHResult = SHLoadIndirectString(source, outBuffer, outBuffer.Capacity, IntPtr.Zero);

            if (loadIndirectStringHResult != 0)
            {
                return null;
            }

            return outBuffer.ToString();
        }

        public static string LoadResourceString(string packageFullName, string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                return null;
            }

            const string resourceScheme = "ms-resource:";

            //Console.WriteLine(resourceKey);

            if (!resourceKey.StartsWith(resourceScheme))
            {
                return resourceKey;
            }                

            string part = resourceKey.Substring(resourceScheme.Length);
            string url;

            if (part.StartsWith("//"))
            {
                url = resourceScheme + part;
            }
            else if (part.StartsWith("/"))
            {
                url = resourceScheme + "//" + part;
            }
            else
            {
                url = resourceScheme + "///resources/" + part;
            }

            var extractedValue = LoadIndirectStringEx(packageFullName, url);

            //TODO: Uh oh
//            if (string.IsNullOrEmpty(extractedValue))
//            {
//                Console.WriteLine(packageFullName);
//            }            

            return extractedValue;
        }

        public static uint LaunchApp(string packageFullName, string arguments = null)
        {
            var pir = IntPtr.Zero;

            try
            {
                int openPackageInfoByFullNameErrorCodeResult = OpenPackageInfoByFullName(packageFullName, 0, out pir);

                Debug.Assert(openPackageInfoByFullNameErrorCodeResult == 0);

                if (openPackageInfoByFullNameErrorCodeResult != 0)
                {
                    throw new Win32Exception(openPackageInfoByFullNameErrorCodeResult);
                }

                int length = 0;
                int count;

                //f the function succeeds it returns ERROR_SUCCESS. Otherwise, the function 
                //returns an error code. The possible error codes include the following.
                //ERROR_INSUFFICIENT_BUFFER                
                int getPackageApplicationIdsErrorResult = GetPackageApplicationIds(pir, ref length, null, out count);

                var buffer = new byte[length];

                int getPackageApplicationIdsErrorCodeResult = GetPackageApplicationIds(pir, ref length, buffer, out count);
                Debug.Assert(getPackageApplicationIdsErrorCodeResult == 0);

                if (getPackageApplicationIdsErrorCodeResult != 0)
                {
                    throw new Win32Exception(openPackageInfoByFullNameErrorCodeResult);
                }                    

                var appUserModelId = Encoding.Unicode.GetString(buffer, IntPtr.Size * count, length - IntPtr.Size * count);

                var activation = new ApplicationActivationManager() as IApplicationActivationManager;

                uint pid;

                int activateApplicationHResult = activation.ActivateApplication(appUserModelId, arguments ?? string.Empty, ActivateOptions.NoErrorUI, out pid);

                if (activateApplicationHResult < 0)
                {
                    Marshal.ThrowExceptionForHR(activateApplicationHResult);
                }
                    
                return pid;
            }
            finally
            {
                if (pir != IntPtr.Zero)
                {
                    ClosePackageInfo(pir);
                }                    
            }
        }
    }
}
