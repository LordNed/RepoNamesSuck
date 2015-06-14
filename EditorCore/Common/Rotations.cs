using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Common
{
    public class XYRotation
    {
        public float X;
        public float Y;

        public XYRotation(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("XYRotation ({0}, {1})", X.ToString("n3"), Y.ToString("n3"));
        }
    }

    public class XYZRotation
    {
        public float X;
        public float Y;
        public float Z;

        public XYZRotation(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return string.Format("XYZRotation ({0}, {1}, {2})", X.ToString("n3"), Y.ToString("n3"), Z.ToString("n3"));
        }
    }
}
