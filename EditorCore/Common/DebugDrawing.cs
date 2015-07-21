using OpenTK;
using System.Collections.Generic;
using WEditor.Rendering;
using WEditor.Rendering.Gizmos;

namespace WEditor
{
    public class DebugDrawing
    {
        public sealed class DrawInstance
        {
            public Mesh Mesh;
            public Vector3 Position;
            public Vector3 Scale;
            public Color Color;
            public bool DepthTest;
        }

        private sealed class LineInstance
        {
            public Vector3 PosA;
            public Vector3 PosB;
            public Color Color;
        }

        private Mesh m_wireCubeMesh;
        private Mesh m_solidCubeMesh;
        private Mesh m_wireLineMesh;
        private List<LineInstance> m_wireLinePointList;
        private List<DrawInstance> m_instanceList;

        public DebugDrawing()
        {
            m_instanceList = new List<DrawInstance>(500);
            m_wireLinePointList = new List<LineInstance>(500);
        }

        public void InitializeSystem()
        {
            m_wireCubeMesh = WireCube.GenerateMesh();
            m_solidCubeMesh = SolidCube.GenerateMesh();
            m_wireLineMesh = new Mesh();
            m_wireLineMesh.SubMeshes.Add(new MeshBatch());
            m_wireLineMesh.SubMeshes[0].PrimitveType = OpenTK.Graphics.OpenGL.PrimitiveType.Lines;
        }

        public void ShutdownSystem()
        {
            m_wireCubeMesh.Dispose();
            m_wireCubeMesh = null;
            m_solidCubeMesh.Dispose();
            m_solidCubeMesh = null;
            m_wireLineMesh.Dispose();
            m_wireLineMesh = null;

            m_instanceList.Clear();
            m_instanceList = null;
        }

        public void DrawWireCube(Vector3 position, Vector3 scale, bool depthTest = true)
        {
            DrawWireCube(position, scale, new Color(1f, 1f, 1f, 1f), depthTest);
        }

        public void DrawWireCube(Vector3 position, Vector3 scale, Color color, bool depthTest = true)
        {
            m_instanceList.Add(new DrawInstance { Mesh = m_wireCubeMesh, Position = position, Scale = scale, Color = color, DepthTest = depthTest });
        }

        public void DrawCube(Vector3 position, Vector3 scale, bool depthTest = true)
        {
            DrawCube(position, scale, new Color(1f, 1f, 1f, 1f), depthTest);
        }

        public void DrawCube(Vector3 position, Vector3 scale, Color color, bool depthTest = true)
        {
            m_instanceList.Add(new DrawInstance { Mesh = m_solidCubeMesh, Position = position, Scale = scale, Color = color, DepthTest = depthTest });
        }

        public void DrawLine(Vector3 posA, Vector3 posB, Color color)
        {
            m_wireLinePointList.Add(new LineInstance { PosA = posA, PosB = posB, Color = color });
        }

        public void ResetList()
        {
            m_instanceList.Clear();
            m_wireLinePointList.Clear();
        }

        public List<DrawInstance> GetInstanceList()
        {
            return m_instanceList;
        }

        public void FinalizePrimitiveBatch()
        {
            Vector3[] vertList = new Vector3[m_wireLinePointList.Count * 2];
            Color[] colorList = new Color[m_wireLinePointList.Count * 2];
            int[] indexList = new int[m_wireLinePointList.Count * 2];

            for (int i = 0; i < m_wireLinePointList.Count; i++)
            {
                vertList[(i*2) + 0] = m_wireLinePointList[i].PosA;
                vertList[(i*2) + 1] = m_wireLinePointList[i].PosB;
                colorList[(i * 2) + 0] = m_wireLinePointList[i].Color;
                colorList[(i * 2) + 1] = m_wireLinePointList[i].Color;
                indexList[(i * 2) + 0] = (i * 2) + 0;
                indexList[(i * 2) + 1] = (i * 2) + 1;
            }

            m_wireLineMesh.SubMeshes[0].Vertices = vertList;
            m_wireLineMesh.SubMeshes[0].Color0 = colorList;
            m_wireLineMesh.SubMeshes[0].Indexes = indexList;

            // Add it to the instance list so that it gets drawn line a normal instance, it jsut happens to have a sort of unique mesh.
            m_instanceList.Add(new DrawInstance { Mesh = m_wireLineMesh, Position = Vector3.Zero, Scale = Vector3.One, Color = new Color(1f, 1f, 1f, 1f), DepthTest = true });
        }
    }
}
