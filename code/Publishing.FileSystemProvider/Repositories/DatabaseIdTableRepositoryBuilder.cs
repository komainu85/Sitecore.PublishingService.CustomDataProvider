using System;
using Sitecore.Framework.Publishing.Data;
using Sitecore.Framework.Publishing.Data.AdoNet;

namespace Publishing.FileSystemProvider
{
    public class DatabaseIdTableRepositoryBuilder : IDatabaseIdTableRepositoryBuilder
    {
        private readonly IConnectionFactory _connectionFactory;

        public DatabaseIdTableRepositoryBuilder(IServiceProvider provider, IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IDatabaseIdTableRepository Build(string connectionName, DataAccessContextType dataAccessContext)
        {
            var connection = _connectionFactory.CreateConnection<IDatabaseConnection>(connectionName, dataAccessContext);

            return new DatabaseIdTableRepository(connection);
        }
    }
}