using OpenTK;
using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class SoundSource : SceneComponent
    {
        public string Name;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, new Vector3(10f, 10f, 10f));
        }
    }
}
