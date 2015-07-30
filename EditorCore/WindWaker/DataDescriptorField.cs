using Newtonsoft.Json;
using WEditor.Maps;

namespace WEditor.Maps
{
    public enum ReferenceTypes
    {
        Room,
        FourCC
    }

    public class DataDescriptorField
    {
        [JsonProperty("Name")]
        public string FieldName;

        [JsonProperty("Type")]
        public PropertyType FieldType;

        /// <summary> The default value of this field on a newly created object. </summary>
        public object DefaultValue;

        /// <summary> A description which can be used to describe this field's usage in the editor. </summary>
        public string Description;

        /// <summary> Used if <see cref="FieldType"/> is set to a type that supports a fixed length variable. </summary>
        public int Length;

        /// <summary> Used if <see cref=" FieldType"/> is set to Enum" </summary>
        public string EnumType;

        /// <summary> Used if Type is set to "objectReference" </summary>
        public ReferenceTypes ReferenceType;

        /// <summary> Used if ReferenceType is set to "FourCC" </summary>
        public string ReferenceFourCCType;

        public override string ToString()
        {
            return string.Format("{0} [{1}]", FieldName, FieldType);
        }
    }
}
