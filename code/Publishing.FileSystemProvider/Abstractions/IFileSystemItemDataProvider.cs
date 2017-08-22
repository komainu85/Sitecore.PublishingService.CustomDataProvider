using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Publishing.FileSystemProvider.Model;
using Publishing.Integration.FileSystemProvider.Model;

namespace Publishing.FileSystemProvider
{
    public interface IFileSystemItemDataProvider
    {
        Task<IEnumerable<SkinnyItemModel>> GetSkinnyItems(IFileSystemConnection connection, IReadOnlyCollection<ItemIdEntity> idItemEntity);

        Task<IEnumerable<Tuple<SkinnyItemModel, ItemModel>>> GetItemModels(IFileSystemConnection connection, IReadOnlyCollection<ItemIdEntity> idItemEntity);
    }
}
