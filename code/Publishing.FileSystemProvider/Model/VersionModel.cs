using Publishing.Integration.FileSystemProvider.Abstractions;
using System;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class VersionModel: FieldsBase
    {
        public int Number { get; set; }
    }
}
