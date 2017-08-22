using Newtonsoft.Json;
using Publishing.Integration.FileSystemProvider.Abstractions;
using Publishing.Integration.FileSystemProvider.Model;
using Sitecore.Data;
using Sitecore.Data.IDTables;
using System;
using System.IO;

namespace Publishing.Integration.FileSystemProvider
{
    public static class Extensions
    {
        private static JsonSerializerSettings _serailzierSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static ItemModel ToModel(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                return null;

            using (var stream = fileInfo.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                var instance = JsonConvert.DeserializeObject<ItemModel>(content, _serailzierSettings);                                
                return instance;
            }
        }

        public static string ToRaw(this ItemModel model)
        {
            return JsonConvert.SerializeObject(model, _serailzierSettings);            
        }

        public static bool WriteChildItem(this IFileSystemRepository repo, ItemModel model, string parentId)
        {
            return repo.WriteChildItem(model.ModelId, parentId, model.ToRaw());
        }

        public static bool WriteItem(this IFileSystemRepository repo, ItemModel model)
        {
            return repo.WriteItem(model.ModelId, model.ToRaw());
        }

        public static bool MoveItem(this IFileSystemRepository repo, IDTableEntry item, IDTableEntry parent)
        {
            return repo.MoveItem(item.ID.ToString(), parent.ToString());
        }

        public static FileInfo GetMedia(this IFileSystemRepository repo, Guid id)
        {
            return repo.GetMedia(id.ToString());
        }

        public static Stream ReadMedia(this IFileSystemRepository repo, Guid id)
        {
            return repo.ReadMedia(id.ToString());
        }

        public static bool WriteMedia(this IFileSystemRepository repo, Guid id, Stream stream)
        {
            return repo.WriteMedia(id.ToString(), stream);
        }

        public static bool RemoveMedia(this IFileSystemRepository repo, Guid id)
        {
            return repo.RemoveMedia(id.ToString());
        }
    }
}
