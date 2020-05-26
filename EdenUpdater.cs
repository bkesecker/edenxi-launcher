using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace EdenLoginManager
{
    public class EdenUpdater
    {
        Socket sock;
        string addr;
        ushort port;
        public string pluginFileName;
        protected internal byte[] UniqueKey = new byte[8];

        public EdenUpdater(string Address, ushort Port)
        {
            addr = Address;
            port = Port;
            pluginFileName = Utilities.RandomString(20);
        }

        public bool Connect()
        {
            sock = EdenNetworking.Connect(addr, port);
            if (sock != null)
                return true;
            return false;
        }
        
        public bool CleanupPlugins()
        {
            DirectoryInfo di = new DirectoryInfo(Utilities.GetEdenAppDataPath());
            FileInfo[] files = di.GetFiles("*.dll")
                                 .Where(p => p.Extension == ".dll").ToArray();
            foreach (FileInfo file in files)
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch {
                    return false;
                }

            return true;
        }
        public bool DownloadPlugin()
        {
            UInt32 FileSize = 0;
            byte[] downloadRequest = { 0x02 };
            byte[] fileData;
            EdenNetworking.SendData(sock, downloadRequest);

            FileSize = EdenNetworking.ReadUInt32(sock);
            
            if (FileSize > 0)
            {
                fileData = new byte[FileSize];
                UInt32 bytesLeft = FileSize;
                UInt32 bytesRead = 0;
                UInt32 totalBytesRead = 0;
                bool GoodToGo = true;
                do
                {
                    byte[] bytes = EdenNetworking.ReadData(sock, out bytesRead);
                    if (bytesRead > 0)
                    {
                        Array.Copy(bytes, 0, fileData, totalBytesRead, bytesRead);
                        totalBytesRead += bytesRead;
                        bytesLeft -= bytesRead;
                    }
                    else
                        GoodToGo = false;

                } while (bytesLeft > 0 && GoodToGo);
                if (GoodToGo)
                {
                    try
                    {
                        using (BinaryWriter binWriter = new BinaryWriter(File.Open(Utilities.GetEdenFullPluginFileName(pluginFileName), FileMode.Create)))
                        {
                            binWriter.Write(fileData);
                        }
                        Array.Copy(fileData, fileData.Length - 8, UniqueKey, 0, 8);
                    }
                    catch (IOException ioexp)
                    {
                        MessageBox.Show("Unable to write plugin file to AppData : {0}", ioexp.Message);
                        GoodToGo = false;
                    }
                }

                return GoodToGo;
            }

            return false;
        }

        public bool ValidatePlugin()
        {
            byte[] validateRequest = new byte[9] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Array.Copy(UniqueKey, 0, validateRequest, 1, 8);
            EdenNetworking.SendData(sock, validateRequest);
            uint Received = 0;
            byte[] recv = EdenNetworking.ReadData(sock, out Received);
            if (Received > 0 && recv[0] == 0x01)
            {
                return true;
            }
            return false;
        }

    }
}
