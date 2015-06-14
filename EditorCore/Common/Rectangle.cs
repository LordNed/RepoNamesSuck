using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.Common
{
    public struct Rect
    {
        public float Width;
        public float Height;
        public float X;
        public float Y;

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return string.Format("(x: {1} y: {2} width: {3} height: {4})", X, Y, Width, Height);
        }
    }
}
