using System.Windows.Controls;
using WindEditor.UI.ViewModel;

namespace WindEditor.UI.View
{
    /// <summary>
    /// Interaction logic for SceneView.xaml
    /// </summary>
    public partial class SceneView : UserControl
    {
        public SceneView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((SceneViewViewModel)DataContext).OnSceneViewSelectObject(((ListBox)sender).SelectedItem);
        }
    }
}
