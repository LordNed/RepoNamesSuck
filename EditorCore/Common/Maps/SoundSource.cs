using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class SoundSource : SceneComponent
    {
        public string Name;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 10f, Color.Pink);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 10f, Color.Pink);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 10f, Color.Red);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
