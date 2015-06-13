using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;
using System.Windows.Controls;

namespace WPF_Interface
{
    partial class EntityPropertyTemplates : ResourceDictionary
    {
        private void OnObjectReferenceButtonClicked(object sender, RoutedEventArgs e)
        {
            Controls.ObjectReferenceSelector ors = new Controls.ObjectReferenceSelector();
            Button btn = (Button)sender;
            Grid grid = btn.Parent as Grid;
            if(grid != null)
            {
                grid.Children.Add(ors);
            }
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
