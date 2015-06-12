using EditorCore.WindWaker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace WPF_Interface
{
    public class ProjectViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<ZArchive> combinedList = new List<ZArchive>();

            for (int i = 0; i < values.Length; i++)
            {
                if(values[i] is ZArchive)
                    combinedList.Add((ZArchive)values[i]);
                else
                    combinedList.AddRange((BindingList<ZArchive>)values[i]);
            }

                return combinedList;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
 	        throw new System.NotImplementedException();
        }
    }
}
