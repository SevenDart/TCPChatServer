using System;
using System.Collections.Generic;
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
            }   
        }

        void SendFileMessage(Message message, Client client)
        {
            byte[] protocolData = {(byte) Message.MessageType.Binary, (byte) (message.Username.Length * 2)};
            client.Stream.Write(protocolData, 0, 2);
            client.Stream.Write(Encoding.Unicode.GetBytes(message.Username), 0, protocolData[1]);
            byte[] bytes = BitConverter.GetBytes(message.File.Filename.Length * 2);
            client.Stream.Write(bytes, 0, 4); 
            client.Stream.Write(Encoding.Unicode.GetBytes(message.File.Filename), 0, message.File.Filename.Length * 2);
            client.Stream.Write(message.File.Data, 0, message.File.Data.Length);
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
    }
}
