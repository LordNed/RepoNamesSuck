using GameFormatReader.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public static class J3DLoader
    {
        private class MeshVertexAttributeHolder
        {
            public List<Vector3> Position;
            public List<Vector3> Normal;
            public List<Color> Color0;
            public List<Vector2> Tex0;
            public List<int> Indexes;

            public List<J3DFileResource.VertexFormat> Attributes;

            public MeshVertexAttributeHolder()
            {
                Position = new List<Vector3>();
                Normal = new List<Vector3>();
                Color0 = new List<Color>();
                Tex0 = new List<Vector2>();
                Attributes = new List<J3DFileResource.VertexFormat>();
                Indexes = new List<int>();
            }
        }

        public static void Load(J3DFileResource resource, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found");

            MeshVertexAttributeHolder vertexData = null;
            Mesh j3dMesh = resource.Mesh;

            // Read the Header
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open), Endian.Big))
            {
                int magic = reader.ReadInt32(); // J3D1, J3D2, etc
                int j3dType = reader.ReadInt32(); // BMD3 (models) BDL4 (models), jpa1 (particles), bck1 (animations), etc.
                int totalFileSize = reader.ReadInt32();
                int chunkCount = reader.ReadInt32();

                // Skip over an unused tag (consistent in all files) and some padding.
                reader.ReadBytes(16);

                for (int i = 0; i < chunkCount; i++)
                {
                    long chunkStart = reader.BaseStream.Position;

                    string tagName = reader.ReadString(4);
                    int chunkSize = reader.ReadInt32();

                    switch (tagName)
                    {
                        case "VTX1":
                            vertexData = LoadVTX1FromFile(resource, reader, chunkSize);
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
                                for (int b = 0; b < batchCount; b++)
                                {
                                    MeshBatch meshBatch = new MeshBatch();
                                    j3dMesh.SubMeshes.Add(meshBatch);
                                    int overallVertexCount = 0;
                                    meshBatch.PrimitveType = OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip; // HackHack, this varies per primitive.
                                    // We need to look on each primitive and convert them to trianglestrips, most are TS some are TF's.

                                    // We re-use the list struct here to dynamically add paired pos/col/tex as we load them
                                    // then we convert them into arrays for the MeshBatch afterwards.
                                    MeshVertexAttributeHolder meshVertexData = new MeshVertexAttributeHolder();

                                    // chunkStart + batchOffset gets you the position where the batches are listed
                                    // 0x28 * b gives you the right batch - a batch is 0x28 in length
                                    reader.BaseStream.Position = chunkStart + batchOffset + (0x28 * b);
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

                                    for (ushort p = 0; p < packetCount; p++)
                                    {
                                        // Packet Location
                                        reader.BaseStream.Position = chunkStart + packetLocationOffset;
                                        reader.BaseStream.Position += (packetIndex + p) * 0x8; // A Packet Location is 0x8 long, so we skip ahead to the right one.

                                        uint packetSize = reader.ReadUInt32();
                                        uint packetOffset = reader.ReadUInt32();

                                        // Jump the read head to the location of the primitives for this packet.
                                        reader.BaseStream.Position = chunkStart + primitiveDataOffset + packetOffset;

                                        uint numPrimitiveBytesRead = 0;
                                        while (numPrimitiveBytesRead < packetSize)
                                        {
                                            // Jump to the primitives
                                            // Primitives
                                            J3DFileResource.PrimitiveType type = (J3DFileResource.PrimitiveType)reader.ReadByte();
                                            ushort vertexCount = reader.ReadUInt16();

                                            numPrimitiveBytesRead += 0x3; // Advance us by 3 for the Primitive header.

                                            // Game pads the chunks out with zeros, so this is the signal for an early break;
                                            if (type == 0)
                                                break;

                                            for (int v = 0; v < vertexCount; v++)
                                            {
                                                meshVertexData.Indexes.Add(overallVertexCount);
                                                overallVertexCount++;

                                                // Iterate through the attribute types. I think the actual vertices are stored in interleaved format,
                                                // ie: there's say 13 vertexes but those 13 vertexes will have a pos/color/tex index listed after it
                                                // depending on the overall attributes of the file.
                                                for (int attrib = 0; attrib < vertexData.Attributes.Count; attrib++)
                                                {
                                                    long curPrimitiveStreamPos = chunkStart + primitiveDataOffset + numPrimitiveBytesRead + packetOffset;

                                                    // Jump the stream head forward to read the attribute type.
                                                    // chunkStart + attributeOffset = start of attribute section
                                                    // batchAttributeOffset + (attrib * 0x8) = attributes for this batch, 0x8 is the size of one attribute.
                                                    reader.BaseStream.Position = chunkStart + attributeOffset + batchAttributeOffset + (attrib * 0x8);
                                                    J3DFileResource.VertexArrayType batchAttribType = (J3DFileResource.VertexArrayType)reader.ReadInt32();
                                                    J3DFileResource.VertexDataType batchDataType = (J3DFileResource.VertexDataType)reader.ReadInt32();

                                                    // Jump back to the primitives we were reading...
                                                    reader.BaseStream.Position = curPrimitiveStreamPos;

                                                    // Now that we know how big the vertex type is stored in (either a Signed8 or a Signed16) we can read that much data
                                                    // and then we can use that index and index into 
                                                    int attributeArrayIndex = 0;
                                                    switch (batchDataType)
                                                    {
                                                        case J3DFileResource.VertexDataType.Signed8:
                                                            attributeArrayIndex = reader.ReadByte();
                                                            numPrimitiveBytesRead += 1;
                                                            break;
                                                        case J3DFileResource.VertexDataType.Signed16:
                                                            attributeArrayIndex = reader.ReadInt16();
                                                            numPrimitiveBytesRead += 2;
                                                            break;
                                                        default:
                                                            Console.WriteLine("[J3DLoader] Unknown Batch Index Type");
                                                            break;
                                                    }

                                                    // Now that we know what the index is, we can retrieve it from the appropriate array
                                                    // and stick it into our vertex. The J3D format removes all duplicate vertex attributes
                                                    // so we need to re-duplicate them here so that we can feed them to a PC GPU in a normal fashion.
                                                    switch (batchAttribType)
                                                    {
                                                        case J3DFileResource.VertexArrayType.Position:
                                                            meshVertexData.Position.Add(vertexData.Position[attributeArrayIndex]);
                                                            break;
                                                        case J3DFileResource.VertexArrayType.Color0:
                                                            meshVertexData.Color0.Add(vertexData.Color0[attributeArrayIndex]);
                                                            break;
                                                        case J3DFileResource.VertexArrayType.Tex0:
                                                            meshVertexData.Tex0.Add(vertexData.Tex0[attributeArrayIndex]);
                                                            break;

                                                        case J3DFileResource.VertexArrayType.Normal:
                                                            meshVertexData.Normal.Add(vertexData.Normal[attributeArrayIndex]);
                                                            break;

                                                        case J3DFileResource.VertexArrayType.Color1:
                                                        case J3DFileResource.VertexArrayType.Tex1:

                                                        case J3DFileResource.VertexArrayType.Tex2:
                                                        case J3DFileResource.VertexArrayType.Tex3:
                                                        case J3DFileResource.VertexArrayType.Tex4:
                                                        case J3DFileResource.VertexArrayType.Tex5:
                                                        case J3DFileResource.VertexArrayType.Tex6:
                                                        case J3DFileResource.VertexArrayType.Tex7:
                                                        default:
                                                            Console.WriteLine("[J3DLoader] Unsupported attribType {0}", batchAttribType);
                                                            break;
                                                    }
                                                }
                                            }

                                            // After we write a primitive, write a speciall null-terminator
                                            meshVertexData.Indexes.Add(0xFFFF);
                                        }
                                    }

                                    meshBatch.Vertices = meshVertexData.Position.ToArray();
                                    meshBatch.Color0 = meshVertexData.Color0.ToArray();
                                    meshBatch.TexCoord0 = meshVertexData.Tex0.ToArray();
                                    meshBatch.Indexes = meshVertexData.Indexes.ToArray();
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

            RenderSystem.HackInstance.m_meshList.Add(resource.Mesh);
        }

        private static MeshVertexAttributeHolder LoadVTX1FromFile(J3DFileResource resource, EndianBinaryReader reader, int chunkSize)
        {
            MeshVertexAttributeHolder dataHolder = new MeshVertexAttributeHolder();

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

            // Don't count the last vertexFormat as it's the NullAttr one.
            dataHolder.Attributes = vertexFormats.GetRange(0, vertexFormats.Count - 1);

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

                            dataHolder.Position = vertices;
                        }
                        break;

                    // Normal Data
                    case 1:
                        {
                            var vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Normal);

                            // ToDo: Use the vertexFormat to see how big a vertex actually is.
                            int vertexSize = 0;
                            switch(vertexFormat.DataType)
                            {
                                case J3DFileResource.VertexDataType.Float32:
                                    vertexSize = 3 * 4;
                                    break;

                                case J3DFileResource.VertexDataType.Unsigned16:
                                case J3DFileResource.VertexDataType.Signed16:
                                    vertexSize = 3 * 2;
                                    break;
                            }

                            int numNormals = totalLength / vertexSize;
                            List<Vector3> normals = new List<Vector3>(numNormals);
                            float scaleFactor = (float)Math.Pow(0.5, vertexFormat.DecimalPoint);

                            for (int v = 0; v < numNormals; v++)
                            {
                                Vector3 normal = new Vector3();

                                switch (vertexFormat.DataType)
                                {
                                    case J3DFileResource.VertexDataType.Float32:
                                        normal.X = reader.ReadSingle() * scaleFactor;
                                        normal.Y = reader.ReadSingle() * scaleFactor;
                                        normal.Z = reader.ReadSingle() * scaleFactor;
                                        break;

                                    case J3DFileResource.VertexDataType.Signed16:
                                        normal.X = reader.ReadInt16() * scaleFactor;
                                        normal.Y = reader.ReadInt16() * scaleFactor;
                                        normal.Z = reader.ReadInt16() * scaleFactor;
                                        break;
                                }


                                normals.Add(normal);
                            }

                            dataHolder.Normal = normals;
                        }
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

                            dataHolder.Color0 = colors;
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

                            dataHolder.Tex0 = texCoords;
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

            return dataHolder;
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
