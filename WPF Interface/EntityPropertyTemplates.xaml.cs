using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WPF_Interface
{
    partial class EntityPropertyTemplates : ResourceDictionary
    {
        private void OnObjectReferenceButtonClicked(object sender, RoutedEventArgs e)
        {
           Controls.ObjectReferenceSelector ors = new Controls.ObjectReferenceSelector();
            Popup popup = new Popup();
            popup.Child = ors;
            popup.Placement = PlacementMode.Bottom;
            popup.StaysOpen = false;
            popup.PlacementTarget = (Button)sender;
            popup.IsOpen = true;
        }
		
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
