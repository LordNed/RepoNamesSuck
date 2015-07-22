using OpenTK;

namespace WEditor.Maps
{
    public class ScaleableObject : SceneComponent
    {
        public string Name { get; set; }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, Color.Grey);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, Color.DarkGrey);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, Color.Red);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
