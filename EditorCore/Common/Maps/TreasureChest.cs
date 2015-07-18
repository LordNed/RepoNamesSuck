using OpenTK;

namespace WEditor.Maps
{
    public class TreasureChest : SceneComponent
    {
        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f);
        }
    }
}
