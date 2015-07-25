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
            searchResults.Visibility = System.Windows.Visibility.Hidden;
        }

        private void WatermarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (WatermarkTextBox)sender;
            if(textBox.Text.Length == 0)
            {
                searchResults.Visibility = System.Windows.Visibility.Hidden;
                tabs.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                searchResults.Visibility = System.Windows.Visibility.Visible;
                tabs.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
