using System;

namespace Publishing.FileSystemProvider
{
    public class FileSystemConnectionOptions
    {
        public string RootFolder { get; set; }

        public string IdTablePrefix { get; set; }

        public string IdTableConnection { get; set; }
    }
}
