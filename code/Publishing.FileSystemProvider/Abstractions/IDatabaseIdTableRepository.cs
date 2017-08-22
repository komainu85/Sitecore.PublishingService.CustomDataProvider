using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Publishing.FileSystemProvider.Model;
using Sitecore.Framework.Publishing.Item;

namespace Publishing.FileSystemProvider
{
    public interface IDatabaseIdTableRepository
    {
        Task<IEnumerable<ItemIdEntity>> GetItemIdData(string prefix);

        Task<IEnumerable<ItemIdEntity>> GetItemIdData(string prefix, IReadOnlyCollection<Guid> ids);

        Task<IEnumerable<ItemIdEntity>> GetItemIdData(string prefix, IReadOnlyCollection<IItemVariantIdentifier> locators);
    }
}