using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdenLoginManager
{
    public class EdenClient
    {
        [DllImport("kernel32.dll")]
        public static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(int hModule,
            [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(int hModule);

        private EdenUpdater eu;
        public struct Configuration
        {
            public string server;
            public ushort port;
        }

        public Configuration config = new Configuration();

        public EdenClient(string[] args)
        {
            config.server = "";
            config.port = 0;

            for (int i = 1; i < args.Length;)
            {
                switch(args[i].ToUpper())
                {
                    case "--SERVER":
                        {
                            if (i + 1 < args.Length)
                                config.server = args[i + 1];
                            i++;
                        }
                        break;
                }
                i++;
            }
        }

        public delegate int XiLoaderMain(int argc, IntPtr[] argv);

        public bool LoadEdenLibrary()
        {
            string fileName = Utilities.GetEdenFullPluginFileName(eu.pluginFileName);
            int hModule = LoadLibrary(fileName);
            if (hModule == 0) return false;
            IntPtr intPtr = GetProcAddress(hModule, "XiLoaderMain");
            XiLoaderMain load = (XiLoaderMain)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(XiLoaderMain));
            string[] argv = Environment.GetCommandLineArgs();
            IntPtr[] arrayOfStrings = new IntPtr[argv.Length + 2];

            try
            {
                for (int i = 0; i < argv.Length; i++)
                {
                    arrayOfStrings[i] = Marshal.StringToCoTaskMemAnsi(argv[i]);
                }

                arrayOfStrings[argv.Length] = Marshal.StringToCoTaskMemAnsi("--ukey");
                arrayOfStrings[argv.Length + 1] = Marshal.StringToCoTaskMemAnsi(Encoding.UTF8.GetString(eu.UniqueKey));

                load(arrayOfStrings.Length, arrayOfStrings);
            }
            catch (Exception)
            {
                // Managed Debugging Assistant 'PInvokeStackImbalance' : 'A call to PInvoke function
                // 'edenxi!EdenLoginManager.EdenClient+XiLoaderMain::Invoke' has unbalanced the stack. This is likely
                // because the managed PInvoke signature does not match the unmanaged target signature. Check that
                // the calling convention and parameters of the PInvoke signature match the target unmanaged signature.'
            }
            finally
            {
                for (int i = 0; i < arrayOfStrings.Length; i++)
                {
                    Marshal.FreeCoTaskMem(arrayOfStrings[i]);
                }
            }

            return false;
        }

        public bool Initialize(string Address, ushort Port)
        {
            eu = new EdenUpdater(Address, Port);
            if (eu.Connect())
            {
                eu.CleanupPlugins();
                if (eu.DownloadPlugin() && eu.ValidatePlugin())
                {
                    //Console.WriteLine("Plugin Downloaded, Loading...");
                    return true;
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString() + " Plugin download failed.");
                }
            }
            return false;
        }

    }
}
