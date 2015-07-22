using OpenTK;

namespace WEditor.Maps
{
    public class Arrow : SceneComponent
    {
        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 30f, Color.Red);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 30f, new Color(0.6f, 0, 0, 1));
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 30f, new Color(0.6f, 0, 0, 1));
        }
    }
}
