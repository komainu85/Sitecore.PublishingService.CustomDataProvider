using Sitecore.Data;
using Sitecore.Data.IDTables;
using System;

namespace Publishing.Integration.FileSystemProvider.Abstractions
{
    public interface IIDTable
    {
        IDTableEntry Add(string prefix, string key, ID id); IDTableEntry Add(string prefix, string key, ID id, ID parentID); IDTableEntry Add(string prefix, string key, ID id, ID parentID, string customData); IDTableEntry Add(string prefix, string key, ID id, ID parentID, string customData, TimeSpan slidingExpiration);

        IDTableEntry GetID(string prefix, string key); IDTableEntry[] GetKeys(string prefix); IDTableEntry[] GetKeys(string prefix, ID id); IDTableEntry GetNewID(string prefix, string key); IDTableEntry GetNewID(string prefix, string key, ID parentID);

        IDTableEntry GetNewID(string prefix, string key, ID parentID, string customData);

        void RemoveID(string prefix, ID id);

        void RemoveKey(string prefix, string key);
    }
}
