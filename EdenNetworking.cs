using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EdenLoginManager
{
    public static class EdenNetworking
    {
        public static Socket Connect(string address, ushort port)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                //Console.WriteLine(DateTime.Now.ToString() + " Establishing Connection to {0}", address);
                s.Connect(address, port);

                return s;
            }
            catch (SocketException e)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Unable to connect to : {0} : {1}", address, e.Message);
            }

            return null;
        }

        public static void Disconnect(Socket s)
        {
            s.Disconnect(false);
            s.Dispose();
        }

        public static byte[] ReadData(Socket s, out UInt32 Length)
        {
            byte[] data = new byte[1024];
            try
            {
                Length = (UInt32)s.Receive(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Unable to receive data : {0}", e.Message);
                Length = 0;
            }
            return data;
        }
        
        public static UInt32 ReadUInt32(Socket s)
        {
            byte[] data = new byte[1024];
            UInt32 value = 0;
            try
            {
                int bytesRead = s.Receive(data, 4, SocketFlags.None);
                if (bytesRead == 4)
                    value = BitConverter.ToUInt32(data, 0);
            }
            catch (SocketException e)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Unable to receive data : {0}", e.Message);
            }
            return value;

        }

        public static int SendData(Socket s, byte[] data, int Length = 0)
        {
            int BytesSent = 0;
            if (Length == 0)
                Length = data.Length;
            try
            {
                BytesSent = s.Send(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Unable to receive data : {0}", e.Message);
                BytesSent = 0;
            }
            return BytesSent;
        }
    }
}
