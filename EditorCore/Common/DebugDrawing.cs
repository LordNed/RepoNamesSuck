using OpenTK;
using System.Collections.Generic;
using WEditor.Rendering;
using WEditor.Rendering.Gizmos;

namespace WEditor
{
    public class DebugDrawing
    {
        public class DrawInstance
        {
            public Mesh Mesh;
            public Vector3 Position;
            public Vector3 Scale;
            public Color Color;
            public bool DepthTest;
        }

        private Mesh m_wireCubeMesh;
        private Mesh m_solidCubeMesh;
        private List<DrawInstance> m_instanceList;

        public DebugDrawing()
        {
            m_instanceList = new List<DrawInstance>(500);
        }

        public void InitializeSystem()
        {
            m_wireCubeMesh = WireCube.GenerateMesh();
            m_solidCubeMesh = SolidCube.GenerateMesh();
        }

        public void ShutdownSystem()
        {
            m_wireCubeMesh.Dispose();
            m_wireCubeMesh = null;
            m_solidCubeMesh.Dispose();
            m_solidCubeMesh = null;

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

        public void ResetList()
        {
            m_instanceList.Clear();
        }

        public List<DrawInstance> GetInstanceList()
        {
            return m_instanceList;
        }
    }
}
