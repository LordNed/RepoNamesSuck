using OpenTK;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using WEditor;

namespace WindEditor.UI
{
    /// <summary>
    /// Interaction logic for QuatAsVector.xaml
    /// </summary>
    public partial class QuatAsVector : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Quaternion Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                m_eulerValue = ConvertQuatToVec(m_value);

                OnPropertyChanged("Value");
                OnPropertyChanged("ControlValues");
            }
        }

        public Vector3 ControlValues
        {
            get { return m_eulerValue;}
            set
            {
                m_eulerValue = value;
                m_value = ConvertVecToQuat(m_eulerValue);

                OnPropertyChanged("Value");
                OnPropertyChanged("ControlValues");
            }
        }

        private Quaternion m_value;
        private Vector3 m_eulerValue;

        public QuatAsVector()
        {
            InitializeComponent();
        }

        private Vector3 ConvertQuatToVec(Quaternion quat)
        {
            Vector3 eulerAngles = new Vector3();

            // Blah blah math.
            float sqx = quat.X * quat.X;
            float sqy = quat.Y * quat.Y;
            float sqz = quat.Z * quat.Z;
            float sqw = quat.W * quat.W;

            float unit = sqx + sqy + sqz + sqw;
            double test = quat.X * quat.Y + quat.Z * quat.W;

            // Singularity at North Pole
            if (test > 0.499f * unit)
            {
                eulerAngles.Y = 2 * (float)Math.Atan2((float)quat.X, (float)quat.W);
                eulerAngles.Z = (float)Math.PI / 2f;
                eulerAngles.X = 0f;
            }
            else if (test < -0.499f * unit)
            {
                // Singularity at South Pole
                eulerAngles.Y = -2 * (float)Math.Atan2((float)quat.X, (float)quat.W);
                eulerAngles.Z = -(float)Math.PI / 2f;
                eulerAngles.X = 0f;
            }
            else
            {
                eulerAngles.Y = (float)Math.Atan2(2 * quat.Y * quat.W - 2 * quat.X * quat.Z, sqx - sqy - sqz + sqw);
                eulerAngles.Z = (float)Math.Asin(2 * test / unit);
                eulerAngles.X = (float)Math.Atan2(2 * quat.X * quat.W - 2 * quat.Y * quat.Z, -sqx + sqy - sqz + sqw);
            }

            return eulerAngles;
        }

        private Quaternion ConvertVecToQuat(Vector3 eulerAngles)
        {
            Quaternion xAxis = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), eulerAngles.X * MathE.Deg2Rad);
            Quaternion yAxis = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), eulerAngles.Y * MathE.Deg2Rad);
            Quaternion zAxis = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), eulerAngles.Z * MathE.Deg2Rad);

            // Swizzling to the ZYX order seems to be the right one.
            Quaternion finalRot = zAxis * yAxis * xAxis;
            return finalRot;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
