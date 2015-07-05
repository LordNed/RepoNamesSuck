using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using WEditor;

namespace WindEditor.UI
{
    public class Color24Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
                Color24 rawColor = (Color24)value;
                return System.Windows.Media.Color.FromRgb(rawColor.R, rawColor.G, rawColor.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var wpfColor = (System.Windows.Media.Color)value;
            return new Color24(wpfColor.R, wpfColor.G, wpfColor.B);
        }
    }
}
