using OpenTK;

namespace WEditor.Maps
{
    public class Actor : SceneComponent
    {
        public string Name;
        public int ParameterField;
        public short SetFlag;
        public short EnemyNumber;

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f);
        }
    }
}
