using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace WPF_Interface
{
    public partial class EntityPropertyTemplates : ResourceDictionary
    {
        void OnObjectFieldDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if(e.Data.GetDataPresent("objectReferenceFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }

            /*AutoSelectTextBox textBox = sender as AutoSelectTextBox;
            Window window = textBox.Tag as Window;
            if(window != null)
            {
                Console.WriteLine("Dropped");
            }*/
        }
    }
}
