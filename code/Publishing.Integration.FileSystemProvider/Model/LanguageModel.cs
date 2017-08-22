using Publishing.Integration.FileSystemProvider.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class LanguageModel : FieldsBase, ICloneable
    {
        public LanguageModel()
        {
            Versions = new List<VersionModel>();
        }

        public string Language { get; set; }

        public List<VersionModel> Versions { get; set; }

        public VersionModel GetVersion(int versionNumber)
        {
            return Versions.FirstOrDefault(v => v.Number == versionNumber);
        }

        public void RemoveVersion(int versionNumber)
        {
            var version = GetVersion(versionNumber);
            Versions.Remove(version);
        }

        public VersionModel GetOrCreateVersion (int versionNumber)
        {
            var version = GetVersion(versionNumber);
            if (version == null)
            {
                version = new VersionModel { Number = versionNumber };
                Versions.Add(version);
            }

            return version;
        }

        public object Clone()
        {
            return new LanguageModel
            {
                Language = this.Language,
                Versions = this.Versions.Select(v => v.Clone()).Cast<VersionModel>().ToList(),
                Fields = this.Fields.Select(f => f.Clone()).Cast<FieldModel>().ToList()
            };
        }
    }
}
