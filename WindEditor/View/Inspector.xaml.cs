using System;
using System.Windows.Controls;
using WindEditor.UI.ViewModel;

namespace WindEditor.UI.View
{
    /// <summary>
    /// Interaction logic for Inspector.xaml
    /// </summary>
    public partial class Inspector : UserControl
    {
        public Inspector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Block the user from opening the Layer dropdown if there's no entity selected, or if that
        /// entity doesn't support having its layer changed.
        /// </summary>
        private void LayerCB_DropDownOpened(object sender, EventArgs e)
        {
            InspectorViewModel vm = (InspectorViewModel)DataContext;
            if(vm.SelectedEntity == null || !vm.SelectedEntity.LayerCanChange)
            {
                ComboBox cb = sender as ComboBox;
                cb.IsDropDownOpen = false;
            }
        }
    }
}
