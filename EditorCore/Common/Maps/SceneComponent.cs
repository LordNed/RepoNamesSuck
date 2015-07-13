
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
    }
}
