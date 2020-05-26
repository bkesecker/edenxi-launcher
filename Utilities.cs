using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EdenLoginManager
{
    public static class Utilities
    {
        public static string GetEdenAppDataPath()
        {
            var folderName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Eden");
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            return folderName;
        }

        public static string GetEdenFullPluginFileName(string randomName)
        {
            return GetEdenAppDataPath() + "\\" + randomName + ".dll";
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
