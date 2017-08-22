using System;

namespace Publishing.Integration.FileSystemProvider.Model
{
    public class FieldModel: ICloneable
    {
        public string Id { get; set; }

        public string Value { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }        
    }
}
