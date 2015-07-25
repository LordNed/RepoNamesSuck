using System;
using System.Windows.Data;
using WEditor;

namespace WindEditor.UI
{
    public class Color32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color32 rawColor = (Color32)value;
            return System.Windows.Media.Color.FromArgb(rawColor.A, rawColor.R, rawColor.G, rawColor.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var wpfColor = (System.Windows.Media.Color)value;
            return new Color32(wpfColor.R, wpfColor.G, wpfColor.B, wpfColor.A);
        }
    }
}
