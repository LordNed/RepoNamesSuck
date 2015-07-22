using OpenTK;

namespace WEditor.Maps
{
    public class Actor : SceneComponent
    {
        public string Name { get; set; }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, Color.Green);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, new Color(0, 0.6f, 0, 1));
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, Color.Red);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
