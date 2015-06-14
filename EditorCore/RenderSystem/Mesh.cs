using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace WEditor.Rendering
{
    public class Mesh
    {
        public Vector3[] Vertices
        {
            get { return m_vertices; }
            set
            {
                m_vertices = value;
                GL.GenBuffers(1, out VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_vertices.Length * 4 * 3), m_vertices, BufferUsageHint.StaticDraw);
            }
        }
        public int[] Indexes
        {
            get { return m_indexes; }
            set
            {
                m_indexes = value;
                GL.GenBuffers(1, out EBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(m_indexes.Length * 4 * 3), m_indexes, BufferUsageHint.StaticDraw);
            }
        }
        public Color[] Color0;

        public VertexDescription VertexDescription;

        private Vector3[] m_vertices;
        private int[] m_indexes;
        private Color[] m_color0;

        public int VBO;
        public int EBO;

        public Mesh()
        {
            //VertexDescription = new VertexDescription(Rendering.VertexDescription.VertexTypes.Position | Rendering.VertexDescription.VertexTypes.Color0);

        }


    }
}
