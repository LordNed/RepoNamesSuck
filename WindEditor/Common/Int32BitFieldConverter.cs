using System;
using System.Text;
using System.Windows.Data;

namespace WindEditor.UI.Converters
{
    public class Int32BitFieldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int rawValue = (int)value;
            string rawBitString = System.Convert.ToString(rawValue, 2);

            StringBuilder sb = new StringBuilder();

            // Unfortunately the rawBitString won't be padded out with zeros at the start, so we'll have an issue where
            // we'll start splitting them up offset, and end up with a tail group that doesn't have enough characters. To
            // solve this, we'll ensure the string is always 32 char in length.
            int numPaddingToAdd = 32 - rawBitString.Length;
            for(int i = 0; i < numPaddingToAdd; i++)
                rawBitString = rawBitString.Insert(0, "0");
            
            // Split it up into groups of 4.
            for (int i = 0; i < rawBitString.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    sb.Append(' ');
                sb.Append(rawBitString[i]);
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string formattedStr = (string)value;

            // Remove the spaces so it's just one long string of 1's and 0's again.
            string rawStr = formattedStr.Replace(" ", string.Empty);
            return System.Convert.ToInt32(rawStr, 2);
        }
    }
}
