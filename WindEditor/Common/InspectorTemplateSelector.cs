using System.Windows;
using System.Windows.Controls;
using WEditor;

namespace WindEditor.UI
{
    class InspectorTemplateSelector : DataTemplateSelector
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
                    //return base.SelectTemplate(item, container);
                    return ByteTemplate;
                case PropertyType.Short:
                    //return base.SelectTemplate(item, container);
                    return ShortTemplate;
                case PropertyType.Int32:
                    //return base.SelectTemplate(item, container);
                    return IntTemplate;
                case PropertyType.Float:
                    //return base.SelectTemplate(item, container);
                    return FloatTemplate;
                case PropertyType.Bool:
                    //return base.SelectTemplate(item, container);
                    return BoolTemplate;
                case PropertyType.String:
                    //return base.SelectTemplate(item, container);
                    return StringTemplate;
                case PropertyType.Vector2:
                    //return base.SelectTemplate(item, container);
                    return Vector2Template;
                case PropertyType.Vector3:
                    //return base.SelectTemplate(item, container);
                    return Vector3Template;
                case PropertyType.Enum:
                    //return base.SelectTemplate(item, container);
                    return EnumTemplate;
                case PropertyType.ObjectReference:
                    //return base.SelectTemplate(item, container);
                    return ObjectReferenceTemplate;
                case PropertyType.XYRotation:
                    //return base.SelectTemplate(item, container);
                    return Vector2Template;
                case PropertyType.XYZRotation:
                    //return base.SelectTemplate(item, container);
                    return Vector3Template;
                case PropertyType.Color24:
                    //return base.SelectTemplate(item, container);
                    return Color24Template;
                case PropertyType.Color32:
                    //return base.SelectTemplate(item, container);
                    return Color32Template;
                case PropertyType.Vector3Byte:
                    //return base.SelectTemplate(item, container);
                    return Vector3ByteTemplate;

                case PropertyType.None:
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}
