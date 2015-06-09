using Newtonsoft.Json;
using System.Collections.Generic;

namespace EditorCore.WindWaker.Templates
{
    public class ItemTemplate
    {
        [JsonProperty("FourCC")]
        public string FourCC;

        [JsonProperty("Properties")]
        public List<ItemTemplateProperty> Properties;
    }

    public class ItemTemplateProperty
    {
        public string Name;
        public string Type;

        /// <summary> Used if Type is set to "enum" </summary>
        public string EnumType;

        /// <summary> Used if Type is set to "objectReference" </summary>
        public string ReferenceType;

        /// <summary> Used if Type is set to a fixed length type variable. </summary>
        public int Length;
    }
}
