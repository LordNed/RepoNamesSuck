using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class LightVector : SceneComponent
    {
        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 30f, Color.Yellow);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 30f, Color.Yellow);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 30f, Color.Yellow);
        }
    }
}
