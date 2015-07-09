using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Maps
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

    public class EntityProperty
    {
        public string Name { get; private set; }
        public object Value { get; set; }
        public PropertyType Type { get; private set; }

        public EntityProperty(string name, PropertyType type, object defaultValue = null)
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

    public class MapEntity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FourCC { get; private set; }

        public BindingList<EntityProperty> Properties
        {
            get { return m_properties; }
            set
            {
                m_properties = value;
                OnPropertyChanged("Properties");
            }
        }

        public MapLayer Layer
        {
            get { return m_mapLayer; }
            set
            {
                m_mapLayer = value;
                OnPropertyChanged("MapLayer");
            }
        }

        private MapLayer m_mapLayer;
        private BindingList<EntityProperty> m_properties;

        public MapEntity(string fourCC)
        {
            FourCC = fourCC;
            Properties = new BindingList<EntityProperty>();
        }

        public override string ToString()
        {
            return FourCC;
        }

        public EntityProperty this[string val]
        {
            get
            {
                EntityProperty prop = null;
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
                EntityProperty prop = null;
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
                    WLog.Warning(LogCategory.EntityLoading, this, "Unsupported property {0}", val);
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
