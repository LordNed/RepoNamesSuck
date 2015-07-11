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
