using OpenTK;

namespace WEditor
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction.Normalized();
        }

        public override string ToString()
        {
            return string.Format("Ray (origin: {0:F3}, {1:F3}, {2:F3}, dir: {3:F3}, {4:F3}, {5:F3})", Origin.X, Origin.Y, Origin.Z, Direction.X, Direction.Y, Direction.Z);
        }
    }
}
