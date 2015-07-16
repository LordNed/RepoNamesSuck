using OpenTK;
using System;

namespace WEditor
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

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;

                    case 1:
                        Y = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
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

        static public implicit operator Quaternion(XYZRotation rotation)
        {
            return new Quaternion(rotation.X * (float)MathE.Deg2Rad, rotation.Y * (float)MathE.Deg2Rad, rotation.Z * (float)MathE.Deg2Rad, 0f);
        }

        public override string ToString()
        {
            return string.Format("XYZRotation ({0}, {1}, {2})", X.ToString("n3"), Y.ToString("n3"), Z.ToString("n3"));
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }
    }
}
