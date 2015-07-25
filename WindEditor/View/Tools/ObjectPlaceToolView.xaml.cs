using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace WEditor.UI.View
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
    }
}
