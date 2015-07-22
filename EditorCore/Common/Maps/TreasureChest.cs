using OpenTK;

namespace WEditor.Maps
{
    public class TreasureChest : SceneComponent
    {
        public string Name { get; set; }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, Color.Purple);
        }

        public override void OnDrawGizmosSelected()
        {
            World.Gizmos.DrawCube(Transform.Position, Vector3.One * 50f, Color.Purple);
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f, Color.White);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
