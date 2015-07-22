using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class PlayerSpawn : SceneComponent
    {
        public string Name { get; set; }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 60f, Color.Seagreen);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 60f, Color.Seagreen);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 60f, Color.Red);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
