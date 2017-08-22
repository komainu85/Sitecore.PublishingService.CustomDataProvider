using System;
using Sitecore.Data;
using Sitecore.Data.IDTables;

namespace Publishing.Integration.FileSystemProvider.Abstractions
{
    public class IDTableWrapper : IIDTable
    {
        public IDTableEntry Add(string prefix, string key, ID id)
        {
            return IDTable.Add(prefix, key, id);
        }

        public IDTableEntry Add(string prefix, string key, ID id, ID parentID)
        {
            return IDTable.Add(prefix, key, id, parentID);
        }

        public IDTableEntry Add(string prefix, string key, ID id, ID parentID, string customData)
        {
            return IDTable.Add(prefix, key, id, parentID, customData);
        }

        public IDTableEntry Add(string prefix, string key, ID id, ID parentID, string customData, TimeSpan slidingExpiration)
        {
            return IDTable.Add(prefix, key, id, parentID, customData, slidingExpiration);
        }

        public IDTableEntry GetID(string prefix, string key)
        {
            return IDTable.GetID(prefix, key);
        }

        public IDTableEntry[] GetKeys(string prefix)
        {
            return IDTable.GetKeys(prefix);
        }

        public IDTableEntry[] GetKeys(string prefix, ID id)
        {
            return IDTable.GetKeys(prefix, id);
        }

        public IDTableEntry GetNewID(string prefix, string key)
        {
            return IDTable.GetNewID(prefix, key);
        }

        public IDTableEntry GetNewID(string prefix, string key, ID parentID)
        {
            return IDTable.GetNewID(prefix, key, parentID);
        }

        public IDTableEntry GetNewID(string prefix, string key, ID parentID, string customData)
        {
            return IDTable.GetNewID(prefix, key, parentID, customData);
        }

        public void RemoveID(string prefix, ID id)
        {
            IDTable.RemoveID(prefix, id);
        }

        public void RemoveKey(string prefix, string key)
        {
            IDTable.RemoveKey(prefix, key);
        }
    }
}
