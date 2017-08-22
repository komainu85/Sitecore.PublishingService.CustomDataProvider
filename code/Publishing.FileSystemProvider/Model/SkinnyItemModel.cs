using System;
using System.IO;
using Publishing.FileSystemProvider.Model;

namespace Publishing.FileSystemProvider
{
    public class SkinnyItemModel
    {
        public Guid ModelId { get; set; }

        public Guid ItemId { get; set; }

        public Guid ParentId { get; set; }

        public Guid TemplateId { get; set; }

        public DataProviderFile File { get; set; }
    }
}