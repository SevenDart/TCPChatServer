namespace ConsoleTCPchat
{
    public class NetworkFile
    {
        internal string Filename { get; }
        internal byte[] Data { get; }

        public NetworkFile(string filename, byte[] data)
        {
            Filename = filename;
            Data = data;
        }
    }
}