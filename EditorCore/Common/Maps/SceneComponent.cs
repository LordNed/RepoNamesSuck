
using OpenTK;
namespace WEditor.Maps
{
    /// <summary>
    /// Something that has a physical prescence in a scene (Position, Rotation). Does not necessairly have a renderable model.
    /// </summary>
    public class SceneComponent : MapEntity
    {
        public Transform Transform
        {
            get { return m_transform; }
            set
            {
                m_transform = value;
                OnPropertyChanged("Transform");
            }
        }

        private Transform m_transform;

        public SceneComponent()
        {
            Transform = new Transform();
        }

        public override void OnDrawGizmos()
        {
            World.Gizmos.DrawWireCube(Transform.Position, Vector3.One * 50f);
        }

        public virtual void GetAABB(out Vector3 aabbMin, out Vector3 aabbMax)
        {
            aabbMin = new Vector3(-25f, -25f, -25f);
            aabbMax = new Vector3(25f, 25f, 25f);

            aabbMin += Transform.Position;
            aabbMax += Transform.Position;
        }
    }
}
