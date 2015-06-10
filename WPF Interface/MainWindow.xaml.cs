using EditorCore.WindWaker;
using EditorCore.WindWaker.Loaders;
using System.ComponentModel;
using System.Windows;

namespace WPF_Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Map Map
        {
            get { return m_map; }
            set
            {
                m_map = value;
                OnPropertyChanged("Map");
            }
        }
        private Map m_map;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Map = MapLoader.Load(@"C:\Users\Matt\Documents\Wind Editor\ma2room");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
