using WEditor.Maps;

namespace WEditor.Common.Maps
{
    public class PlayerSpawn : SceneComponent
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
