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
    /// Interaction logic for EntityOutliner.xaml
    /// </summary>
    public partial class EntityOutliner : UserControl
    {
        public EntityOutliner()
        {
            InitializeComponent();
        }

        private void SelectedEntityChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = (EntityOutlinerViewModel)DataContext;
            if (e.AddedItems.Count > 0)
                viewModel.OnSelectedEntityChanged(e.AddedItems[0]);
            else
                viewModel.OnSelectedEntityChanged(null);
        }
    }
}
