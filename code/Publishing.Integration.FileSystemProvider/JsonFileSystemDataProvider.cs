using Sitecore.Data.DataProviders;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.IDTables;
using Sitecore.Collections;
using Publishing.Integration.FileSystemProvider.Abstractions;
using Publishing.Integration.FileSystemProvider.Model;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace Publishing.Integration.FileSystemProvider
{
    public class JsonFileSystemDataProvider : DataProvider
    {
        private readonly IIDTable _idtable;
        private readonly IFileSystemRepository _repository;
        private readonly string _idTablePrefix;
        
        public JsonFileSystemDataProvider(string filePath, string rootItemId, string idTablePrefix)
            : this(new IDTableWrapper(), new FileSystemRepository(filePath, ".json", rootItemId), idTablePrefix)
        {
        }

        internal JsonFileSystemDataProvider(IIDTable idTable, IFileSystemRepository repository, string idTablePrefix)
        {
            Assert.ArgumentNotNull(idTable, "idTable");
            Assert.ArgumentNotNull(repository, "repository");
            Assert.ArgumentNotNullOrEmpty(idTablePrefix, "idTablePrefix");

            _idtable = idTable;
            _repository = repository;
            _idTablePrefix = idTablePrefix;                         
        }

        #region Item

        public override ItemDefinition GetItemDefinition(ID itemId, CallContext context)
        {
            Assert.ArgumentNotNull(itemId, "itemId");
            Assert.ArgumentNotNull(context, "context");

            var item = GetItem(itemId);
            if (item == null)
                return base.GetItemDefinition(itemId, context);

            context.Abort();
            return new ItemDefinition(itemId, item.ItemName, ID.Parse(item.TemplateId), ID.Null);
        }

        public override bool CreateItem(ID itemID, string itemName, ID templateID, ItemDefinition parent, CallContext context)
        {
            var parentKey = string.Empty;

            if (parent.ID.ToString() != _repository.RootId)
            {
                var parentEntry = GetStoreEntry(parent.ID);
                if (parentEntry == null)
                    return base.CreateItem(itemID, itemName, templateID, parent, context);

                parentKey = parentEntry.Key;
            }
            else
            {
                parentKey = _repository.RootId;
            }
            var modelId = CreateStoreKey(itemID, parent.ID);
            var item = new ItemModel
            {
                ModelId = modelId,
                ItemName = itemName,
                TemplateId = templateID.ToString()
            };

            context.Abort();
            return _repository.WriteChildItem(item, parentKey);
        }

        public override bool CreateItem(ID itemID, string itemName, ID templateID, ItemDefinition parent, DateTime created, CallContext context)
        {
            // TODO: honour created timestamp in model
            return CreateItem(itemID, itemName, templateID, parent, context);
        }

        public override bool DeleteItem(ItemDefinition itemDefinition, CallContext context)
        {
            var entry = GetStoreEntry(itemDefinition.ID);
            if (entry == null)
                return base.DeleteItem(itemDefinition, context);

            context.Abort();
            var result = _repository.RemoveItem(entry.Key);
            if (result)
            {
                RemoveStoreKey(entry.Key);
            }

            return result;
        }

        public override bool MoveItem(ItemDefinition itemDefinition, ItemDefinition destination, CallContext context)
        {
            var entry = GetStoreEntry(itemDefinition.ID);
            if (entry == null)
                return base.MoveItem(itemDefinition, destination, context);

            var parentKey = string.Empty;

            if (destination.ID.ToString() == _repository.RootId)
            {
                parentKey = _repository.RootId;
            }
            else
            {
                var parentEntry = GetStoreEntry(destination.ID);
                if (parentEntry == null)
                    return base.MoveItem(itemDefinition, destination, context);

                parentKey = parentEntry.Key;
            }

            context.Abort();
            MoveStoreKey(entry.Key, destination.ID);
            return _repository.MoveItem(entry.Key, parentKey);

        }

        public override bool SaveItem(ItemDefinition itemDefinition, ItemChanges changes, CallContext context)
        {
            var item = GetItem(itemDefinition.ID);
            if (item == null)
                return base.SaveItem(itemDefinition, changes, context);

            foreach (FieldChange change in changes.FieldChanges)
            {
                FieldModel field = null;
                if (change.IsShared)
                {
                    field = item.GetOrCreateField(change.FieldID.ToString());
                }
                else
                {
                    var language = item.GetOrCreateLanguage(change.Language.ToString());
                    if (change.IsUnversioned)
                    {
                        field = language.GetOrCreateField(change.FieldID.ToString());
                    }
                    else
                    {
                        var version = language.GetOrCreateVersion(change.Version.Number);
                        field = version.GetOrCreateField(change.FieldID.ToString());
                    }
                }
                field.Value = change.Value;
            }

            foreach (var change in changes.Properties)
            {
                if (change.Key.ToLowerInvariant() == "name")
                {
                    item.ItemName = change.Value.Value.ToString();
                }
            }

            context.Abort();
            return _repository.WriteItem(item);
        }

        public override bool CopyItem(ItemDefinition source, ItemDefinition destination, string copyName, ID copyID, CallContext context)
        {
            var item = GetItem(source.ID);
            if (item == null)
                return base.CopyItem(source, destination, copyName, copyID, context);

            item.ModelId = CreateStoreKey(copyID, destination.ID);
            var parent = GetStoreEntry(destination.ID);

            context.Abort();
            return _repository.WriteChildItem(item, parent.Key);
        }
        
        public override FieldList GetItemFields(ItemDefinition itemDefinition, VersionUri versionUri, CallContext context)
        {
            var item = GetItem(itemDefinition.ID);
            if (item == null)
                return base.GetItemFields(itemDefinition, versionUri, context);

            var template = TemplateManager.GetTemplate(item.TemplateId, Factory.GetDatabase("master"));
            var fields = template.GetFields();

            var resultFields = new FieldList();
            var itemFields = new List<FieldModel>();

            // shared fields
            itemFields.AddRange(item.Fields);

            var language = item.GetLanguage(versionUri.Language.ToString());
            if (language != null)
            {
                itemFields.AddRange(language.Fields);
                var version = language.GetVersion(versionUri.Version.Number);
                if (version != null)
                    itemFields.AddRange(version.Fields);
            }


            foreach (var field in fields)
            {
                var itemField = itemFields.FirstOrDefault(f => f.Id == field.ID.ToString());
                if (itemField != null)
                {
                    resultFields.Add(field.ID, itemField.Value);
                }
                else
                {
                    resultFields.Add(field.ID, field.DefaultValue);
                }
            }

            context.Abort();
            return resultFields;
        }

        #endregion

        #region Language

        public override LanguageCollection GetLanguages(CallContext context)
        {
            return null;
        }

        public override void RemoveLanguageData(Language language, CallContext context)
        {
            //TODO: strip from all items?
        }
        
        #endregion

        #region Version

        public override int AddVersion(ItemDefinition itemDefinition, VersionUri baseVersion, CallContext context)
        {
            var item = GetItem(itemDefinition.ID);
            if (item == null)
                return base.AddVersion(itemDefinition, baseVersion, context);

            var language = item.GetOrCreateLanguage(baseVersion.Language.ToString());

            if (baseVersion.Version.Number > 0)
            {
                var current = language.GetOrCreateVersion(baseVersion.Version.Number);
                var clone = current.Clone() as VersionModel;
                clone.Number++;
                language.Versions.Add(clone);
            }
            else
            {
                language.GetOrCreateVersion(1);
            }

            context.Abort();
            return _repository.WriteItem(item) ? 1 : -1;
        }

        public override VersionUriList GetItemVersions(ItemDefinition itemDefinition, CallContext context)
        {
            var item = GetItem(itemDefinition.ID);
            if (item != null)
            {
                var versions = new VersionUriList();
                foreach (var entry in item.Languages)
                {
                    var versionList = new VersionList();
                    foreach (var v in entry.Versions)
                    {
                        versions.Add(new VersionUri(Language.Parse(entry.Language), Sitecore.Data.Version.Parse(v.Number)));
                    }
                }

                context.Abort();
                return versions;

            }
            return base.GetItemVersions(itemDefinition, context);
        }
        
        public override bool RemoveVersion(ItemDefinition itemDefinition, VersionUri version, CallContext context)
        {
            var item = GetItem(itemDefinition.ID);
            if (item == null)
                return base.RemoveVersion(itemDefinition, version, context);

            context.Abort();

            var language = item.GetLanguage(version.Language.ToString());
            if (language == null)
                return true;

            language.RemoveVersion(version.Version.Number);
            return _repository.WriteItem(item);
        }

        public override bool RemoveVersions(ItemDefinition itemDefinition, Language language, bool removeSharedData, CallContext context)
        {
            return RemoveVersions(itemDefinition, language, context);
        }

        public override bool RemoveVersions(ItemDefinition itemDefinition, Language language, CallContext context)
        {
            var item = GetItem(itemDefinition.ID);
            if (item == null)
                return base.RemoveVersions(itemDefinition, language, context);

            context.Abort();

            var languageEntry = item.GetLanguage(language.ToString());
            if (languageEntry == null)
                return true;

            languageEntry.Versions.Clear();

            return _repository.WriteItem(item);
        }

        #endregion

        #region Meta

        public override IDList GetChildIDs(ItemDefinition itemDefinition, CallContext context)
        {
            IEnumerable<FileInfo> children = null;
            if (itemDefinition.ID.ToString() == _repository.RootId)
            {
                children = _repository.GetChildren(_repository.RootId);
            }
            else
            {
                var entry = GetStoreEntry(itemDefinition.ID);
                if (entry == null)
                    return base.GetChildIDs(itemDefinition, context);

                children = _repository.GetChildren(entry.Key);
            }

            var idlist = new IDList();

            foreach (var child in children.Select(x => x.ToModel()))
            {
                var scId = GetSitecoreId(child.ModelId);
                if (scId == (ID)null)
                    scId = CreateSitecoreId(child.ModelId, itemDefinition);

                idlist.Add(scId);
            }

            context.Abort();
            return idlist;
        }

        public override ID GetParentID(ItemDefinition itemDefinition, CallContext context)
        {
            var entry = GetStoreEntry(itemDefinition.ID);
            if (entry == null)
                return base.GetParentID(itemDefinition, context);

            context.Abort();
            return entry.ParentID;
        }

        public override bool HasChildren(ItemDefinition itemDefinition, CallContext context)
        {
            var entry = GetStoreEntry(itemDefinition.ID);
            if (entry == null)
                return base.HasChildren(itemDefinition, context);

            context.Abort();
            return _repository.GetChildren(entry.Key).Count() > 0;
        }

        public override ID GetRootID(CallContext context)
        {
            return ID.Parse(_repository.RootId);
        }

        #endregion

        #region Media

        public override bool BlobStreamExists(Guid blobId, CallContext context)
        {
            var entry = _repository.GetMedia(blobId);
            return (entry == null || !entry.Exists);
        }

        public override Stream GetBlobStream(Guid blobId, CallContext context)
        {
            var stream = _repository.ReadMedia(blobId);
            if(stream == null)
                return base.GetBlobStream(blobId, context);

            context.Abort();
            return stream;
        }

        public override bool RemoveBlobStream(Guid blobId, CallContext context)
        {
            var result = _repository.RemoveMedia(blobId);
            if (result)
                context.Abort();

            return result;
        }

        public override bool SetBlobStream(Stream stream, Guid blobId, CallContext context)
        {
            var result = _repository.WriteMedia(blobId, stream);
            if (result)
                context.Abort();            

            return result;
        }

        #endregion

        #region Private

        private IDTableEntry GetStoreEntry(ID itemId)
        {
            // get the entry of the first matching entry fo the id
            return _idtable.GetKeys(_idTablePrefix, itemId).FirstOrDefault();
        }
        
        private string CreateStoreKey(ID itemId, ID parentId)
        {
            var key = Guid.NewGuid().ToString("N");         
               
            return _idtable.Add(_idTablePrefix, key, itemId, parentId).Key;
        }
        
        private ID GetSitecoreId(string key)
        {
            var entry = _idtable.GetID(_idTablePrefix, key);
            return entry == null ? null : entry.ID;
        }

        private ID CreateSitecoreId(string key, ItemDefinition parent)
        {
            return _idtable.GetNewID(_idTablePrefix, key, parent.ID).ID;
        }

        private void RemoveStoreKey(string key)
        {
            _idtable.RemoveKey(_idTablePrefix, key);
        }

        private void MoveStoreKey(string key, ID newParent)
        {
            var entry = _idtable.GetID(_idTablePrefix, key);
            _idtable.RemoveKey(_idTablePrefix, key);
            _idtable.Add(_idTablePrefix, key, entry.ID, newParent);
        }

        private ItemModel GetItem(ID itemId)
        {
            var id = GetStoreEntry(itemId);
            if (id == null)
                return null;

            return _repository.GetItem(id.Key)
                .ToModel();
        }
        
        #endregion       

    }
}
