using System;
using Sitecore.Framework.Publishing.Data;

namespace Publishing.FileSystemProvider
{
    public interface IFileSystemConnection: IConnection
    {
        string RootFolder { get; }

        string IdTableConnectionName { get; }

        string IdTablePrefix { get; }
    }
}