using Publishing.Integration.FileSystemProvider.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class ItemModel: FieldsBase
    {
        public ItemModel()
        {
            Languages = new List<LanguageModel>();
        }

        public Guid ModelId { get; set; }
        
        public Guid TemplateId { get; set; }

        public string ItemName { get; set; }

        public List<LanguageModel> Languages { get; set; }

        public LanguageModel GetLanguage(string language)
        {
            return Languages.FirstOrDefault(l => l.Language == language);
        }        
    }
}
