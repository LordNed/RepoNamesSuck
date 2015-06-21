using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Rendering;

namespace WEditor.WindWaker
{
    public class J3DFileResource : BaseFileResource
    {
        public enum HierarchyDataTypes
        {
            Finish = 0x0, //Terminator
            NewNode = 0x01, //Hierarchy down (insert node), new child
            EndNode = 0x02, //Hierarchy up, close child
            Joint = 0x10, //
            Material = 0x11,
            Batch = 0x12, //ie Batch
        }

        public enum VertexArrayType
        {
            PositionMatrixIndex,
            Tex0MatrixIndex,
            Tex1MatrixIndex,
            Tex2MatrixIndex, 
            Tex3MatrixIndex,
            Tex4MatrixIndex,
            Tex5MatrixIndex, 
            Tex6MatrixIndex, 
            Tex7MatrixIndex,
            Position,
            Normal,
            Color0,
            Color1,
            Tex0, 
            Tex1, 
            Tex2, 
            Tex3, 
            Tex4, 
            Tex5, 
            Tex6, 
            Tex7,
            PositionMatrixArray, 
            NormalMatrixArray, 
            TextureMatrixArray, 
            LitMatrixArray, 
            NormalBinormalTangent,
            NullAttr = 0xFF,
        }

        public enum VertexDataType
        {
            Unsigned8 = 0x0,
            Signed8 = 0x1,
            Unsigned16 = 0x2,
            Signed16 = 0x3,
            Float32 = 0x4,

            RGB565 = 0x0,
            RGB8 = 0x1,
            RGBX8 = 0x2,
            RGBA4 = 0x3,
            RGBA6 = 0x4,
            RGBA8 = 0x5
        }

        public enum PrimitiveType
        {
            Points = 0xB8,
            Lines = 0xA8,
            LineStrip = 0xB0,
            Triangles = 0x80,
            TriangleStrip = 0x98,
            TriangleFan = 0xA0,
            Quads = 0x80,
        }

        public class VertexFormat
        {
            public VertexArrayType ArrayType;
            public int ComponentCount; // Meaning depends o
            public VertexDataType DataType; // What type of data is stored here. 
            public byte DecimalPoint; // Number of Mantissa bits for fixed point numbers (position of decimal point)
        }

        public class MaterialEntry
        {
            public byte Unknown1Index;
            public byte CullModeIndex;
            public byte NumChannelsIndex;
            public byte NumTexGensIndex;
            public byte NumTevStagesIndex;
            public byte ZCompareLocIndex;
            public byte ZModeIndex;
            public byte DitherIndex;
            public short[] MaterialColorIndex; // 2
            public short[] ChannelControlIndex; // 4 of these - Sets the lighting controls for a particular color channel.
            public short[] AmbientColorIndex; // 2 of these 'matcolor'
            public short[] LightingIndex; // 8
            public short[] TexCoordIndex; // 8
            public short[] TexCoord2Index; // 8
            public short[] TexMatrixIndex; // 10
            public short[] TexMatrix2Index; // 20
            public short[] texIndex; // 8 - Texture
            public short[] tevConstantColorIndex; // 4
            public byte[] constColorSel; // 16
            public byte[] constAlphaSel; // 16
            public short[] tevOrderIndex; // 16
            public short[] tevColorIndex; // 4
            public short[] tevStageInfoIndex; // 16 // Tev Combiner
            public short[] tevSwapModeInfoIndex; // 16
            public short[] tevSwapModeTableInfoIndex; // 16
            public short[] unknownIndices; // 12 more :s
            public short FogIndex;
            public short AlphaCompareIndex;
            public short BlendModeIndex;
            public short Unknown2Index;
        }

        public class InfoNode
        {
            public HierarchyDataTypes Type;
            public ushort Value;
        }

        public Mesh Mesh;

        public J3DFileResource(string fileName, string folderName, ZArchive parentArchive) : base (fileName, folderName, parentArchive)
        {
            Mesh = new Mesh();
        }
    }
}
