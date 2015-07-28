using System.Collections.Generic;

namespace WEditor.Maps
{
    public class MapEntityDataDescriptor
    {
        public string FourCC;
        public List<FieldParameter> Fields;

        public override string ToString()
        {
            return string.Format("FourCC: {0} Property Count: {1}", FourCC, Fields.Count);
        }
    }
}
