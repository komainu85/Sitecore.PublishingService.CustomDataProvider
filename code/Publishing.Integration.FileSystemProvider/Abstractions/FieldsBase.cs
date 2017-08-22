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

        public List<FieldModel> Fields { get; set; }

        public FieldModel GetField(string fieldId)
        {
            return Fields.FirstOrDefault(f => f.Id == fieldId);
        }

        public void RemoveField(string fieldId)
        {
            var field = GetField(fieldId);
            Fields.Remove(field);
        }

        public FieldModel GetOrCreateField(string fieldId)
        {
            var field = GetField(fieldId);
            if (field == null)
            {
                field = new FieldModel { Id = fieldId };
                Fields.Add(field);
            }

            return field;

        }
    }
}
