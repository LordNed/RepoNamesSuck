namespace WEditor
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
            return string.Format("(x: {0} y: {1} width: {2} height: {3})", X, Y, Width, Height);
        }
    }
}
