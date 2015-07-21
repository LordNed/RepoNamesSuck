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

        public override void OnDrawGizmos()
        {
            for(int i = 0; i < Points.Count-1; i++)
            {
                World.Gizmos.DrawLine(Points[i].Postion, Points[i + 1].Postion, new Color(1f, 0f, 0f, 1f));
            }
        }
    }
}
