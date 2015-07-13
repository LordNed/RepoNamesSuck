using System;
using System.ComponentModel;

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
        Vector3Byte,
        Int32BitField
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

    public class MapEntity : WObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FourCC { get; internal set; }

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

        public bool LayerCanChange { get; internal set; }

        private MapLayer m_mapLayer;
        private BindingList<EntityProperty> m_properties;

        public MapEntity()
        {
            Layer = MapLayer.Default;
            FourCC = string.Empty;
            Properties = new BindingList<EntityProperty>();
            LayerCanChange = false;
        }

        public override string ToString()
        {
            return FourCC;
        }

        public T GetProperty<T>(string propertyName)
        {
            EntityProperty prop = null;
            for (int i = 0; i < Properties.Count; i++)
            {
                if (string.Compare(propertyName, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    prop = Properties[i];
                    break;
                }
            }

            if(prop == null)
            {
                WLog.Warning(LogCategory.EditorCore, this, "Requested Property {0} on object {1}, but no property found!", propertyName, this);
                return default(T);
            }

            return (T) prop.Value;
        }

        public void SetProperty(string propertyName, object value)
        {
            EntityProperty prop = null;
            for (int i = 0; i < Properties.Count; i++)
            {
                if (string.Compare(propertyName, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    prop = Properties[i];
                    break;
                }
            }

            if(prop != null)
            {
                prop.Value = value;
                return;
            }

            WLog.Warning(LogCategory.EditorCore, this, "Tried to set Property {0} on object {1}, but no property found!", propertyName, this);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
