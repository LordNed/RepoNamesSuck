using System.ComponentModel;
using System.Windows.Data;
using WEditor.Maps;

namespace WindEditor.UI
{
    public class MapEntityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(values[0] is MapEntity)
            {
                MapEntity mapEnt = values[0] as MapEntity;
                BindingList<object> allFields = new BindingList<object>();

                if(values[0] is SceneComponent)
                {
                    allFields.Add(((SceneComponent)values[0]).Transform.Position);
                    allFields.Add(((SceneComponent)values[0]).Transform.Rotation);
                    allFields.Add(((SceneComponent)values[0]).Transform.LocalScale);
                }

                foreach (var field in mapEnt.Fields.Properties)
                    allFields.Add(field);

                return allFields;
            }
            return null;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
