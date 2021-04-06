using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleTCPchat
{
    public class Server
    {
        private int serverPort = 8888;
        private List<Client> _clients;
        private int _bufferSize = 2048;

        public void AddConnection(Client client)
        {
            _clients.Add(client);
        }

        public void RemoveConnection(Client client)
        {
            _clients.Remove(client);
        }

        protected internal void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, serverPort);
            listener.Start();
            Console.WriteLine("Server started. Waiting for connections...");
            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();

                Client client = new Client(tcpClient, this);
                Thread clientThread = new Thread(client.Process);
                clientThread.Start();
            }
        }

        internal void BroadcastMessage(Message message, int clientId)
        {
            foreach (var client in _clients)
            {
                if (client.Id != clientId)
                    SendMessage(message, client);
            }
        }


        void SendMessage(Message message, Client client)
        {
            switch (message.Type)
            {
                case Message.MessageType.Text:
                    SendTextMessage(message, client);
                    break;
                case Message.MessageType.Binary:
                    SendFileMessage(message, client);
                    break;
                case Message.MessageType.Image:
                    SendFileMessage(message, client);
                    break;
            }   
        }

        void SendFileMessage(Message message, Client client)
        {
            byte[] protocolData = {(byte) message.Type, (byte) (message.Username.Length * 2)};
            client.Stream.Write(protocolData, 0, 2);
            client.Stream.Write(Encoding.Unicode.GetBytes(message.Username), 0, protocolData[1]);
            
            byte[] bytesLength = BitConverter.GetBytes(message.File.Filename.Length * 2);
            client.Stream.Write(bytesLength, 0, 4);
            client.Stream.Write(Encoding.Unicode.GetBytes(message.File.Filename), 0, message.File.Filename.Length * 2);
            
            byte[] fileSizeBytes = BitConverter.GetBytes(message.File.FileSize);
            client.Stream.Write(fileSizeBytes, 0, 4);
            
            using (FileStream file = File.Open(@".\Downloads\" + message.File.Filename, FileMode.Open))
            {
                byte[] buffer = new byte[_bufferSize];
                do
                {
                    int length = file.Read(buffer, 0, _bufferSize);
                    client.Stream.Write(buffer, 0, length);
                } while (file.Position != file.Length);
            }
            
        }

        void SendTextMessage(Message message, Client client)
        {
            byte[] protocolData = {(byte)message.Type, (byte)(message.Username.Length * 2)};
            client.Stream.Write(protocolData, 0, 2);
            client.Stream.Write(Encoding.Unicode.GetBytes(message.Username), 0, protocolData[1]);
            client.Stream.Write(message.Data, 0, message.Data.Length);
        }

        public Server()
        {
            _clients = new List<Client>();
        }
        
        public Server(int port)
        {
            serverPort = port;
            _clients = new List<Client>();
        }
    }
}
