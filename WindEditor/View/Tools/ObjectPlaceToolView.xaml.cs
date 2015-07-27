using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace WindEditor.UI.View
{
    /// <summary>
    /// Interaction logic for ModesPlaceView.xaml
    /// </summary>
    public partial class ObjectPlaceToolView : UserControl
    {
        public ObjectPlaceToolView()
        {
            InitializeComponent();
        }

        private void tabs_Initialized(object sender, System.EventArgs e)
        {
            var control = (TabControl)sender;
            control.SelectedIndex = 0;
        }

        private void TabControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataObject data = new DataObject("actorSpawnData", ((FrameworkElement)sender).DataContext);
            DragDrop.DoDragDrop((DependencyObject)e.Source, data, DragDropEffects.Copy);
        }
    }
}
