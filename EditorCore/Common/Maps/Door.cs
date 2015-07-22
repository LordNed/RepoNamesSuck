using OpenTK;

namespace WEditor.Maps
{
    public class Door : SceneComponent
    {
        public string Name;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, Color.Blue);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, new Color(0, 0, 0.6f, 1));
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, new Color(0, 0, 0.6f, 1));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
