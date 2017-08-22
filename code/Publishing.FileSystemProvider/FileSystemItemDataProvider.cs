using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Publishing.FileSystemProvider.Model;
using Publishing.Integration.FileSystemProvider.Model;
using Sitecore.Framework.Conditions;

namespace Publishing.FileSystemProvider
{
    public class FileSystemItemDataProvider : IFileSystemItemDataProvider
    {
        public virtual async Task<IEnumerable<SkinnyItemModel>> GetSkinnyItems(
            IFileSystemConnection connection, 
            IReadOnlyCollection<ItemIdEntity> idEntities)
        {
            Condition.Requires(connection, nameof(connection)).IsNotNull();
            Condition.Requires(idEntities, nameof(idEntities)).IsNotNull();

            if (!idEntities.Any()) return Enumerable.Empty<SkinnyItemModel>();

            var fileNames = GetAllFiles(connection);

            var skinnyModels = new List<SkinnyItemModel>();

            foreach (var fileName in fileNames)
            {
                var idItem = idEntities.FirstOrDefault(x => x.Key == fileName.FileName);

                if (idItem != null)
                {
                    skinnyModels.Add(await GetSkinnyModel(fileName, idItem).ConfigureAwait(false));
                }
            }

            return skinnyModels;
        }

        public async Task<IEnumerable<Tuple<SkinnyItemModel, ItemModel>>> GetItemModels(
            IFileSystemConnection connection,
            IReadOnlyCollection<ItemIdEntity> idEntities)
        {
            Condition.Requires(connection, nameof(connection)).IsNotNull();
            Condition.Requires(idEntities, nameof(idEntities)).IsNotNull();

            var skinnyItems = await GetSkinnyItems(connection, idEntities).ConfigureAwait(false);

            var models = new List<Tuple<SkinnyItemModel, ItemModel>>();

            foreach (var skinnyItem in skinnyItems)
            {
                var itemModel = await GetModel(skinnyItem.File).ConfigureAwait(false);
                models.Add(new Tuple<SkinnyItemModel, ItemModel>(skinnyItem, itemModel));
            }

            return models;
        }

        protected virtual IEnumerable<DataProviderFile> GetAllFiles(IFileSystemConnection connection)
        {
            var root = new DirectoryInfo(connection.RootFolder);

            var files = root.GetFiles("*", SearchOption.AllDirectories);

            return files.Select(x => new DataProviderFile(x.Name, x.Extension, x.FullName));
        }

        protected virtual async Task<SkinnyItemModel> GetSkinnyModel(DataProviderFile dataProviderFile, ItemIdEntity itemIdEntity)
        {
            var file = new FileInfo(dataProviderFile.Path);
            using (var stream = file.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                var model = JsonConvert.DeserializeObject<SkinnyItemModel>(content);
                model.File = dataProviderFile;
                model.ParentId = itemIdEntity.ParentId;
                model.ItemId = itemIdEntity.Id;
                return model;
            }
        }

        protected virtual async Task<ItemModel> GetModel(DataProviderFile dataProviderFile)
        {
            var file = new FileInfo(dataProviderFile.Path);
            using (var stream = file.OpenRead())
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<ItemModel>(content);
            }
        }
    }
}