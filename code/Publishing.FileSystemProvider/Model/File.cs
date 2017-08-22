using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publishing.FileSystemProvider.Model
{
    public class DataProviderFile
    {
        public string FileName { get; }

        public string Extension { get; }

        public string Path { get; }

        public DataProviderFile(string fileName, string extension, string path)
        {
            FileName = fileName.Contains(extension) ? fileName.Replace(extension, "") : fileName;
            Extension = extension;
            Path = path;
        }
    }
}
