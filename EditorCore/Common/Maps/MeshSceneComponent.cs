using WEditor.Rendering;

namespace WEditor.Maps
{
    /// <summary>
    /// Adds a renderable mesh that has a position/rotation in the world.
    /// </summary>
    public class MeshSceneComponent : SceneComponent
    {
        public Mesh Mesh
        {
            get { return m_mesh; }
            set
            {
                m_mesh = value;
                OnPropertyChanged("Mesh");
            }
        }


        private Mesh m_mesh;
    }
}
