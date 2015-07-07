using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }

        private Mesh m_cubeMesh;
        private List<DrawInstance> m_instanceList;

        public DebugDrawing()
        {
            m_instanceList = new List<DrawInstance>(500);
        }

        public void InitializeSystem()
        {
            m_cubeMesh = WireCube.GetMesh();
        }

        public void ShutdownSystem()
        {
            m_cubeMesh.Dispose();
            m_cubeMesh = null;

            m_instanceList.Clear();
            m_instanceList = null;
        }

        public void DrawWireCube(Vector3 position, Vector3 scale)
        {
            m_instanceList.Add(new DrawInstance { Mesh = m_cubeMesh, Position = position, Scale = scale });
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
