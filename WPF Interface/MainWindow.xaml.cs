using EditorCore.WindWaker;
using EditorCore.WindWaker.Loaders;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

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

        private void OnProjectViewMousesMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            ZArchive archive = (ZArchive)sender;

            // No Drag and Drop for stages, they can't be assigned anywhere.
            if (archive.Type == ArchiveType.Stage)
                return;

            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                // Package the data to be stored for Drag and Drop
                DataObject data = new DataObject();
                data.SetData("Object", this);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }
    }
}
