using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class ShipSpawn : SceneComponent
    {
        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, Color.Orange);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, Color.Orange);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, Color.Red);
        }

    }
}
