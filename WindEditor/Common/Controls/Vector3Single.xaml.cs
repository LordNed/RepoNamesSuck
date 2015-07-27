using OpenTK;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WindEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for Vector3Single.xaml
    /// </summary>
    public partial class Vector3Single : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Vector3 Value
        {
            get { return (Vector3)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Vector3), typeof(Vector3Single), new PropertyMetadata(Vector3.Zero, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Vector3Single ctrl = sender as Vector3Single;
            if(ctrl != null)
            {
                if(ctrl.PropertyChanged == null)
                    return;

                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("X"));
                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("Y"));
                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("Z"));
            }
        }

        public float X
        {
            get { return ((Vector3)GetValue(ValueProperty)).X; }
            set
            {
                Value = new Vector3(value, Value.Y, Value.Z);
                OnPropertyChanged("X");
            }
        }

        public float Y
        {
            get { return ((Vector3)GetValue(ValueProperty)).Y; }
            set
            {
                Value = new Vector3(Value.X, value, Value.Z);
                OnPropertyChanged("Y");
            }
        }

        public float Z
        {
            get { return ((Vector3)GetValue(ValueProperty)).Z; }
            set
            {
                Value = new Vector3(Value.X, Value.Y, value);
                OnPropertyChanged("Z");
            }
        }

        public Vector3Single()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
