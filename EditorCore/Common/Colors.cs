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
    }
}
