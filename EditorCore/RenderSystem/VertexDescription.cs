using SharpGL;
using System;

namespace WEditor.Rendering
{
    public class VertexDescription
    {
        [Flags]
        public enum VertexTypes
        {
            Position = 1,
            Normal = 2,
            Color0 = 4,
            Color1 = 8,
            Tex0 = 16,
            Tex1 = 32,
            Indexes = 64
        }

        public VertexTypes Layout;
        public int PositionBuffer = -1;
        public int NormalBuffer = -1;
        public int Color0Buffer = -1;
        public int Color1Buffer = -1;
        public int Tex0Buffer = -1;
        public int Tex1Buffer = -1;
        public int IndexBuffer = -1;

        private OpenGL m_context;

        public VertexDescription(OpenGL context, VertexTypes layout)
        {
            Layout = layout;
            m_context = context;

            GenerateBuffers();
        }

        private void GenerateBuffers()
        {
            Array enumValues = Enum.GetValues(typeof(VertexTypes));

            // Find out how many buffers we need to generate.
            int descriptionCount = 0;
            for (int i = 0; i < enumValues.Length; i++)
            {
                if ((Layout & (VertexTypes)enumValues.GetValue(i)) != 0)
                {
                    descriptionCount++;
                }
            }

            uint[] ids = new uint[descriptionCount + 1];
            m_context.GenBuffers(1, ids);

            for (int i = 0; i < ids.Length; i++)
            {
                if ((Layout & VertexTypes.Position) != 0)
                    PositionBuffer = i;
                else if ((Layout & VertexTypes.Normal) != 0)
                    NormalBuffer = i;
                else if ((Layout & VertexTypes.Color0) != 0)
                    Color0Buffer = i;
                else if ((Layout & VertexTypes.Color1) != 0)
                    Color1Buffer = i;
                else if ((Layout & VertexTypes.Tex0) != 0)
                    Tex0Buffer = i;
                else if ((Layout & VertexTypes.Tex1) != 0)
                    Tex1Buffer = i;
                else
                    Console.WriteLine("Something went wrong generating buffers.");
            }

            // Always generate an Index Buffer cause uh... yeah.
            IndexBuffer = (int)ids[(ids.Length - 1)];
        }

        public void UploadData(VertexTypes dataType, int elementSize, int elementCount, IntPtr data)
        {
            int bufferNum;

            switch (dataType)
            {
                case VertexTypes.Position:
                    bufferNum = PositionBuffer;
                    break;
                case VertexTypes.Normal:
                    bufferNum = NormalBuffer;
                    break;
                case VertexTypes.Color0:
                    bufferNum = Color0Buffer;
                    break;
                case VertexTypes.Color1:
                    bufferNum = Color1Buffer;
                    break;
                case VertexTypes.Tex0:
                    bufferNum = Tex0Buffer;
                    break;
                case VertexTypes.Tex1:
                    bufferNum = Tex1Buffer;
                    break;
                default:
                    bufferNum = 0;
                    Console.WriteLine("[VertexDescriptor] Failed to bind buffer :(");
                    break;
            }

            if(dataType == VertexTypes.Indexes)
            {
                m_context.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, (uint)bufferNum);
                m_context.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, elementSize * elementCount, data, OpenGL.GL_STATIC_DRAW);
            }
            else
            {
                m_context.BindBuffer(OpenGL.GL_ARRAY_BUFFER, (uint)bufferNum);
                m_context.BufferData(OpenGL.GL_ARRAY_BUFFER, elementSize * elementCount, data, OpenGL.GL_STATIC_DRAW);
            }
        }
    }
}
