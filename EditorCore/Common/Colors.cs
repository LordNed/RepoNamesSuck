
using System;
namespace WEditor
{
    /// <summary>
    /// RGBA Color that uses Floats
    /// </summary>
    public struct Color
    {
        public float R, G, B, A;

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            return string.Format("[Color] (r: {0} g: {1} b: {2} a: {3})", R, G, B, A);
        }

        public static implicit operator Color(Color32 color32)
        {
            return new Color(color32.R / 255f, color32.G / 255f, color32.B / 255f, color32.A / 255f);
        }

        public static implicit operator Color(Color24 color24)
        {
            return new Color(color24.R / 255f, color24.G / 255f, color24.B / 255f, 1f);
        }

        public static implicit operator Color32(Color color)
        {
            return new Color32((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255), (byte)(color.A * 255));
        }

        public float this[int index]
        {
            get
            {
                switch(index)
                {
                    case 0:
                        return R;
                    case 1:
                        return G;
                    case 2:
                        return B;
                    case 3:
                        return A;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch(index)
                {
                    case 0:
                        R = value;
                        break;

                    case 1:
                        G = value;
                        break;

                    case 2:
                        B = value;
                        break;

                    case 3:
                        A = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }
    }

    /// <summary>
    /// RGBA Color that uses Bytes
    /// </summary>
    public struct Color32
    {
        public byte R, G, B, A;

        public Color32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            return string.Format("[Color32] (r: {0} g: {1} b: {2} a: {3})", R, G, B, A);
        }

        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return R;
                    case 1:
                        return G;
                    case 2:
                        return B;
                    case 3:
                        return A;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        R = value;
                        break;

                    case 1:
                        G = value;
                        break;

                    case 2:
                        B = value;
                        break;

                    case 3:
                        A = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }
    }

    /// <summary>
    /// RGB Color that uses Bytes
    /// </summary>
    public struct Color24
    {
        public byte R, G, B;

        public Color24(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public override string ToString()
        {
            return string.Format("[Color24] (r: {0} g: {1} b: {2})", R, G, B);            
        }

        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return R;
                    case 1:
                        return G;
                    case 2:
                        return B;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        R = value;
                        break;

                    case 1:
                        G = value;
                        break;

                    case 2:
                        B = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("index");
                }
            }
        }
    }
}
