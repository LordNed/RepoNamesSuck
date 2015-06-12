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

namespace WPF_Interface.Controls
{
    /// <summary>
    /// Interaction logic for ObjectReferenceSelector.xaml
    /// </summary>
    public partial class ObjectReferenceSelector : UserControl
    {
        public ObjectReferenceSelector()
        {
            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(m_browseListBox.ItemsSource);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(m_browseSearchBox.Text))
                return true;
            else
            {
                // ToDo: Implement this in a bit. Requires checking for a [Name] field, or a FourCC field and returning results for both.

                //return ((item as User).Name.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
                return true;
            }
        }
    }
}
