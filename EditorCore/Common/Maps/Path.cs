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
            bool bLoops = Fields.GetProperty<byte>("Path Loops") == 1;

            // Draw each point
            for (int i = 0; i < Points.Count; i++)
            {
                World.Gizmos.DrawCube(Points[i].Postion, Vector3.One * 25, new Color(1f, 0.5f, 0.25f, 1f));
            }

            // Draw lines between all of the points.
            for (int i = 0; i < Points.Count - 1; i++)
            {
                World.Gizmos.DrawLine(Points[i].Postion, Points[i + 1].Postion, new Color(1f, 0.5f, 0.25f, 1f));
                if(i == Points.Count-2 && bLoops)
                {
                    World.Gizmos.DrawLine(Points[i+1].Postion, Points[0].Postion, new Color(1f, 0.5f, 0.25f, 1f));
                }
            }
        }

        public override void OnDrawGizmosSelected()
        {
            bool bLoops = Fields.GetProperty<byte>("Path Loops") == 1;

            // Draw each point
            for (int i = 0; i < Points.Count; i++)
            {
                World.Gizmos.DrawCube(Points[i].Postion, Vector3.One * 25, Color.Seagreen);
            }

            // Draw lines between all of the points.
            for (int i = 0; i < Points.Count - 1; i++)
            {
                World.Gizmos.DrawLine(Points[i].Postion, Points[i + 1].Postion, Color.Seagreen);

                if (i == Points.Count - 2 && bLoops)
                {
                    World.Gizmos.DrawLine(Points[i + 1].Postion, Points[0].Postion, Color.Seagreen);
                }
            }
        }
    }
}
