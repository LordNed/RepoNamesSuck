using System;
using System.ComponentModel;

namespace WEditor
{
    public enum PropertyType
    {
        None,
        Byte,
        Short,
        Int32,
        Float,
        Bool,
        String,
        Vector2,
        Vector3,
        Enum,
        ObjectReference,
        XYRotation,
        XYZRotation,
        Color24,
        Color32,
        Vector3Byte
    }

    public class MapEntityObject
    {
        public class Property
        {
            public string Name { get; private set; }
            public object Value { get; set; }
            public PropertyType Type { get; private set; }

            public Property(string name, PropertyType type, object defaultValue = null)
            {
                Name = name;
                Type = type;
                Value = defaultValue;
            }

            public override string ToString()
            {
                return string.Format("{0} [{1}]", Name, Type);
            }
        }

        public string FourCC { get; private set; }
        public BindingList<Property> Properties { get; private set; }

        public MapEntityObject(string fourCC)
        {
            FourCC = fourCC;
            Properties = new BindingList<Property>();
        }

        public override string ToString()
        {
            return FourCC;
        }

        public Property this[string val]
        {
            get
            {
                Property prop = null;
                for (int i = 0; i < Properties.Count; i++)
                {
                    if (string.Compare(val, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        prop = Properties[i];
                        break;
                    }
                }

                return prop;
            }
            set
            {
                Property prop = null;
                for (int i = 0; i < Properties.Count; i++)
                {
                    if (string.Compare(val, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        prop = Properties[i];
                        break;
                    }
                }

                if (prop != null)
                    prop.Value = value;
                else
                    Console.WriteLine("Unsupported property {0}", val);
            }
        }

    }
}
