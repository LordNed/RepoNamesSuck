using GameFormatReader.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor.WindWaker.Loaders
{
    public static class J3DLoader
    {
        public static void Load(J3DFileResource resource, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found");

            // Read the Header
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open), Endian.Big))
            {
                int magic = reader.ReadInt32(); // J3D1, J3D2, etc
                int j3dType = reader.ReadInt32(); // BMD3 (models) BDL4 (models), jpa1 (particles), bck1 (animations), etc.
                int totalFileSize = reader.ReadInt32();
                int chunkCount = reader.ReadInt32();

                // Skip over an unused tag (consistent in all files) and some padding.
                reader.ReadBytes(16);

                for(int i = 0; i < chunkCount; i++)
                {
                    long chunkStart = reader.BaseStream.Position;

                    string tagName = reader.ReadString(4);
                    int chunkSize = reader.ReadInt32();

                    switch(tagName)
                    {
                        case "VTX1":
                            LoadVTX1FromFile(resource, reader, chunkSize);
                            break;

                        case "SHP1":
                        {
                            short batchCount = reader.ReadInt16();
                            short padding = reader.ReadInt16();
                            int batchOffset = reader.ReadInt32();
                            int unknownTableOffset = reader.ReadInt32();
                            int alwaysZero = reader.ReadInt32(); // ToDo: Make sure this is actually always zero...
                            int attributeOffset = reader.ReadInt32();
                            int matrixTableOffset = reader.ReadInt32();
                            int primitiveDataOffset = reader.ReadInt32();
                            int matrixDataOffset = reader.ReadInt32();
                            int packetLocationOffset = reader.ReadInt32();

                            // Batches can have different attributes (ie: some have pos, some have normal, some have texcoords, etc.) they're split by batches,
                            // where everything in the batch uses the same set of vertex attributes. Each batch then has several packets, which are a collection
                            // of primitives.
                            for(int b = 0; b < batchCount; b++)
                            {
                                reader.BaseStream.Position = chunkStart + batchOffset;
                                long batchStart = reader.BaseStream.Position;

                                byte matrixType = reader.ReadByte();
                                byte unknown0 = reader.ReadByte();
                                ushort packetCount = reader.ReadUInt16();
                                ushort batchAttributeOffset = reader.ReadUInt16();
                                ushort firstMatrixIndex = reader.ReadUInt16();
                                ushort packetIndex = reader.ReadUInt16();
                                ushort unknownpadding = reader.ReadUInt16();

                                float unknown1 = reader.ReadSingle();
                                Vector3 boundingBoxMin = new Vector3();
                                boundingBoxMin.X = reader.ReadSingle();
                                boundingBoxMin.Y = reader.ReadSingle();
                                boundingBoxMin.Z = reader.ReadSingle();

                                Vector3 boundingBoxMax = new Vector3();
                                boundingBoxMax.X = reader.ReadSingle();
                                boundingBoxMax.Y = reader.ReadSingle();
                                boundingBoxMax.Z = reader.ReadSingle();

                                for(ushort p = 0; p < packetCount; p++)
                                {
                                    // Packet Location
                                    reader.BaseStream.Position = batchStart + packetLocationOffset;
                                    reader.BaseStream.Position += (packetIndex + p) * 0x8; // A Packet Location is 0x8 long, so we skip ahead to the right one.

                                    uint packetSize = reader.ReadUInt32();
                                    uint packetOffset = reader.ReadUInt32();

                                    uint numPrimitiveBytesRead = packetOffset;
                                    while(numPrimitiveBytesRead < packetOffset + packetSize)
                                    {
                                        // Primitives
                                        J3DFileResource.PrimitiveType type = (J3DFileResource.PrimitiveType)reader.ReadByte();
                                        ushort vertexCount = reader.ReadUInt16();

                                        // Game pads the chunks out with zeros, so this is the signal for an early break;
                                        if (type == 0)
                                            break;


                                        for(int vertIndex = 0; vertIndex < attributeCount; vertIndex++)
                                        {
                                            long curOffset = reader.BaseStream.Position;

                                            // Go read the Batch attributes.
                                            reader.BaseStream.Position = (batchStart + batchAttributeOffset) + (vertIndex * 0x8);
                                            J3DFileResource.VertexArrayType attribType = (J3DFileResource.VertexArrayType)reader.ReadInt32();
                                            J3DFileResource.VertexDataType attribDataType = (J3DFileResource.VertexDataType)reader.ReadInt32();

                                            uint index;
                                            reader.BaseStream.Position = batchStart + ????????

                                            switch (attribDataType)
                                            {
                                                case J3DFileResource.VertexDataType.Signed8:
                                                    index = reader.ReadByte();
                                                    break;
                                                case J3DFileResource.VertexDataType.Signed16:
                                                    index = (uint)reader.ReadInt16();

                                                case J3DFileResource.VertexDataType.Unsigned8:
                                                case J3DFileResource.VertexDataType.Unsigned16:
                                                case J3DFileResource.VertexDataType.Float32:
                                                    throw new Exception("Unknown attribType!");
                                            }

                                            switch (attribType)
	                                        {
                                                case J3DFileResource.VertexArrayType.Position:
                                                case J3DFileResource.VertexArrayType.Normal:
                                                case J3DFileResource.VertexArrayType.Color0:
                                                case J3DFileResource.VertexArrayType.Color1:
                                                case J3DFileResource.VertexArrayType.Tex0:
                                                case J3DFileResource.VertexArrayType.Tex1:
                                                case J3DFileResource.VertexArrayType.Tex2:
                                                case J3DFileResource.VertexArrayType.Tex3:
                                                case J3DFileResource.VertexArrayType.Tex4:
                                                case J3DFileResource.VertexArrayType.Tex5:
                                                case J3DFileResource.VertexArrayType.Tex6:
                                                case J3DFileResource.VertexArrayType.Tex7:

                                                    break;
                                                default:
                                                    Console.WriteLine("[J3DLoader] Unsupported attribType {0}", attribType);
                                                 break;
	                                                }
                                        }
                                    }
                                }
                            }


                        }
                            break;


                        case "MAT3":

                            break;


                        case "TEX1":

                            break;

                    }

                    reader.BaseStream.Position = chunkStart + chunkSize;
                }
            }
        }

        private static void LoadVTX1FromFile(J3DFileResource resource, EndianBinaryReader reader, int chunkSize)
        {
            long headerStart = reader.BaseStream.Position;
            int vertexFormatOffset = reader.ReadInt32();
            int[] vertexDataOffsets = new int[13];
            for (int k = 0; k < vertexDataOffsets.Length; k++)
                vertexDataOffsets[k] = reader.ReadInt32();

            List<J3DFileResource.VertexFormat> vertexFormats = new List<J3DFileResource.VertexFormat>();
            J3DFileResource.VertexFormat curFormat = null;
            do
            {
                curFormat = new J3DFileResource.VertexFormat();
                curFormat.ArrayType = (J3DFileResource.VertexArrayType)reader.ReadInt32();
                curFormat.ComponentCount = reader.ReadInt32();
                curFormat.DataType = (J3DFileResource.VertexDataType)reader.ReadInt32();
                curFormat.DecimalPoint = reader.ReadByte();
                reader.ReadBytes(3); // Padding
                vertexFormats.Add(curFormat);
            } while (curFormat.ArrayType != J3DFileResource.VertexArrayType.NullAttr);

            // Now that we know how the vertexes are described, we can get the various data.
            for (int k = 0; k < vertexDataOffsets.Length; k++)
            {
                if (vertexDataOffsets[k] == 0)
                    continue;

                // Get the total length of this block of data.
                int totalLength = GetVertexDataLength(vertexDataOffsets, k, (int)(headerStart + chunkSize));

                switch (k)
                {
                    // Position Data
                    case 0:
                        {
                            var vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Position);

                            // ToDo: Use the vertexFormat to see how big a vertex actually is.
                            int numVertexes = totalLength / 12;
                            List<Vector3> vertices = new List<Vector3>(numVertexes);

                            for (int v = 0; v < numVertexes; v++)
                            {
                                Vector3 vertex = new Vector3();
                                vertex.X = reader.ReadSingle();
                                vertex.Y = reader.ReadSingle();
                                vertex.Z = reader.ReadSingle();

                                vertices.Add(vertex);
                            }

                            resource.Mesh.Vertices = vertices.ToArray();
                        }
                        break;

                    // Normal Data
                    case 1:
                        break;

                    // Normal Binormal Tangent Data (presumed)
                    case 2:
                        break;

                    // Color 0 Data
                    case 3:
                        {
                            var vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Color0);

                            // ToDo: Use the vertexFormat to see how the colors are actually stored.
                            int numColors = totalLength / 4;
                            List<Color> colors = new List<Color>(numColors);

                            for (int c = 0; c < numColors; c++)
                            {
                                Color color = new Color();
                                color.R = reader.ReadByte() / 255f;
                                color.G = reader.ReadByte() / 255f;
                                color.B = reader.ReadByte() / 255f;
                                color.A = reader.ReadByte() / 255f;

                                colors.Add(color);
                            }

                            resource.Mesh.Color0 = colors.ToArray();
                        }
                        break;

                    // Color 1 Data (presumed)
                    case 4:
                        break;

                    // Tex 0 Data
                    case 5:
                        {
                            var vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex0);

                            // ToDo: Use the vertexFormat to see how the UV coords are actually stored.
                            int numTexCoords = totalLength / 4;
                            float scaleFactor = (float)Math.Pow(0.5, vertexFormat.DecimalPoint);
                            List<Vector2> texCoords = new List<Vector2>(numTexCoords);

                            for (int t = 0; t < numTexCoords; t++)
                            {
                                Vector2 texCoord = new Vector2();
                                texCoord.X = reader.ReadSingle() * scaleFactor;
                                texCoord.Y = reader.ReadSingle() * scaleFactor;

                                texCoords.Add(texCoord);
                            }

                            resource.Mesh.TexCoord0 = texCoords.ToArray();
                        }

                        break;

                    // Other Tex data
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:

                        break;
                }
            }
        }


        private static int GetVertexDataLength(int[] dataOffsets, int currentIndex, int endChunkOffset)
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
