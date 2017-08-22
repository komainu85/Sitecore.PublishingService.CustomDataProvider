using Publishing.Integration.FileSystemProvider.Abstractions;
using Sitecore.Data.IDTables;
using Sitecore.Diagnostics;
using System.IO;
using System.Linq;
using System;

namespace Publishing.Integration.FileSystemProvider
{
    public class FileSystemRepository : IFileSystemRepository
    {
        /*
         * Example structure
         * root
         * |- {id}.ext
         * |- {id}
         *      |- {id}.ext
         * |- {id}.ext
         * |- media
         *      | - {id}
         *      | - {id}
         * etc
         */
         
        private DirectoryInfo _media;

        public FileSystemRepository(string rootPath, string extension, string rootId)
        {
            Assert.ArgumentNotNullOrEmpty(rootPath, "rootPath");

            RootId = rootId;
            Extension = extension;
            Init(rootPath);            
        }

        public string RootId
        {
            get;
            protected set;
        }

        public DirectoryInfo Root
        {
            get;
            protected set;
        }
        
        public string Extension
        {
            get;
            protected set;
        }

        public FileInfo GetItem(string id)
        {
            return FindFile(id);
        }

        public FileInfo GetMedia(string id)
        {
            return FindMedia(id);
        }

        public FileInfo[] GetChildren(string id)
        {
            Assert.ArgumentNotNull(id, "id");

            if (id == RootId)
            {
                return Root.GetFiles();
            }
            else
            {
                var itemRoot = Root.GetDirectories(id).FirstOrDefault();
                if (itemRoot == null)
                    return new FileInfo[0];

                return itemRoot.GetFiles();
            }
        }
        
        private void Init(string rootPath)
        {
            var root = new DirectoryInfo(rootPath);
            if (!root.Exists)
                root.Create();

            root.Refresh();
            Root = root;

            _media = root.CreateSubdirectory("media");            
        }

        public bool WriteItem(string id, string content)
        {
            var entry = FindFile(id);
            if (entry != null && entry.Exists)
            {
                // overwrite
                using (var stream = entry.Open(FileMode.Create))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                }
                return true;
            }            

            return false;
        }

        public bool WriteChildItem(string id, string parentId, string content)
        {
            // if parent is root, root directory == root
            // else find parent file
            // if exists get or create parent folder

            DirectoryInfo parent = null;
            if (parentId == RootId)
                parent = Root;
            else
            {
                parent = GetChildFolder(parentId);
            }

            if (parent == null)
                return false;

            // check child item doesn't exist
            var child = FindFile(id);
            if(child != null && child.Exists && child.Directory != parent)
            {
                // child item exists in the wrong location
                return false;
            }

            var writePath = Path.Combine(parent.FullName, id + Extension);
            File.WriteAllText(writePath, content);

            return true;
        }

        public bool RemoveItem(string id)
        {
            var item = GetItem(id);
            if(item != null && item.Exists)
            {
                item.Delete();               
            }

            var childrenDir = Root.GetDirectories(id).FirstOrDefault();
            if(childrenDir != null && childrenDir.Exists)
            {
                childrenDir.Delete(true);
            }

            return true;
        }

        public bool MoveItem(string id, string parentId)
        {
            var entry = FindFile(id);
            if(entry == null || !entry.Exists)
            {
                return false;
            }

            var parent = GetChildFolder(parentId);
            if (parent == null)
                return false;

            entry.MoveTo(Path.Combine(parent.FullName, entry.Name));

            return true;
        }

        public Stream ReadMedia(string id)
        {
            var media = FindMedia(id);
            if(media.Exists)
            {
                var memStream = new MemoryStream();
                using (var s = media.OpenRead())
                {
                    s.CopyTo(memStream);
                }
                
                return memStream;
            }

            return null;
        }

        public bool WriteMedia(string id, Stream stream)
        {
            var media = FindMedia(id);
            
            using (var mediaStream = media.Open(FileMode.Create))
            {
                stream.CopyTo(mediaStream);
            }

            return true;
        }
        
        public bool RemoveMedia(string id)
        {
            var media = FindMedia(id);
            if (media.Exists)
            {
                media.Delete();
                return true;
            }

            return false;
        }
        
        private FileInfo FindFile(string id)
        {
            Assert.ArgumentNotNull(id, "id");

            return Root.GetFiles(id + Extension, SearchOption.AllDirectories).FirstOrDefault();
        }

        private FileInfo FindMedia(string id)
        {
            Assert.ArgumentNotNull(id, "id");

            return new FileInfo(Path.Combine(_media.FullName, id));
        }

        private DirectoryInfo FindDirectory(string id)
        {
            Assert.ArgumentNotNull(id, "id");
            return Root.GetDirectories(id, SearchOption.AllDirectories).FirstOrDefault();
        }        

        private DirectoryInfo GetChildFolder(string id)
        {
            Assert.ArgumentNotNull(id, "id");

            if (id == RootId)
                return Root;

            var file = FindFile(id);
            if (file == null)
                return null;

            var directory = Path.Combine(file.DirectoryName, id);
            var info = new DirectoryInfo(directory);
            if (!info.Exists)
                info.Create();

            return info;
        }
    }
}
