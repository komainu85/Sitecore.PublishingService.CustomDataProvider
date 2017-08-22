using Sitecore.Framework.Publishing.Data;

namespace Publishing.FileSystemProvider
{
    public interface IDatabaseIdTableRepositoryBuilder
    {
        IDatabaseIdTableRepository Build(string connectionName, DataAccessContextType dataAccessContext);
    }
}