using System.IO;

namespace Publishing.Integration.FileSystemProvider.Abstractions
{
    public interface IFileSystemRepository
    {
        string RootId { get; }

        DirectoryInfo Root { get; }

        string Extension { get; }

        FileInfo GetItem(string id);

        FileInfo GetMedia(string id);

        FileInfo[] GetChildren(string id);

        bool WriteItem(string id, string content);

        bool WriteChildItem(string id, string parentId, string content);

        bool RemoveItem(string id);

        bool MoveItem(string id, string parentId);
        
        Stream ReadMedia(string id);

        bool WriteMedia(string id, Stream stream);

        bool RemoveMedia(string id);
    }
}
