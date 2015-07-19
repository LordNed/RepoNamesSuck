using OpenTK;

namespace WEditor.Maps
{
    public class Door : SceneComponent
    {
        public string Name;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
