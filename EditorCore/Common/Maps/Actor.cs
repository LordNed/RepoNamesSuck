using OpenTK;

namespace WEditor.Maps
{
    public class Actor : SceneComponent
    {
        public string Name { get; set; }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, new Color(0, 1, 0, 1));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
