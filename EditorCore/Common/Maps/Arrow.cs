using OpenTK;

namespace WEditor.Common.Maps
{
    public class Arrow : WObject
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public short Padding;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Position, Vector3.One * 50f);
        }
    }
}
