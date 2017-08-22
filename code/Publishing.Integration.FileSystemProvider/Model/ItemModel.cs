using Publishing.Integration.FileSystemProvider.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class ItemModel: FieldsBase, ICloneable
    {
        public ItemModel()
        {
            Languages = new List<LanguageModel>();
        }

        public string ModelId { get; set; }
        
        public string TemplateId { get; set; }

        public string ItemName { get; set; }

        public List<LanguageModel> Languages { get; set; }

        public LanguageModel GetLanguage(string language)
        {
            return Languages.FirstOrDefault(l => l.Language == language);
        }

        public void RemoveVersion(string language)
        {
            var lang = GetLanguage(language);
            Languages.Remove(lang);
        }

        public LanguageModel GetOrCreateLanguage(string language)
        {
            var lang = GetLanguage(language);
            if (lang == null)
            {
                lang = new LanguageModel { Language = language };
                Languages.Add(lang);
            }

            return lang;
        }

        public object Clone()
        {
            return new ItemModel
            {
                ModelId = this.ModelId,
                TemplateId = this.TemplateId,
                ItemName = this.ItemName,
                Languages = this.Languages.Select(l => l.Clone()).Cast<LanguageModel>().ToList(),
                Fields = this.Fields.Select(f => f.Clone()).Cast<FieldModel>().ToList()                
            };
        }
    }
}
