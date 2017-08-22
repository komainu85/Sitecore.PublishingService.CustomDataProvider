using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Publishing.FileSystemProvider.Model;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Publishing.Item;
using Sitecore.Framework.Publishing.Sql;
using Sitecore.Framework.Publishing.Data.AdoNet;

namespace Publishing.FileSystemProvider
{
    public class DatabaseIdTableRepository : IDatabaseIdTableRepository
    {
        private readonly IDatabaseConnection _connection;

        public DatabaseIdTableRepository(IDatabaseConnection connection)
        {
            Condition.Requires(connection, nameof(connection)).IsNotNull();

            _connection = connection;
        }

        public async Task<IEnumerable<ItemIdEntity>> GetItemIdData(string prefix)
        {
            Condition.Requires(prefix, nameof(prefix)).IsNotNullOrEmpty();

            var result = await _connection.ExecuteAsync(async conn =>
                await conn.QueryAsync<ItemIdEntity>(
                    Schema.Queries.IdTableByPrefixQuery,
                    new
                    {
                        Prefix = prefix
                    },
                    null,
                    _connection.CommandTimeout,
                    CommandType.Text).ConfigureAwait(false)).ConfigureAwait(false);

            return result;
        }

        public async Task<IEnumerable<ItemIdEntity>> GetItemIdData(string prefix, IReadOnlyCollection<Guid> ids)
        {
            Condition.Requires(prefix, nameof(prefix)).IsNotNullOrEmpty();
            Condition.Requires(ids, nameof(ids)).IsNotNull();

            var result = await _connection.ExecuteAsync(async conn => 
                await conn.QueryAsync<ItemIdEntity>(
                    Schema.Queries.IdTableByIdAndPrefixQuery,
                    new
                    {
                        Prefix = prefix,
                        Ids = SqlServerUtils.BuildIdTable(ids)
                    },
                    null,
                    _connection.CommandTimeout,
                    CommandType.Text).ConfigureAwait(false)).ConfigureAwait(false);

            return result;
        }

        public Task<IEnumerable<ItemIdEntity>> GetItemIdData(string prefix, IReadOnlyCollection<IItemVariantIdentifier> identifiers)
        {
            Condition.Requires(prefix, nameof(prefix)).IsNotNullOrEmpty();
            Condition.Requires(identifiers, nameof(identifiers)).IsNotNull();

            var ids = identifiers.Select(x => x.Id).ToArray();

            return GetItemIdData(prefix, ids);
        }
    }
}