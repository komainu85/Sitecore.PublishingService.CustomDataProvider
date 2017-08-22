using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Framework.Publishing.Data;
using Sitecore.Framework.Publishing.Data.Repository;
using Sitecore.Framework.Publishing.Item;

namespace Publishing.FileSystemProvider
{
    public class FileSystemItemReadRepositoryBuilder : DefaultRepositoryBuilder<IIndexableItemReadRepository, FileSystemItemReadRepository, IFileSystemConnection>
    {
        public FileSystemItemReadRepositoryBuilder(IServiceProvider services) : base(services)
        {

        }
    }
}
