using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor.Rendering
{
    public class MeshBatch
    {
        public Vector3[] Vertices
        {
            get { return m_vertices; }
            set
            {
                m_vertices = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Position, m_vertices);
            }
        }

        public Vector3[] Normals
        {
            get { return m_normals; }
            set
            {
                m_normals = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Normal, m_normals);
            }
        }

        public Color[] Color0
        {
            get { return m_colors0; }
            set
            {
                m_colors0 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Color0, m_colors0);
            }
        }

        public Color[] Color1
        {
            get { return m_colors1; }
            set
            {
                m_colors1 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Color1, m_colors1);
            }
        }

        public Vector2[] TexCoord0
        {
            get { return m_texCoord0; }
            set
            {
                m_texCoord0 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex0, m_texCoord0);
            }
        }

        public Vector2[] TexCoord1
        {
            get { return m_texCoord1; }
            set
            {
                m_texCoord1 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex1, m_texCoord1);
            }
        }

        public int[] Indexes
        {
            get { return m_indexes; }
            set
            {
                m_indexes = value;
                if (value.Length == 0)
                    return;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_elementArrayBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(m_indexes.Length * 4), m_indexes, BufferUsageHint.StaticDraw);
            }
        }

        public Texture2D Texture;

        public PrimitiveType PrimitveType = PrimitiveType.Triangles;

        /// <summary> Indicates which vertex attributes are enabled on this mesh. </summary>
        private VertexDescription m_vertexDescription;

        /// <summary> Separate GL Buffers for each attribute. </summary>
        private int[] m_attributeBuffers;
        private int m_elementArrayBuffer;

        /// <summary> Position Vertices. </summary>
        private Vector3[] m_vertices;
        private Vector3[] m_normals;
        private Color[] m_colors0;
        private Color[] m_colors1;
        private Vector2[] m_texCoord0;
        private Vector2[] m_texCoord1;
        private int[] m_indexes;

        private int m_maxBufferCount;

        public MeshBatch()
        {
            m_vertexDescription = new VertexDescription();
            m_maxBufferCount = Enum.GetValues(typeof(ShaderAttributeIds)).Length;
            m_attributeBuffers = new int[m_maxBufferCount];

            m_vertices = new Vector3[0];
            m_normals = new Vector3[0];
            m_colors0 = new Color[0];
            m_colors1 = new Color[0];
            m_texCoord0 = new Vector2[0];
            m_texCoord1 = new Vector2[0];

            // Generate our element array buffer
            GL.GenBuffers(1, out m_elementArrayBuffer);
        }


        public void Bind()
        {
            for (int i = 0; i < m_maxBufferCount; i++)
            {
                ShaderAttributeIds attribute = (ShaderAttributeIds)i;
                if(m_vertexDescription.AttributeIsEnabled(attribute))
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, m_attributeBuffers[(int)attribute]);
                    GL.EnableVertexAttribArray((int)attribute);
                    GL.VertexAttribPointer((int)attribute, m_vertexDescription.GetAttributeSize(attribute), m_vertexDescription.GetAttributePointerType(attribute), false, m_vertexDescription.GetStride(attribute), 0);
                }
            }

            // Then Bind our Element Array Buffer so we can actually draw.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_elementArrayBuffer);
        }

        public void Unbind()
        {
            for (int i = 0; i < m_maxBufferCount; i++)
            {
                GL.DisableVertexAttribArray(i);
            }
        }

        public VertexDescription GetVertexDescription()
        {
            // This should probably return a copy
            return m_vertexDescription;
        }

        private void UpdateAttributeAndBuffers<T>(ShaderAttributeIds attribute, T[] data) where T : struct
        {
            // See if this attribute is already enabled. If it's not already enabled, we need to generate a buffer for it.
            if (!m_vertexDescription.AttributeIsEnabled(attribute))
            {
                GL.GenBuffers(1, out m_attributeBuffers[(int)attribute]);
                m_vertexDescription.EnableAttribute(attribute);
            }

            // Bind the buffer before updating the data.
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_attributeBuffers[(int)attribute]);

            // Finally, update the data.
            int stride = m_vertexDescription.GetStride(attribute);
            GL.BufferData<T>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * stride), data, BufferUsageHint.StaticDraw);
        }
    }
}
