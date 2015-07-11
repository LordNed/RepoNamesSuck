using GameFormatReader.Common;
using OpenTK;
using System;
using System.Collections.Generic;

namespace WEditor.WindWaker.Loaders
{
    public partial class J3DLoader
    {
        // This might be GXVertexAttribute
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

            None = 0xFF // WindEditor addition to make loading easier.
        }

        public enum VertexColorType
        {
            RGB565 = 0x0,
            RGB8 = 0x1,
            RGBX8 = 0x2,
            RGBA4 = 0x3,
            RGBA6 = 0x4,
            RGBA8 = 0x5,

            None = 0xFF // WindEditor addition to make loading easier.
        }

        public class VertexFormat
        {
            public VertexArrayType ArrayType;
            public int ComponentCount; // Meaning depends o
            public VertexDataType DataType; // What type of data is stored here. 
            public VertexColorType ColorDataType; // What kind of Color data is stored here if DataType is set to None

            public byte DecimalPoint; // Number of Mantissa bits for fixed point numbers (position of decimal point)
        }

        private MeshVertexAttributeHolder LoadVTX1FromFile(EndianBinaryReader reader, long chunkStart, int chunkSize)
        {
            MeshVertexAttributeHolder dataHolder = new MeshVertexAttributeHolder();

            //long headerStart = reader.BaseStream.Position;
            int vertexFormatOffset = reader.ReadInt32();
            int[] vertexDataOffsets = new int[13];
            for (int k = 0; k < vertexDataOffsets.Length; k++)
                vertexDataOffsets[k] = reader.ReadInt32();

            reader.BaseStream.Position = chunkStart + vertexFormatOffset;
            List<VertexFormat> vertexFormats = new List<VertexFormat>();
            VertexFormat curFormat = null;
            do
            {
                curFormat = new VertexFormat();
                curFormat.ArrayType = (VertexArrayType)reader.ReadInt32();
                curFormat.ComponentCount = reader.ReadInt32();
                curFormat.DataType = (VertexDataType)reader.ReadInt32();
                curFormat.ColorDataType = (VertexColorType)curFormat.DataType;
                curFormat.DecimalPoint = reader.ReadByte();
                reader.ReadBytes(3); // Padding
                vertexFormats.Add(curFormat);
            } while (curFormat.ArrayType != VertexArrayType.NullAttr);

            // Don't count the last vertexFormat as it's the NullAttr one.
            dataHolder.Attributes = vertexFormats.GetRange(0, vertexFormats.Count - 1);

            // Now that we know how the vertexes are described, we can get the various data.
            for (int k = 0; k < vertexDataOffsets.Length; k++)
            {
                if (vertexDataOffsets[k] == 0)
                    continue;

                // Get the total length of this block of data.
                int totalLength = GetVertexDataLength(vertexDataOffsets, k, (int)(chunkSize));
                VertexFormat vertexFormat = null;
                reader.BaseStream.Position = chunkStart + vertexDataOffsets[k];

                switch (k)
                {
                    // Position Data
                    case 0:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Position);
                        dataHolder.Position = LoadVertexAttribute<Vector3>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Position, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Normal Data
                    case 1:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Normal);
                        dataHolder.Normal = LoadVertexAttribute<Vector3>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Normal, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Normal Binormal Tangent Data (presumed)
                    case 2:
                        break;

                    // Color 0 Data
                    case 3:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Color0);
                        dataHolder.Color0 = LoadVertexAttribute<Color>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Color0, VertexDataType.None, vertexFormat.ColorDataType);
                        break;

                    // Color 1 Data (presumed)
                    case 4:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Color1);
                        dataHolder.Color1 = LoadVertexAttribute<Color>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Color1, VertexDataType.None, vertexFormat.ColorDataType);
                        break;

                    // Tex 0 Data
                    case 5:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex0);
                        dataHolder.Tex0 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex0, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 1 Data
                    case 6:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex1);
                        dataHolder.Tex1 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex1, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 2 Data
                    case 7:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex2);
                        dataHolder.Tex2 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex2, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 3 Data
                    case 8:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex3);
                        dataHolder.Tex3 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex3, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 4 Data
                    case 9:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex4);
                        dataHolder.Tex4 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex4, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 5 Data
                    case 10:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex5);
                        dataHolder.Tex5 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex5, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 6 Data
                    case 11:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex6);
                        dataHolder.Tex6 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex6, vertexFormat.DataType, VertexColorType.None);
                        break;

                    // Tex 7 Data
                    case 12:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == VertexArrayType.Tex7);
                        dataHolder.Tex7 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, VertexArrayType.Tex7, vertexFormat.DataType, VertexColorType.None);
                        break;
                }
            }

            return dataHolder;
        }

        private List<T> LoadVertexAttribute<T>(EndianBinaryReader reader, int totalAttributeDataLength, byte decimalPoint, VertexArrayType arrayType, VertexDataType dataType, VertexColorType colorType) where T : new()
        {
            int componentCount = 0;
            switch (arrayType)
            {
                case VertexArrayType.Position:
                case VertexArrayType.Normal:
                    componentCount = 3;
                    break;
                case VertexArrayType.Color0:
                case VertexArrayType.Color1:
                    componentCount = 4;
                    break;
                case VertexArrayType.Tex0:
                case VertexArrayType.Tex1:
                case VertexArrayType.Tex2:
                case VertexArrayType.Tex3:
                case VertexArrayType.Tex4:
                case VertexArrayType.Tex5:
                case VertexArrayType.Tex6:
                case VertexArrayType.Tex7:
                    componentCount = 2;
                    break;
                default:
                    WLog.Warning(LogCategory.ModelLoading, null, "Unsupported ArrayType \"{0}\" found while loading VTX1!", arrayType);
                    break;
            }


            // We need to know the length of each 'vertex' (which can vary based on how many attributes and what types there are)
            int vertexSize = 0;
            switch (dataType)
            {
                case VertexDataType.Float32:
                    vertexSize = componentCount * 4;
                    break;

                case VertexDataType.Unsigned16:
                case VertexDataType.Signed16:
                    vertexSize = componentCount * 2;
                    break;

                case VertexDataType.Signed8:
                case VertexDataType.Unsigned8:
                    vertexSize = componentCount * 1;
                    break;

                case VertexDataType.None:
                    break;

                default:
                    WLog.Warning(LogCategory.ModelLoading, null, "Unsupported DataType \"{0}\" found while loading VTX1!", dataType);
                    break;
            }

            switch (colorType)
            {
                case VertexColorType.RGB8:
                    vertexSize = 3;
                    break;
                case VertexColorType.RGBX8:
                case VertexColorType.RGBA8:
                    vertexSize = 4;
                    break;

                case VertexColorType.None:
                    break;

                case VertexColorType.RGB565:
                case VertexColorType.RGBA4:
                case VertexColorType.RGBA6:
                default:
                    WLog.Warning(LogCategory.ModelLoading, null, "Unsupported Color Data Type: {0}!", colorType);
                    break;
            }


            int sectionSize = totalAttributeDataLength / vertexSize;
            List<T> values = new List<T>(sectionSize);
            float scaleFactor = (float)Math.Pow(0.5, decimalPoint);

            for (int v = 0; v < sectionSize; v++)
            {
                // Create a default version of the object and then fill it up depending on our component count and its data type...
                dynamic value = new T();

                for (int i = 0; i < componentCount; i++)
                {
                    switch (dataType)
                    {
                        case VertexDataType.Float32:
                            value[i] = reader.ReadSingle() * scaleFactor;
                            break;

                        case VertexDataType.Unsigned16:
                            value[i] = (float)reader.ReadUInt16() * scaleFactor;
                            break;

                        case VertexDataType.Signed16:
                            value[i] = (float)reader.ReadInt16() * scaleFactor;
                            break;

                        case VertexDataType.Unsigned8:
                            value[i] = (float)reader.ReadByte() * scaleFactor;
                            break;

                        case VertexDataType.Signed8:
                            value[i] = (float)reader.ReadSByte() * scaleFactor;
                            break;

                        case VertexDataType.None:
                            // Let the next switch statement get it.
                            break;

                        default:
                            WLog.Warning(LogCategory.ModelLoading, null, "Unsupported Data Type: {0}!", dataType);
                            break;
                    }


                    switch (colorType)
                    {
                        case VertexColorType.RGBX8:
                        case VertexColorType.RGB8:
                        case VertexColorType.RGBA8:
                            value[i] = reader.ReadByte() / 255f;
                            break;

                        case VertexColorType.None:
                            break;

                        case VertexColorType.RGB565:
                        case VertexColorType.RGBA4:
                        case VertexColorType.RGBA6:
                        default:
                            WLog.Warning(LogCategory.ModelLoading, null, "Unsupported Color Data Type: {0}!", colorType);
                            break;
                    }
                }
                values.Add(value);
            }

            return values;
        }


        private int GetVertexDataLength(int[] dataOffsets, int currentIndex, int endChunkOffset)
        {
            int currentOffset = dataOffsets[currentIndex];

            // Find the next available offset in the array, and subtract the two offsets to get the length of the data.
            for (int i = currentIndex + 1; i < dataOffsets.Length; i++)
            {
                if (dataOffsets[i] != 0)
                {
                    return dataOffsets[i] - currentOffset;
                }
            }

            // If we didn't find a dataOffset that was valid, then we go to the end of the chunk.
            return endChunkOffset - currentOffset;
        }
    }
}
