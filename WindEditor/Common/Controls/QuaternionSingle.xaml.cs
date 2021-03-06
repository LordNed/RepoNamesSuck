﻿using OpenTK;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WEditor;

namespace WindEditor.UI.Controls
{
    /// <summary>
    /// Interaction logic for QuaternionSingle.xaml
    /// </summary>
    public partial class QuaternionSingle : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Quaternion Value
        {
            get { return (Quaternion)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Quaternion), typeof(QuaternionSingle), new PropertyMetadata(Quaternion.Identity, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            QuaternionSingle ctrl = sender as QuaternionSingle;
            if (ctrl != null)
            {
                if (ctrl.PropertyChanged == null)
                    return;

                ctrl.m_eulerAngles = ConvertQuatToVec(ctrl.Value);
                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("X"));
                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("Y"));
                ctrl.PropertyChanged(sender, new PropertyChangedEventArgs("Z"));
            }
        }

        public float X
        {
            get { return m_eulerAngles.X; }
            set
            {
                m_eulerAngles = new Vector3(value, m_eulerAngles.Y, m_eulerAngles.Z);
                Value = ConvertVecToQuat(m_eulerAngles);
                OnPropertyChanged("X");
            }
        }

        public float Y
        {
            get { return m_eulerAngles.Y; }
            set
            {
                m_eulerAngles = new Vector3(m_eulerAngles.X, value, m_eulerAngles.Z);
                Value = ConvertVecToQuat(m_eulerAngles);

                OnPropertyChanged("Y");
            }
        }

        public float Z
        {
            get { return m_eulerAngles.Z; }
            set
            {
                m_eulerAngles = new Vector3(m_eulerAngles.X, m_eulerAngles.Y, value);
                Value = ConvertVecToQuat(m_eulerAngles);

                OnPropertyChanged("Z");
            }
        }

        private Vector3 m_eulerAngles;

        public QuaternionSingle()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        private static Vector3 ConvertQuatToVec(Quaternion quat)
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

        private static Quaternion ConvertVecToQuat(Vector3 eulerAngles)
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
