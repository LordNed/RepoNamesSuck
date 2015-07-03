using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindEditor.UI;

namespace WEditor.UI.View
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

        private void SceneView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((SceneViewViewModel)DataContext).OnSceneViewSelectObject(e.NewValue);
        }
    }
}
