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
        FixedLengthString,
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
        Int32BitField,
        Quaternion,
        ObjectReferenceShort,
        ObjectReferenceArray,
        YRotation,
        Bits
    }

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

    public class PropertyCollection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Human readable name display name which describes this collection. </summary>
        public string DisplayName
        {
            get { return m_displayName; }
            set
            {
                m_displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        /// <summary> A collection of fields for this collection which store their type and value. </summary>
        public BindingList<Property> Properties
        {
            get { return m_properties; }
            private set
            {
                m_properties = value;
                OnPropertyChanged("Properties");
            }
        }

        private string m_displayName;
        private BindingList<Property> m_properties;

        public PropertyCollection()
        {
            Properties = new BindingList<Property>();
            DisplayName = string.Empty;
        }

        public T GetProperty<T>(string propertyName)
        {
            Property prop = null;
            for (int i = 0; i < Properties.Count; i++)
            {
                if (string.Compare(propertyName, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    prop = Properties[i];
                    break;
                }
            }

            if (prop == null)
            {
                WLog.Warning(LogCategory.EditorCore, this, "Requested Property {0} on object {1}, but no property found!", propertyName, this);
                return default(T);
            }

            if (prop.Value == null)
                return default(T);

            return (T)prop.Value;
        }

        public void SetProperty(string propertyName, object value)
        {
            Property prop = null;
            for (int i = 0; i < Properties.Count; i++)
            {
                if (string.Compare(propertyName, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    prop = Properties[i];
                    break;
                }
            }

            if (prop != null)
            {
                prop.Value = value;
                return;
            }

            WLog.Warning(LogCategory.EditorCore, this, "Tried to set Property {0} on object {1}, but no property found!", propertyName, this);
        }

        public void RemoveProperty(string propertyName)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                if (string.Compare(propertyName, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Properties.RemoveAt(i);
                    return;
                }
            }

            WLog.Warning(LogCategory.EditorCore, this, "Tried to remove Property {0} on object {1}, but no property found!", propertyName, this);
        }

        public bool HasProperty(string propertyName)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                if (string.Compare(propertyName, Properties[i].Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return DisplayName;
        }

    }
}
