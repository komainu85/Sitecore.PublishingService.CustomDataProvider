using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Publishing.Integration.FileSystemProvider.Model;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Publishing.Data.Classic;
using Sitecore.Framework.Publishing.Item;
using Sitecore.Framework.Publishing;

namespace Publishing.FileSystemProvider
{
    public class FileSystemItemReadRepository : IIndexableItemReadRepository
    {
        protected readonly IFileSystemConnection Connection;
        private readonly IFileSystemItemDataProvider _dataProvider;
        private readonly ILogger<FileSystemItemReadRepository> _logger;
        private readonly IDatabaseIdTableRepository _idTableRepository;
        
        public FileSystemItemReadRepository(
            IFileSystemItemDataProvider dataProvider,
            IFileSystemConnection connection,
            IDatabaseIdTableRepositoryBuilder idTableRepositoryBuilder,
            ILogger<FileSystemItemReadRepository> logger)
        {
            Condition.Requires(dataProvider, nameof(dataProvider)).IsNotNull();
            Condition.Requires(connection, nameof(connection)).IsNotNull();
            Condition.Requires(logger, nameof(logger)).IsNotNull();

            _dataProvider = dataProvider;
            Connection = connection;
            _logger = logger;

            _idTableRepository = idTableRepositoryBuilder.Build(connection.IdTableConnectionName, connection.DataAccessContext);
        }

        public async Task<IEnumerable<IItemNode>> GetItemNodes(IReadOnlyCollection<Guid> ids, NodeQueryContext queryContext)
        {
            Condition.Requires(ids, nameof(ids)).IsNotNull();
            Condition.Requires(queryContext, nameof(queryContext)).IsNotNull();

            if (!ids.Any()) return Enumerable.Empty<IItemNode>();

            var entities = (await _idTableRepository.GetItemIdData(Connection.IdTablePrefix, ids).ConfigureAwait(false)).EnsureEnumerated();

            if (!entities.Any()) return Enumerable.Empty<IItemNode>();

            var models = await _dataProvider.GetItemModels(Connection, entities).ConfigureAwait(false);

            var itemNodes = new List<IItemNode>();
            foreach (var model in models)
            {
                var skinnyItemModel = model.Item1;
                var itemModel = model.Item2;

                var properties = new ItemProperties(
                    itemModel.ItemName, 
                    skinnyItemModel.ParentId, 
                    itemModel.TemplateId,
                    null);

                var langVariantFields = new Dictionary<Language, IReadOnlyList<IFieldData>>();
                var variantFields = new Dictionary<IVarianceIdentity, IReadOnlyList<IFieldData>>();

                foreach (var langModel in itemModel.Languages)
                {
                    var lang = Language.Parse(langModel.Language);

                    // Honour the language filter for the query
                    if (!queryContext.LanguageFilter.Contains(lang)) continue;

                    var langVariance = VarianceInfo.LanguageVariant(lang);

                    langVariantFields.Add(
                        lang,
                        langModel.Fields
                            .Where(f => queryContext.FieldFilter.Contains(f.Id)) // Honour the field filter for the query
                            .Select(f => new FieldData(
                                f.Id, 
                                skinnyItemModel.ItemId, 
                                f.Value,
                                langVariance))
                            .ToArray());

                    foreach (var versionModel in langModel.Versions)
                    {
                        var ver = Sitecore.Framework.Publishing.Item.Version.Parse(versionModel.Number);
                        var variance = VarianceInfo.Variant(lang, ver);

                        var fields = versionModel.Fields
                            .Where(f => queryContext.FieldFilter.Contains(f.Id)) // Honour the field filter for the query
                            .Select(f => new FieldData(f.Id, skinnyItemModel.ItemId, f.Value, variance))
                            .ToArray();

                        // Sitecore stores the revision as a field, so retrieve the revision value
                        var revision = ClassicItemRepository.GetClassicRevision(fields);

                        variantFields.Add(new VarianceIdentity(lang, ver, revision), fields);
                    }
                }

                itemNodes.Add(new ItemNode(
                    skinnyItemModel.ItemId,
                    properties,
                    itemModel.Fields
                        .Where(f => queryContext.FieldFilter.Contains(f.Id)) // Honour the field filter for the query
                        .Select(f => new FieldData(f.Id, skinnyItemModel.ItemId, f.Value, VarianceInfo.Invariant))
                        .ToArray(),
                    langVariantFields,
                    variantFields));
            }

            return itemNodes;
        }

        public async Task<IEnumerable<IItemVariant>> GetVariants(IReadOnlyCollection<IItemVariantIdentifier> identifiers)
        {
            Condition.Requires(identifiers, nameof(identifiers)).IsNotNull();

            if (!identifiers.Any()) return Enumerable.Empty<IItemVariant>();

            var entities = (await _idTableRepository.GetItemIdData(Connection.IdTablePrefix, identifiers).ConfigureAwait(false)).EnsureEnumerated();
            if (!entities.Any()) return Enumerable.Empty<IItemVariant>();

            var models = (await _dataProvider.GetItemModels(Connection, entities).ConfigureAwait(false))
                .ToDictionary(m => m.Item1.ItemId);

            var itemVariants = new List<IItemVariant>();

            foreach (var identifier in identifiers)
            {
                Tuple<SkinnyItemModel, ItemModel> itemEntry;
                if (models.TryGetValue(identifier.Id, out itemEntry))
                {
                        var properties = new ItemProperties(
                            itemEntry.Item2.ItemName,
                            itemEntry.Item1.ParentId,
                            itemEntry.Item2.TemplateId,
                            null);

                        var language = itemEntry.Item2.GetLanguage(identifier.Language);
                        var version = language.GetVersion(identifier.Version);

                        var fieldDatas = new List<FieldData>(GetFieldData(identifier, itemEntry.Item2));

                        // Sitecore stores the revision as a field, so retrieve the revision value
                        var revision = ClassicItemRepository.GetClassicRevision(fieldDatas);

                        itemVariants.Add(new ItemVariant(
                            itemEntry.Item1.ItemId,
                            language.Language,
                            version.Number,
                            revision,
                            properties,
                            fieldDatas));
                }
            }

            return itemVariants;
        }

        public async Task<IEnumerable<IItemNodeDescriptor>> GetAllItemDescriptors()
        {
            var ids = await _idTableRepository.GetItemIdData(Connection.IdTablePrefix).ConfigureAwait(false);

            var results = await _dataProvider.GetSkinnyItems(Connection, ids.EnsureEnumerated()).ConfigureAwait(false);

            return results
                .Select(model => new ItemNodeDescriptor(
                    model.ItemId,
                    model.ParentId,
                    model.TemplateId,
                    new Guid[0]))
                .ToList();
        }

        private IEnumerable<FieldData> GetFieldData(IItemVariantIdentifier identifier, ItemModel itemModel)
        {
            var language = itemModel.GetLanguage(identifier.Language);
            var version = language.GetVersion(identifier.Version);
            
            var invariantFields = itemModel.Fields.Select(field => new FieldData(
                field.Id,
                identifier.Id,
                field.Value,
                VarianceInfo.Invariant));

            var languageVariantFields = language.Fields.Select(field => new FieldData(
                field.Id,
                identifier.Id,
                field.Value,
                VarianceInfo.LanguageVariant(language.Language)));

            var variantFields = version.Fields.Select(field => new FieldData(
                field.Id,
                identifier.Id,
                field.Value,
                VarianceInfo.Variant(language.Language, version.Number)));

            return invariantFields.Union(languageVariantFields).Union(variantFields);
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}