using System.Collections.Generic;
using System.ComponentModel;

namespace WEditor.Maps
{
    public class Path
    {
        public BindingList<PathPoint> Points { get; private set; }
        public short Unknown1;
        public byte Unknown2;
        public byte LoopType;
        public short Unknown3;

        public Path()
        {
            Points = new BindingList<PathPoint>();
        }
    }
}
