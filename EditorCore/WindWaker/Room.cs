using OpenTK;

namespace WEditor.WindWaker
{
    public class Room : Scene
    {
        // Information from the MULT chunk
        public Vector3 Translation;
        public Quaternion Rotation;

        public Room()
        {
            Translation = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }
    }
}
