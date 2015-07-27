using OpenTK;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WindEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for Vector2Single.xaml
    /// </summary>
    public partial class Vector2Single : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Vector2 Value
        {
            get { return (Vector2)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Vector2), typeof(Vector2Single), new PropertyMetadata(Vector2.Zero, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Vector2Single ctrl = sender as Vector2Single;
            if(ctrl != null)
            {
                if(ctrl.PropertyChanged == null)
                    return;

                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("X"));
                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("Y"));
            }
        }

        public float X
        {
            get { return ((Vector2)GetValue(ValueProperty)).X; }
            set
            {
                Value = new Vector2(value, Value.Y);
                OnPropertyChanged("X");
            }
        }

        public float Y
        {
            get { return ((Vector2)GetValue(ValueProperty)).Y; }
            set
            {
                Value = new Vector2(Value.X, value);
                OnPropertyChanged("Y");
            }
        }

        public Vector2Single()
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
