using Publishing.Integration.FileSystemProvider.Abstractions;
using System;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class VersionModel: FieldsBase, ICloneable
    {
        public int Number { get; set; }

        public object Clone()
        {
            return new VersionModel
            {
                Number = this.Number,
                Fields = this.Fields.Select(f => f.Clone()).Cast<FieldModel>().ToList()
            };
        }
    }
}
