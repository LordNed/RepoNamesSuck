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
            public List<Vector2> Tex1;
            public List<int> Indexes;

            public List<J3DFileResource.VertexFormat> Attributes;

            public MeshVertexAttributeHolder()
            {
                Position = new List<Vector3>();
                Normal = new List<Vector3>();
                Color0 = new List<Color>();
                Tex0 = new List<Vector2>();
                Tex1 = new List<Vector2>();
                Attributes = new List<J3DFileResource.VertexFormat>();
                Indexes = new List<int>();
            }
        }

        private class SceneNode
        {
            public List<SceneNode> Children;
            public J3DFileResource.HierarchyDataTypes Type;
            public ushort Value;

            public SceneNode()
            {
                Children = new List<SceneNode>();
            }
        }

        public static void Load(J3DFileResource resource, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found");

            MeshVertexAttributeHolder vertexData = null;
            SceneNode rootNode = new SceneNode();

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
                        case "INF1":
                            {
                                //ushort unknown0 = reader.ReadUInt16();
                                //ushort unknown1 = reader.ReadUInt16(); // wtf is with my documentation.
                                ushort unknown1 = reader.ReadUInt16(); // A lot of Link's models have it but no idea what it means. Alt. doc says: "0 for BDL, 01 for BMD"
                                ushort padding = reader.ReadUInt16();
                                uint packetCount = reader.ReadUInt32(); // Total number of Packets across all Batches in file.
                                uint vertexCount = reader.ReadUInt32(); // Total number of vertexes across all batches within the file.
                                uint hierarchyDataOffset = reader.ReadUInt32();

                                // The Hierarchy defines how Joints, Materials and Shapes are laid out. This allows them to bind a material
                                // and draw multiple shapes (batches) with the material. It also complicates drawing things, but whatever.
                                reader.BaseStream.Position = chunkStart + hierarchyDataOffset;

                                List<J3DFileResource.InfoNode> infoNodes = new List<J3DFileResource.InfoNode>();
                                J3DFileResource.InfoNode curNode = null;

                                do
                                {
                                    curNode = new J3DFileResource.InfoNode();
                                    curNode.Type = (J3DFileResource.HierarchyDataTypes)reader.ReadUInt16();
                                    curNode.Value = reader.ReadUInt16(); // "Index into Joint, Material, or Shape table.

                                    infoNodes.Add(curNode);
                                }
                                while (curNode.Type != J3DFileResource.HierarchyDataTypes.Finish);

                                ConvertInfoHiearchyToSceneGraph(ref rootNode, infoNodes, 0);
                            }
                            break;
                        case "VTX1":
                            vertexData = LoadVTX1FromFile(resource, reader, chunkSize);
                            break;

                        case "SHP1":
                            LoadSHP1Section(vertexData, j3dMesh, reader, chunkStart);
                            break;

                        case "MAT3":
                            LoadMAT3Section(reader, chunkStart);

                            break;


                        case "TEX1":

                            break;

                    }

                    reader.BaseStream.Position = chunkStart + chunkSize;
                }
            }

            RenderSystem.HackInstance.m_meshList.Add(resource.Mesh);
        }

        /// <summary>
        /// This function is used to convert from a flat-file of J3DFileResource.InfoNodes into a treeview-type version of SceneNode.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="allNodes"></param>
        /// <param name="currentListIndex"></param>
        /// <returns></returns>
        private static int ConvertInfoHiearchyToSceneGraph(ref SceneNode parent, List<J3DFileResource.InfoNode> allNodes, int currentListIndex)
        {
            for (int i = currentListIndex; i < allNodes.Count; i++)
            {
                J3DFileResource.InfoNode curNode = allNodes[i];
                SceneNode newNode = new SceneNode();

                switch (curNode.Type)
                {
                    case J3DFileResource.HierarchyDataTypes.NewNode:
                        // Increase the depth of the hierarchy by creating a new node and then processing all of the next nodes as its children.
                        // This function is recursive and will return the integer value of how many nodes it processed, this allows us to skip
                        // the list forward that many now that they've been handled.
                        newNode.Type = J3DFileResource.HierarchyDataTypes.NewNode;
                        newNode.Value = curNode.Value;

                        i += ConvertInfoHiearchyToSceneGraph(ref newNode, allNodes, i + 1);
                        parent.Children.Add(newNode);
                        break;

                    case J3DFileResource.HierarchyDataTypes.EndNode:
                        // Alternatively, if it's a EndNode, that's our signal to go up a level. We return the number of nodes that were processed between the last NewNode
                        // and this EndNode at this depth in the hierarchy.
                        return i - currentListIndex + 1;

                    case J3DFileResource.HierarchyDataTypes.Material:
                    case J3DFileResource.HierarchyDataTypes.Joint:
                    case J3DFileResource.HierarchyDataTypes.Batch:
                    case J3DFileResource.HierarchyDataTypes.Finish:

                        // If it's any of the above we simply create a node for them. We create and pull from a different InfoNode because
                        // Hitting a NewNode can modify the value of i so curNode is now no longer valid.
                        J3DFileResource.InfoNode thisNode = allNodes[i];
                        newNode.Type = thisNode.Type;
                        newNode.Value = thisNode.Value;
                        parent.Children.Add(newNode);
                        break;

                    default:
                        Console.WriteLine("[J3DLoader] Unsupported HierarchyDataType \"{0}\" in model!", curNode.Type);
                        break;
                }
            }

            return 0;
        }

        private static void LoadMAT3Section(EndianBinaryReader reader, long chunkStart)
        {
            short materialCount = reader.ReadInt16();
            short padding = reader.ReadInt16();
            int materialsOffset = reader.ReadInt32();
            int materialIndexOffset = reader.ReadInt32();
            int stringTableOffset = reader.ReadInt32(); // Name Offset
            int indirectTexturingOffset = reader.ReadInt32();
            int gxCullModeOffset = reader.ReadInt32();
            int materialColorOffset = reader.ReadInt32(); // gxColorMaterial Color
            int colorChanNum = reader.ReadInt32();
            int colorChanInfoOffset = reader.ReadInt32();
            int ambientColorOffset = reader.ReadInt32(); // Ambient Color
            int lightInfoOffset = reader.ReadInt32(); //
            int texGenNumber = reader.ReadInt32(); // numTexGens
            int texCoordInfoOffset = reader.ReadInt32(); // TexCoordGen Offset
            int texCoordInfo2Offset = reader.ReadInt32();
            int texMatrixInfoOffset = reader.ReadInt32();
            int texMatrixInfo2Offset = reader.ReadInt32();
            int texTableOffset = reader.ReadInt32(); // Texture Offset (?)
            int tevOrderInfoOffset = reader.ReadInt32();
            int tevColorOffset = reader.ReadInt32(); // gxColorS10
            int tevKColorOffset = reader.ReadInt32(); // gxColor
            int tevStageNumInfoOffset = reader.ReadInt32();
            int tevStageInfoOffset = reader.ReadInt32(); // Tev Combiner
            int tevSwapModeInfoOffset = reader.ReadInt32();
            int tevSwapModeTableInfoOffset = reader.ReadInt32();
            int fogInfoOffset = reader.ReadInt32();
            int alphaCompInfoOffset = reader.ReadInt32();
            int blendInfoOffset = reader.ReadInt32();
            int zModeInfoOffset = reader.ReadInt32();
            int ditherInfoOffset = reader.ReadInt32();
            int nbtScaleInfoOffset = reader.ReadInt32();

            for (int m = 0; m < materialCount; m++)
            {
                // A Material entry is 0x14c long.
                reader.BaseStream.Position = chunkStart + materialsOffset + (m * 0x14c);

                // Start reading each material.
                J3DFileResource.MaterialEntry material = new J3DFileResource.MaterialEntry();
                material.Unknown1Index = reader.ReadByte();
                material.CullModeIndex = reader.ReadByte();
                material.NumChannelsIndex = reader.ReadByte();
                material.NumTexGensIndex = reader.ReadByte();
                material.NumTevStagesIndex = reader.ReadByte();
                material.ZCompareLocIndex = reader.ReadByte();
                material.ZModeIndex = reader.ReadByte();
                material.DitherIndex = reader.ReadByte();

                material.MaterialColorIndex = new short[2];
                for (int l = 0; l < 2; l++)
                    material.MaterialColorIndex[l] = reader.ReadInt16();

                material.ChannelControlIndex = new short[4];
                for (int l = 0; l < 4; l++)
                    material.ChannelControlIndex[l] = reader.ReadInt16();

                material.AmbientColorIndex = new short[2];
                for (int l = 0; l < 2; l++)
                    material.AmbientColorIndex[l] = reader.ReadInt16();

                material.LightingIndex = new short[8];
                for (int l = 0; l < 8; l++)
                    material.LightingIndex[l] = reader.ReadInt16();

                material.TexCoordIndex = new short[8];
                for (int l = 0; l < 8; l++)
                    material.TexCoordIndex[l] = reader.ReadInt16();

                material.TexCoord2Index = new short[8];
                for (int l = 0; l < 8; l++)
                    material.TexCoord2Index[l] = reader.ReadInt16();

                material.TexMatrixIndex = new short[10];
                for (int l = 0; l < 10; l++)
                    material.TexMatrixIndex[l] = reader.ReadInt16();

                material.TexMatrix2Index = new short[20];
                for (int l = 0; l < 20; l++)
                    material.TexMatrix2Index[l] = reader.ReadInt16();

                material.texIndex = new short[8];
                for (int l = 0; l < 8; l++)
                    material.texIndex[l] = reader.ReadInt16();

                material.tevConstantColorIndex = new short[4];
                for (int l = 0; l < 4; l++)
                    material.tevConstantColorIndex[l] = reader.ReadInt16();

                material.constColorSel = new byte[16];
                for (int l = 0; l < 16; l++)
                    material.constColorSel[l] = reader.ReadByte();

                material.constAlphaSel = new byte[16];
                for (int l = 0; l < 16; l++)
                    material.constAlphaSel[l] = reader.ReadByte();

                material.tevOrderIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevOrderIndex[l] = reader.ReadInt16();

                material.tevColorIndex = new short[4];
                for (int l = 0; l < 4; l++)
                    material.tevColorIndex[l] = reader.ReadInt16();

                material.tevStageInfoIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevStageInfoIndex[l] = reader.ReadInt16();

                material.tevSwapModeInfoIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevSwapModeInfoIndex[l] = reader.ReadInt16();

                material.tevSwapModeTableInfoIndex = new short[16];
                for (int l = 0; l < 16; l++)
                    material.tevSwapModeTableInfoIndex[l] = reader.ReadInt16();

                material.unknownIndices = new short[12];
                for (int l = 0; l < 12; l++)
                    material.unknownIndices[l] = reader.ReadInt16();

                material.FogIndex = reader.ReadInt16();
                material.AlphaCompareIndex = reader.ReadInt16();
                material.BlendModeIndex = reader.ReadInt16();
                material.Unknown2Index = reader.ReadInt16();
            }
        }

        private static void LoadSHP1Section(MeshVertexAttributeHolder vertexData, Mesh j3dMesh, EndianBinaryReader reader, long chunkStart)
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
                curFormat.ColorDataType = (J3DFileResource.VertexColorType)curFormat.DataType;
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
                J3DFileResource.VertexFormat vertexFormat = null;

                switch (k)
                {
                    // Position Data
                    case 0:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Position);
                        dataHolder.Position = LoadVertexAttribute<Vector3>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Position, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Normal Data
                    case 1:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Normal);
                        dataHolder.Normal = LoadVertexAttribute<Vector3>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Normal, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Normal Binormal Tangent Data (presumed)
                    case 2:
                        break;

                    // Color 0 Data
                    case 3:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Color0);
                        dataHolder.Color0 = LoadVertexAttribute<Color>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Color0, J3DFileResource.VertexDataType.None, vertexFormat.ColorDataType);
                        break;

                    // Color 1 Data (presumed)
                    case 4:
                        break;

                    // Tex 0 Data
                    case 5:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex0);
                        dataHolder.Tex0 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex0, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Tex 1 Data
                    case 6:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex1);
                        dataHolder.Tex1 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex1, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Other Tex data
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

        private static List<T> LoadVertexAttribute<T>(EndianBinaryReader reader, int totalAttributeDataLength, byte decimalPoint, J3DFileResource.VertexArrayType arrayType, J3DFileResource.VertexDataType dataType, J3DFileResource.VertexColorType colorType) where T : new()
        {
            int componentCount = 0;
            switch (arrayType)
            {
                case J3DFileResource.VertexArrayType.Position:
                case J3DFileResource.VertexArrayType.Normal:
                    componentCount = 3;
                    break;
                case J3DFileResource.VertexArrayType.Color0:
                case J3DFileResource.VertexArrayType.Color1:
                    componentCount = 4;
                    break;
                case J3DFileResource.VertexArrayType.Tex0:
                case J3DFileResource.VertexArrayType.Tex1:
                case J3DFileResource.VertexArrayType.Tex2:
                case J3DFileResource.VertexArrayType.Tex3:
                case J3DFileResource.VertexArrayType.Tex4:
                case J3DFileResource.VertexArrayType.Tex5:
                case J3DFileResource.VertexArrayType.Tex6:
                case J3DFileResource.VertexArrayType.Tex7:
                    componentCount = 2;
                    break;
                default:
                    Console.WriteLine("[J3DLoader] Unsupported ArrayType \"{0}\" found while loading VTX1!", arrayType);
                    break;
            }


            // We need to know the length of each 'vertex' (which can vary based on how many attributes and what types there are)
            int vertexSize = 0;
            switch (dataType)
            {
                case J3DFileResource.VertexDataType.Float32:
                    vertexSize = componentCount * 4;
                    break;

                case J3DFileResource.VertexDataType.Unsigned16:
                case J3DFileResource.VertexDataType.Signed16:
                    vertexSize = componentCount * 2;
                    break;

                case J3DFileResource.VertexDataType.Signed8:
                case J3DFileResource.VertexDataType.Unsigned8:
                    vertexSize = componentCount * 1;
                    break;
                default:
                    Console.WriteLine("[J3DLoader] Unsupported DataType \"{0}\" found while loading VTX1!", dataType);
                    break;
            }

            switch (colorType)
            {
                case J3DFileResource.VertexColorType.RGB8:
                    vertexSize = 3;
                    break;
                case J3DFileResource.VertexColorType.RGBX8:
                case J3DFileResource.VertexColorType.RGBA8:
                    vertexSize = 4;
                    break;

                case J3DFileResource.VertexColorType.None:
                    break;

                case J3DFileResource.VertexColorType.RGB565:
                case J3DFileResource.VertexColorType.RGBA4:
                case J3DFileResource.VertexColorType.RGBA6:
                default:
                    Console.WriteLine("[J3DLoader] Unsupported Color Data Type: {0}!", colorType);
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
                        case J3DFileResource.VertexDataType.Float32:
                            value[i] = reader.ReadSingle() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Unsigned16:
                            value[i] = reader.ReadUInt16() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Signed16:
                            value[i] = reader.ReadInt16() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Unsigned8:
                            value[i] = reader.ReadByte() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Signed8:
                            value[i] = reader.ReadSByte() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.None:
                            // Let the next switch statement get it.
                            break;

                        default:
                            Console.WriteLine("[J3DLoader] Unsupported Data Type: {0}!", dataType);
                            break;
                    }


                    switch (colorType)
                    {
                        case J3DFileResource.VertexColorType.RGBX8:
                        case J3DFileResource.VertexColorType.RGB8:
                        case J3DFileResource.VertexColorType.RGBA8:
                            value[i] = reader.ReadByte() / 255f;
                            break;

                        case J3DFileResource.VertexColorType.None:
                            break;

                        case J3DFileResource.VertexColorType.RGB565:
                        case J3DFileResource.VertexColorType.RGBA4:
                        case J3DFileResource.VertexColorType.RGBA6:
                        default:
                            Console.WriteLine("[J3DLoader] Unsupported Color Data Type: {0}!", colorType);
                            break;
                    }
                }
                values.Add(value);
            }

            return values;
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
