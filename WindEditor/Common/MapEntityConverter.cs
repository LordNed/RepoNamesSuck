using OpenTK;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Data;
using WEditor.Maps;

namespace WindEditor.UI
{
    public class MapEntityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MapEntity mapEntity = value as MapEntity;
            if (mapEntity == null)
                return null;


            // Use reflection to get the fields from the MapEntity instance. These fields will then go through a template selector
            // to generate the right controls based on their contents.
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            List<Property> entityFields = new List<Property>();
            Type entityType = mapEntity.GetType();

            FieldInfo[] allFields = entityType.GetFields(bindingFlags);
            PropertyInfo[] allProperties = entityType.GetProperties(bindingFlags);

            //foreach (var field in allFields)
            //{
            //    entityFields.Add(field.FieldType);
            //}

            foreach (var property in allProperties)
            {
                if(property.PropertyType == typeof(PropertyCollection))
                {
                    var propCollection = (PropertyCollection) property.GetValue(mapEntity);
                    entityFields.AddRange(propCollection.Properties);
                }
                else
                {
                    string fieldName = property.Name;
                    Property newProp = new Property(fieldName, SystemTypeToEnum(property.PropertyType));
                }
            }


            return entityFields;
        }

        private PropertyType SystemTypeToEnum(Type type)
        {
            if (type == typeof(byte))
                return PropertyType.Byte;

            if (type == typeof(short))
                return PropertyType.Short;

            if (type == typeof(int))
                return PropertyType.Int32;

            if (type == typeof(float))
                return PropertyType.Float;

            if (type == typeof(bool))
                return PropertyType.Bool;

            if (type == typeof(string))
                return PropertyType.String;

            if(type == typeof(Vector2))
            return PropertyType.Vector2;

            if (type == typeof(Vector3))
                return PropertyType.Vector3;

            return PropertyType.None;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
