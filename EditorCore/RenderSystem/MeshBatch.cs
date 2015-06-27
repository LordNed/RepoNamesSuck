using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using WEditor.Common.Nintendo.J3D;

namespace WEditor.Rendering
{
    public class BoneWeight
    {
        public ushort[] BoneIndexes;
        public float[] BoneWeights;
    }

    public class MeshBatch
    {
        #region Properties
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

        public Vector2[] TexCoord2
        {
            get { return m_texCoord2; }
            set
            {
                m_texCoord2 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex2, m_texCoord2);
            }
        }
        public Vector2[] TexCoord3
        {
            get { return m_texCoord3; }
            set
            {
                m_texCoord3 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex3, m_texCoord3);
            }
        }
        public Vector2[] TexCoord4
        {
            get { return m_texCoord4; }
            set
            {
                m_texCoord4 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex4, m_texCoord4);
            }
        }
        public Vector2[] TexCoord5
        {
            get { return m_texCoord5; }
            set
            {
                m_texCoord5 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex5, m_texCoord5);
            }
        }
        public Vector2[] TexCoord6
        {
            get { return m_texCoord6; }
            set
            {
                m_texCoord6 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex6, m_texCoord6);
            }
        }
        public Vector2[] TexCoord7
        {
            get { return m_texCoord7; }
            set
            {
                m_texCoord7 = value;
                if (value.Length == 0)
                    return;

                UpdateAttributeAndBuffers(ShaderAttributeIds.Tex7, m_texCoord7);
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
        #endregion

        // This is all kind of hackily thrown on and I should clean it up later.
        public PrimitiveType PrimitveType = PrimitiveType.Triangles;
        public Material Material;

        // WW Specific Hack to get bone support in before refactor
        public List<ushort> drawIndexes = new List<ushort>();   // "PacketMatrixData" which I believe is an index into the DRW1 section to get more info for this Packet.

        public BoneWeight[] BoneWeights;
        // There's one List<ushort> per Packet, the list is PacketMatrixData.Count long.


        // A batch can have a PositionMatrixIndex attribute which stores an index
        // into the SHP1::MatrixTable section which is then used to index into the
        // DRW1 section to do bone skinning for meshes which have 'complex' weights.
        // Not all meshes use a PMI per vertex, I think those that don't do PMI per
        // vertex have an implicit index into the SHP1::MatrixTable based on their
        // packet which applies to all vertices. 
        public List<int> PositionMatrixIndexs;

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
        private Vector2[] m_texCoord2;
        private Vector2[] m_texCoord3;
        private Vector2[] m_texCoord4;
        private Vector2[] m_texCoord5;
        private Vector2[] m_texCoord6;
        private Vector2[] m_texCoord7;
        private int[] m_indexes;

        private int m_maxBufferCount;



        public MeshBatch()
        {
            m_vertexDescription = new VertexDescription();
            m_maxBufferCount = Enum.GetValues(typeof(ShaderAttributeIds)).Length;
            m_attributeBuffers = new int[m_maxBufferCount];
            PositionMatrixIndexs = new List<int>();

            m_vertices = new Vector3[0];
            m_normals = new Vector3[0];
            m_colors0 = new Color[0];
            m_colors1 = new Color[0];
            m_texCoord0 = new Vector2[0];
            m_texCoord1 = new Vector2[0];
            m_texCoord2 = new Vector2[0];
            m_texCoord3 = new Vector2[0];
            m_texCoord4 = new Vector2[0];
            m_texCoord5 = new Vector2[0];
            m_texCoord6 = new Vector2[0];
            m_texCoord7 = new Vector2[0];

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
