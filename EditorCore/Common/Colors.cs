using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCore.Common
{
    public class Color32
    {
        public byte R, G, B, A;

        public Color32()
        {
            R = G = B = A = 0;
        }

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

    public class Color24
    {
        public byte R, G, B;

        public Color24()
        {
            R = G = B;
        }

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
