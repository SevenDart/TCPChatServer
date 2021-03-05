using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ConsoleTCPchat;

namespace ConsoleTCPchat
{
    public class Client
    {
        private static int _currentId;
        private Server _server;
        private TcpClient _tcpClient;
        protected internal NetworkStream Stream;
        public int Id { get; }
        private string _username;

        public Client(TcpClient tcpClient, Server server)
        {
            Id = _currentId++;
            _server = server;
            _tcpClient = tcpClient;
            _server.AddConnection(this);
        }

        protected internal void Process()
        {
            Stream = _tcpClient.GetStream();
            var message = GetMessage();
            _username = message.Username;
            _server.BroadcastMessage(new Message(Encoding.Unicode.GetBytes(_username + " entered chat"), Message.MessageType.Text, _username, null), Id);
            Console.WriteLine(_username + " entered chat");
            try
            {
                while (true)
                {
                    message = GetMessage();
                    switch (message.Type)
                    {
                        case Message.MessageType.Text:
                            Console.WriteLine(_username + ": " + Encoding.Unicode.GetString(message.Data, 0, message.Data.Length));
                            break;
                        case Message.MessageType.Image:
                            Console.WriteLine(_username + ": image");
                            break;
                        case Message.MessageType.Binary:
                            Console.WriteLine(_username + ": binary message");
                            break;
                    }
                    _server.BroadcastMessage(message, Id);
                }
            }
            catch (Exception e)
            {
                Disconnect();
            }
            
        }
        

        Message GetMessage()
        {
            byte[] protocolData = new byte[2];
            Stream.Read(protocolData, 0, 2);
            if (protocolData[0] == 0) return new Message(null, 0, null, null);
            byte[] usernameBytes = new byte[protocolData[1]];
            Stream.Read(usernameBytes, 0, protocolData[1]);
            string username = Encoding.Unicode.GetString(usernameBytes, 0, protocolData[1]);

            if (protocolData[0] == (int) Message.MessageType.Disconnect)
            {
                Disconnect();
            }
                
            if (protocolData[0] == (int)Message.MessageType.Binary || protocolData[0] == (int)Message.MessageType.Image)
            {
                return new Message(null, (Message.MessageType)protocolData[0], username, GetFile());
            }
            
            byte[] result = new byte[0];
            int resInd = 0;
            byte[] buffer = new byte[256];
            int length;
            do
            {
                length = Stream.Read(buffer, 0, buffer.Length);
                Array.Resize(ref result, result.Length + length);
                for (int i = 0; i < length; i++)
                    result[resInd + i] = buffer[i];
            } while (Stream.DataAvailable);
            return new Message(result, (Message.MessageType) protocolData[0], username, null);
        }
        
        NetworkFile GetFile()
        {
            using (BinaryReader reader = new BinaryReader(Stream))
            {
                int filenameLength = reader.ReadInt32();
                byte[] filenameBytes = reader.ReadBytes(filenameLength);
                string filename = Encoding.Unicode.GetString(filenameBytes, 0, filenameLength);
                int length;
                byte[] buffer = new byte[1024];
                byte[] result = new byte[0];
                int resInd = 0;
                do
                {
                    length = Stream.Read(buffer, 0, buffer.Length);
                    Array.Resize(ref result, length + result.Length);
                    for (int i = 0; i < length; i++)
                        result[resInd + i] = buffer[i];
                } while (Stream.DataAvailable);
                return new NetworkFile(filename, result);
            }
        }
        
        void Disconnect()
        {
            byte[] text = Encoding.Unicode.GetBytes("Left chat");
            try
            {
                _server.BroadcastMessage(new Message(text, Message.MessageType.Text, _username, null), Id);
            }
            catch (Exception e)
            {
                _server.RemoveConnection(this);
                Thread.CurrentThread.Abort();    
            }
            _server.RemoveConnection(this);
            Thread.CurrentThread.Abort();
        }
    }
}