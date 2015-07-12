using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.WindWaker
{
    public class Room : Scene
    {
        // Information from the MULT chunk
        public Vector3 Translation;
        public float YRotation;
        public byte MULT_Unknown1;

        public override string ToString()
        {
            return Name;
        }
    }
}
