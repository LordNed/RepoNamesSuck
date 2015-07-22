using OpenTK;
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
            for (int i = 0; i < Points.Count; i++)
            {
                World.Gizmos.DrawCube(Points[i].Postion, new Vector3(10, 10, 10), new Color(1f, 0.5f, 0.25f, 1f));
            }
            for (int i = 0; i < Points.Count - 1; i++)
            {
                World.Gizmos.DrawLine(Points[i].Postion, Points[i + 1].Postion, new Color(1f, 0.5f, 0.25f, 1f));
            }
        }

        public override void OnDrawGizmosSelected()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                World.Gizmos.DrawCube(Points[i].Postion, new Vector3(10, 10, 10), Color.Seagreen);
            }
            for (int i = 0; i < Points.Count - 1; i++)
            {
                World.Gizmos.DrawLine(Points[i].Postion, Points[i + 1].Postion, Color.Seagreen);
            }
        }
    }
}
