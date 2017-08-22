using Publishing.Integration.FileSystemProvider.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class LanguageModel : FieldsBase
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
    }
}
