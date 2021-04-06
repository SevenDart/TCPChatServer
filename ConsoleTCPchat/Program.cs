using System;
using System.Net.Sockets;

namespace ConsoleTCPchat
{
    internal class Program
    {
        public static bool IsAvailable(string ip, int port)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static void Main(string[] args)
        {
            int port;
            do
            {
                Console.WriteLine("Input port:");
                port = Convert.ToInt32(Console.ReadLine());    
            } while (IsAvailable("127.0.0.1", port));
            
            Server server = new Server(port);
            server.Listen();
        }
    }
}