using System.Collections.Generic;
using System.ComponentModel;

namespace WEditor.Maps
{
    public class Path : MapEntity
    {
        public BindingList<PathPoint> Points { get; private set; }

        public Path()
        {
            Points = new BindingList<PathPoint>();
        }
    }
}
