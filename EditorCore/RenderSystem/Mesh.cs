using SharpGL;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Runtime.InteropServices;

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
                GCHandle pinnedArray = GCHandle.Alloc(m_vertices, GCHandleType.Pinned);
                VertexDescription.UploadData(VertexDescription.VertexTypes.Position, 12, m_vertices.Length, pinnedArray.AddrOfPinnedObject());
                pinnedArray.Free();
            }
        }
        public int[] Indexes
        {
            get { return m_indexes; }
            set
            {
                m_indexes = value;
                GCHandle pinnedArray = GCHandle.Alloc(m_indexes, GCHandleType.Pinned);
                VertexDescription.UploadData(VertexDescription.VertexTypes.Indexes, 12, m_vertices.Length, pinnedArray.AddrOfPinnedObject());
                pinnedArray.Free();
            }
        }
        public Color[] Color0;

        public VertexDescription VertexDescription;

        private Vector3[] m_vertices;
        private int[] m_indexes;
        private Color[] m_color0;


        public Mesh(OpenGL context)
        {
            VertexDescription = new VertexDescription(context, Rendering.VertexDescription.VertexTypes.Position | Rendering.VertexDescription.VertexTypes.Color0);
        }


    }
}
