
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
    }
}
