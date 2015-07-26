using OpenTK;
using System;
using System.Text;
using System.Windows.Data;
using WEditor;

namespace WindEditor.UI
{
    public class QuaternionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Quaternion rawValue = (Quaternion)value;
            Vector3 eulerAngles = new Vector3();

            // Blah blah math.
            float sqx = rawValue.X * rawValue.X;
            float sqy = rawValue.Y * rawValue.Y;
            float sqz = rawValue.Z * rawValue.Z;
            float sqw = rawValue.W * rawValue.W;

            float unit = sqx + sqy + sqz + sqw;
            double test = rawValue.X * rawValue.Y + rawValue.Z * rawValue.W;

            // Singularity at North Pole
            if(test > 0.499f*unit)
            {
                eulerAngles.Y = 2 * (float)Math.Atan2((float)rawValue.X, (float)rawValue.W);
                eulerAngles.Z = (float)Math.PI / 2f;
                eulerAngles.X = 0f;
            }
            else if(test < -0.499f * unit)
            {
                // Singularity at South Pole
                eulerAngles.Y = -2 * (float)Math.Atan2((float)rawValue.X, (float)rawValue.W);
                eulerAngles.Z = -(float)Math.PI / 2f;
                eulerAngles.X = 0f;
            }
            else
            {
                eulerAngles.Y = (float)Math.Atan2(2 * rawValue.Y * rawValue.W - 2 * rawValue.X * rawValue.Z, sqx - sqy - sqz + sqw);
                eulerAngles.Z = (float)Math.Asin(2 * test / unit);
                eulerAngles.X = (float)Math.Atan2(2 * rawValue.X * rawValue.W - 2 * rawValue.Y * rawValue.Z, -sqx + sqy - sqz + sqw);
            }

            return eulerAngles;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Vector3 eulerAngles = (Vector3)value;

            Quaternion xAxis = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), eulerAngles.X * MathE.Deg2Rad);
            Quaternion yAxis = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), eulerAngles.Y * MathE.Deg2Rad);
            Quaternion zAxis = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), eulerAngles.Z * MathE.Deg2Rad);

            // Swizzling to the ZYX order seems to be the right one.
            Quaternion finalRot = zAxis * yAxis * xAxis;
            return finalRot;
        }
    }
}
