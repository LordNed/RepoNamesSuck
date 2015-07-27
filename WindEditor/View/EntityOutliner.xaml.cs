using System.Windows;
using System.Windows.Controls;
using WindEditor.UI.ViewModel;

namespace WindEditor.UI.View
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
