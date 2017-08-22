using System;

namespace Publishing.FileSystemProvider.Model
{
    public class ItemIdEntity
    {
        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        public string Key { get; set; }
    }
}