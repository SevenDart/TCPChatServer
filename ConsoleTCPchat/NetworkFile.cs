namespace ConsoleTCPchat
{
    public class NetworkFile
    {
        internal string Filename { get; }

        internal int FileSize { get; }

        public NetworkFile(string filename, int fileSize)
        {
            Filename = filename;
            FileSize = fileSize;
        }
    }
}