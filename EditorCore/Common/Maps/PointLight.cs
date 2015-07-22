using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class PointLight : SceneComponent
    {
        public Vector3 Radius;
        public Color Color;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 40f, Color.Yellow);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 40f, Color.Yellow);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 40f, Color.White);
        }
    }
}
