using System;
using Publishing.Integration.FileSystemProvider.Model;
using System.Collections.Generic;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Abstractions
{
    public abstract class FieldsBase
    {
        protected FieldsBase()
        {
            Fields = new List<FieldModel>();
        }

        public IReadOnlyList<FieldModel> Fields { get; set; }

        public FieldModel GetField(Guid fieldId)
        {
            return Fields.FirstOrDefault(f => f.Id == fieldId);
        }
    }
}
