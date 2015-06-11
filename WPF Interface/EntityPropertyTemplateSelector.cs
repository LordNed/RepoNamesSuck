using EditorCore.Common;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Interface
{
    public class EntityPropertyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ByteTemplate { get; set; }
        public DataTemplate ShortTemplate { get; set; }
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate FloatTemplate { get; set; }
        public DataTemplate BoolTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate Vector2Template { get; set; }
        public DataTemplate Vector3Template { get; set; }
        public DataTemplate EnumTemplate { get; set; }
        public DataTemplate ObjectReferenceTemplate { get; set; }
        public DataTemplate Color24Template { get; set; }
        public DataTemplate Color32Template { get; set; }
        public DataTemplate Vector3ByteTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            MapEntityObject.Property property = (MapEntityObject.Property)item;
            switch (property.Type)
            {
                case PropertyType.Byte:
                    return ByteTemplate;
                case PropertyType.Short:
                    return ShortTemplate;
                case PropertyType.Int32:
                    return IntTemplate;
                case PropertyType.Float:
                    return FloatTemplate;
                case PropertyType.Bool:
                    return BoolTemplate;
                case PropertyType.String:
                    return StringTemplate;
                case PropertyType.Vector2:
                    return Vector2Template;
                case PropertyType.Vector3:
                    return Vector3Template;
                case PropertyType.Enum:
                    return EnumTemplate;
                case PropertyType.ObjectReference:
                    return ObjectReferenceTemplate;
                case PropertyType.XYRotation:
                    return Vector2Template;
                case PropertyType.XYZRotation:
                    return Vector3Template;
                case PropertyType.Color24:
                    return Color24Template;
                case PropertyType.Color32:
                    return Color32Template;
                case PropertyType.Vector3Byte:
                    return Vector3ByteTemplate;

                case PropertyType.None:
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}
