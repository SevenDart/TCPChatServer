namespace ConsoleTCPchat
{
    internal class Message
    {
        internal enum MessageType : byte
        {
            Nothing,
            Text,
            Image,
            Binary,
            Disconnect
        }
        
        internal byte[] Data { get; }
        internal string Username { get; }
        internal MessageType Type { get; }
        internal NetworkFile File { get; }
        
        public Message(byte[] data, MessageType type, string username, NetworkFile file)
        {
            this.Data = data;
            this.Type = type;
            Username = username;
            File = file;
        }
    }

}