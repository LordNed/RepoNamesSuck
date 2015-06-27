using GameFormatReader.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
        private class MeshVertexAttributeHolder
        {
            public List<Vector3> Position;
            public List<Vector3> Normal;
            public List<Color> Color0;
            public List<Color> Color1;
            public List<Vector2> Tex0;
            public List<Vector2> Tex1;
            public List<Vector2> Tex2;
            public List<Vector2> Tex3;
            public List<Vector2> Tex4;
            public List<Vector2> Tex5;
            public List<Vector2> Tex6;
            public List<Vector2> Tex7;
            public List<int> PositionMatrixIndexes;
            public List<int> Indexes;

            public List<J3DFileResource.VertexFormat> Attributes;

            public MeshVertexAttributeHolder()
            {
                Position = new List<Vector3>();
                Normal = new List<Vector3>();
                Color0 = new List<Color>();
                Color1 = new List<Color>();
                Tex0 = new List<Vector2>();
                Tex1 = new List<Vector2>();
                Tex2 = new List<Vector2>();
                Tex3 = new List<Vector2>();
                Tex4 = new List<Vector2>();
                Tex5 = new List<Vector2>();
                Tex6 = new List<Vector2>();
                Tex7 = new List<Vector2>();
                Attributes = new List<J3DFileResource.VertexFormat>();

                PositionMatrixIndexes = new List<int>();
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

            public override string ToString()
            {
                return string.Format("{0} - {1}", Type, Value);
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
            List<Texture2D> textureList = new List<Texture2D>();
            List<ushort> materialRemapIndexs = new List<ushort>();
            List<WEditor.Common.Nintendo.J3D.Material> materialList = null;
            List<SkeletonBone> skeletonBones;

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
                        // INFO - Vertex Count, Scene Hierarchy
                        case "INF1":
                            rootNode = LoadINF1FromFile(rootNode, reader, chunkStart);
                            break;
                        // VERTEX - Stores vertex arrays for pos/normal/color0/tex0 etc. Contains VertexAttributes which describe
                        // how this data is stored/laid out.
                        case "VTX1":
                            vertexData = LoadVTX1FromFile(resource, reader, chunkStart, chunkSize);
                            break;
                        // ENVELOPES - Defines vertex weights for skinning.
                        case "EVP1":
                            break;
                        // DRAW (Skeletal Animation Data) - Stores which matrices are weighted, and which are used directly.
                        case "DRW1":
                            break;
                        // JOINTS - Stores the skeletal joints (position, rotation, scale, etc.)
                        case "JNT1":
                            skeletonBones = LoadJNT1SectionFromStream(reader, chunkStart);
                            break;
                        // SHAPE - Face/Triangle information for model.
                        case "SHP1":
                            LoadSHP1SectionFromFile(vertexData, j3dMesh, reader, chunkStart);
                            break;
                        // MATERIAL - Stores materials (which describes how textures, etc. are drawn)
                        case "MAT3":
                            materialList = LoadMAT3SectionFromStream(reader, chunkStart, chunkSize, materialRemapIndexs);
                            break;
                        // TEXTURES - Stores binary texture images.
                        case "TEX1":
                            textureList = LoadTEX1FromFile(reader, chunkStart);
                            break;
                        // MODEL - Seems to be bypass commands for Materials and invokes GX registers directly.
                        case "MDL3":
                            break;
                    }

                    reader.BaseStream.Position = chunkStart + chunkSize;
                }
            }

            // Resolve the texture indexes into actual textures now that we've loaded the TEX1 section.
            foreach (Material mat in materialList)
            {
                for (int i = 0; i < mat.TextureIndexes.Length; i++)
                {
                    if (mat.TextureIndexes[i] < 0)
                        continue;

                    mat.Textures.Add(textureList[mat.TextureIndexes[0]]);
                }
            }

            // loltests
            for (int i = 0; i < materialList.Count; i++)
            {
                materialList[i].VtxDesc = j3dMesh.SubMeshes[0].GetVertexDescription();
                Shader shader = TEVShaderGenerator.GenerateShader(materialList[i]);
                materialList[i].Shader = shader;
            }

            // We're going to do something a little crazy - we're going to read the scene view and apply textures to meshes (for now)
            Material curMat = null;
            AssignTextureToMeshRecursive(rootNode, j3dMesh, textureList, ref curMat, materialList, materialRemapIndexs);
            RenderSystem.HackInstance.m_meshList.Add(resource.Mesh);
        }

        

        private static void AssignTextureToMeshRecursive(SceneNode node, Mesh mesh, List<Texture2D> textures, ref Material curMaterial, List<Material> materialList, List<ushort> remapIndexList)
        {
            if (node.Type == J3DFileResource.HierarchyDataTypes.Material)
            {
                // Don't ask me why this is so complicated. The node.Value has an index into the remapIndexList,
                // which gives an index into the MaterialList, which finally gives an index into the Textures list.
                // And no, I dont' know why texIndex is an array.
                //
                // Apparently it gets one step more complicated. You then have to take the textureIndex provided by the material
                // and index into the finalTextureIndex list that comes from the MAT3 section, lol.
                ushort materialIndex = remapIndexList[node.Value];
                short textureIndex = materialList[materialIndex].TextureIndexes[0];

                curMaterial = materialList[materialIndex];

                // Models that don't use textures will have a textureIndex of -1.
                //if(textureIndex >= 0)
                //curTexture = textures[textureIndex];
            }

            if (node.Type == J3DFileResource.HierarchyDataTypes.Batch)
                mesh.SubMeshes[node.Value].Material = curMaterial;

            foreach (var child in node.Children)
                AssignTextureToMeshRecursive(child, mesh, textures, ref curMaterial, materialList, remapIndexList);
        }

        private static List<Texture2D> LoadTEX1FromFile(EndianBinaryReader reader, long chunkStart)
        {
            ushort textureCount = reader.ReadUInt16();
            ushort padding = reader.ReadUInt16(); // Usually 0xFFFF?
            uint textureHeaderOffset = reader.ReadUInt32(); // textureCount # bti image headers are stored here, relative to chunkStart.
            uint stringTableOffset = reader.ReadUInt32(); // One filename per texture. relative to chunkStart.
            List<Texture2D> textureList = new List<Texture2D>();

            // Get all Texture Names
            reader.BaseStream.Position = chunkStart + stringTableOffset;
            StringTable stringTable = StringTable.FromStream(reader);

            for (int t = 0; t < textureCount; t++)
            {
                // 0x20 is the length of the BinaryTextureImage header which all come in a row, but then the stream gets jumped around while loading the BTI file.
                reader.BaseStream.Position = chunkStart + textureHeaderOffset + (t * 0x20);
                BinaryTextureImage texture = new BinaryTextureImage();
                texture.Load(reader, chunkStart + 0x20, t);

                Texture2D texture2D = new Texture2D(texture.Width, texture.Height);
                texture2D.PixelData = texture.GetData();
                textureList.Add(texture2D);


                string executionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                texture.SaveImageToDisk(executionPath + "/TextureDump/" + string.Format("[{2}]_{0}_{1}.png", stringTable[t], texture.Format, t));
            }

            return textureList;
        }


        private struct ShapeAttribute
        {
            public J3DFileResource.VertexArrayType ArrayType;
            public J3DFileResource.VertexDataType DataType;
        }

        private static void LoadSHP1SectionFromFile(MeshVertexAttributeHolder vertexData, Mesh j3dMesh, EndianBinaryReader reader, long chunkStart)
        {
            short batchCount = reader.ReadInt16();
            short padding = reader.ReadInt16();
            int batchOffset = reader.ReadInt32();
            int unknownTableOffset = reader.ReadInt32();
            int alwaysZero = reader.ReadInt32(); Debug.Assert(alwaysZero == 0);            
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
                byte unknown0 = reader.ReadByte(); Debug.Assert(unknown0 == 0xFF);
                ushort packetCount = reader.ReadUInt16();
                ushort batchAttributeOffset = reader.ReadUInt16();
                ushort firstMatrixIndex = reader.ReadUInt16();
                ushort firstPacketIndex = reader.ReadUInt16();
                ushort unknownpadding = reader.ReadUInt16(); Debug.Assert(unknownpadding == 0xFFFF);

                float boundingSphereDiameter = reader.ReadSingle();
                Vector3 boundingBoxMin = new Vector3();
                boundingBoxMin.X = reader.ReadSingle();
                boundingBoxMin.Y = reader.ReadSingle();
                boundingBoxMin.Z = reader.ReadSingle();

                Vector3 boundingBoxMax = new Vector3();
                boundingBoxMax.X = reader.ReadSingle();
                boundingBoxMax.Y = reader.ReadSingle();
                boundingBoxMax.Z = reader.ReadSingle();


                // We need to figure out how many primitive attributes there are in the SHP1 section. This can differ from the number of
                // attributes in the VTX1 section, as the SHP1 can also include things like PositionMatrixIndex, so the count can be different.
                // This also varies *per batch* as not all batches will have the things like PositionMatrixIndex. Wow.
                reader.BaseStream.Position = chunkStart + attributeOffset + batchAttributeOffset;
                var shp1Attributes = new List<ShapeAttribute>();
                do
                {
                    ShapeAttribute attribute = new ShapeAttribute();
                    attribute.ArrayType = (J3DFileResource.VertexArrayType)reader.ReadInt32();
                    attribute.DataType = (J3DFileResource.VertexDataType)reader.ReadInt32();

                    if (attribute.ArrayType == J3DFileResource.VertexArrayType.NullAttr)
                        break;

                    shp1Attributes.Add(attribute);

                } while (true);


                for (ushort p = 0; p < packetCount; p++)
                {
                    // Packet Location
                    reader.BaseStream.Position = chunkStart + packetLocationOffset;
                    reader.BaseStream.Position += (firstPacketIndex + p) * 0x8; // A Packet Location is 0x8 long, so we skip ahead to the right one.

                    int packetSize = reader.ReadInt32();
                    int packetOffset = reader.ReadInt32();

                    // Jump the read head to the location of the primitives for this packet.
                    reader.BaseStream.Position = chunkStart + primitiveDataOffset + packetOffset;

                    uint numPrimitiveBytesRead = 0;
                    while (numPrimitiveBytesRead < packetSize)
                    {
                        // Jump to the primitives
                        // Primitives
                        GXPrimitiveType type = (GXPrimitiveType)reader.ReadByte();
                        // Game pads the chunks out with zeros, so this is the signal for an early break;
                        if (type == 0 || numPrimitiveBytesRead >= packetSize)
                            break;

                        ushort vertexCount = reader.ReadUInt16();

                        if (type != GXPrimitiveType.TriangleStrip)
                        {
                            Console.WriteLine("Unsupported GXPrimitiveType {0}", type);
                        }

                        numPrimitiveBytesRead += 0x3; // Advance us by 3 for the Primitive header.

            

                        for (int v = 0; v < vertexCount; v++)
                        {
                            meshVertexData.Indexes.Add(overallVertexCount);
                            overallVertexCount++;

                            // Iterate through the attribute types. I think the actual vertices are stored in interleaved format,
                            // ie: there's say 13 vertexes but those 13 vertexes will have a pos/color/tex index listed after it
                            // depending on the overall attributes of the file.
                            for (int attrib = 0; attrib < shp1Attributes.Count; attrib++)
                            {
                                // Jump to primitive location
                                //reader.BaseStream.Position = chunkStart + primitiveDataOffset + numPrimitiveBytesRead + packetOffset;

                                // Now that we know how big the vertex type is stored in (either a Signed8 or a Signed16) we can read that much data
                                // and then we can use that index and index into 
                                int val = 0;
                                uint numBytesRead = 0;
                                switch (shp1Attributes[attrib].DataType)
                                {
                                    case J3DFileResource.VertexDataType.Signed8:
                                        val = reader.ReadByte();
                                        numBytesRead = 1;
                                        break;
                                    case J3DFileResource.VertexDataType.Signed16:
                                        val = reader.ReadInt16();
                                        numBytesRead = 2;
                                        break;
                                    default:
                                        Console.WriteLine("[J3DLoader] Unknown Batch Index Type: {0}", shp1Attributes[attrib].DataType);
                                        break;
                                }

                                // Now that we know what the index is, we can retrieve it from the appropriate array
                                // and stick it into our vertex. The J3D format removes all duplicate vertex attributes
                                // so we need to re-duplicate them here so that we can feed them to a PC GPU in a normal fashion.
                                switch (shp1Attributes[attrib].ArrayType)
                                {
                                    case J3DFileResource.VertexArrayType.Position:
                                        meshVertexData.Position.Add(vertexData.Position[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.PositionMatrixIndex:
                                        meshVertexData.PositionMatrixIndexes.Add(val);
                                        break;
                                    case J3DFileResource.VertexArrayType.Normal:
                                        meshVertexData.Normal.Add(vertexData.Normal[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Color0:
                                        meshVertexData.Color0.Add(vertexData.Color0[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Color1:
                                        meshVertexData.Color1.Add(vertexData.Color1[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex0:
                                        meshVertexData.Tex0.Add(vertexData.Tex0[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex1:
                                        meshVertexData.Tex1.Add(vertexData.Tex1[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex2:
                                        meshVertexData.Tex2.Add(vertexData.Tex2[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex3:
                                        meshVertexData.Tex3.Add(vertexData.Tex3[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex4:
                                        meshVertexData.Tex4.Add(vertexData.Tex4[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex5:
                                        meshVertexData.Tex5.Add(vertexData.Tex5[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex6:
                                        meshVertexData.Tex6.Add(vertexData.Tex6[val]);
                                        break;
                                    case J3DFileResource.VertexArrayType.Tex7:
                                        meshVertexData.Tex7.Add(vertexData.Tex7[val]);
                                        break;
                                    default:
                                        Console.WriteLine("[J3DLoader] Unsupported attribType {0}", shp1Attributes[attrib].ArrayType);
                                        break;
                                }

                                numPrimitiveBytesRead += numBytesRead;
                            }
                        }

                        // After we write a primitive, write a speciall null-terminator
                        meshVertexData.Indexes.Add(0xFFFF);
                    }


                    // Now read the matrix data for this packet
                    reader.BaseStream.Position = chunkStart + matrixDataOffset + (firstMatrixIndex + p) * 0x08;
                    ushort matrixUnknown0 = reader.ReadUInt16();
                    ushort matrixCount = reader.ReadUInt16();
                    uint matrixFirstIndex = reader.ReadUInt32();

                    // Skip ahead to the actual data.
                    reader.BaseStream.Position = chunkStart + matrixTableOffset + (matrixFirstIndex * 0x2);
                    List<ushort> matrixTable = new List<ushort>();
                    for(int m = 0; m < matrixCount; m++)
                    {
                        matrixTable.Add(reader.ReadUInt16());
                    }
                }

                meshBatch.Vertices = meshVertexData.Position.ToArray();
                meshBatch.Color0 = meshVertexData.Color0.ToArray();
                meshBatch.Color1 = meshVertexData.Color1.ToArray();
                meshBatch.TexCoord0 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord1 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord2 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord3 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord4 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord5 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord6 = meshVertexData.Tex0.ToArray();
                meshBatch.TexCoord7 = meshVertexData.Tex0.ToArray();
                meshBatch.Indexes = meshVertexData.Indexes.ToArray();

            }
        }

        private static MeshVertexAttributeHolder LoadVTX1FromFile(J3DFileResource resource, EndianBinaryReader reader, long chunkStart, int chunkSize)
        {
            MeshVertexAttributeHolder dataHolder = new MeshVertexAttributeHolder();

            //long headerStart = reader.BaseStream.Position;
            int vertexFormatOffset = reader.ReadInt32();
            int[] vertexDataOffsets = new int[13];
            for (int k = 0; k < vertexDataOffsets.Length; k++)
                vertexDataOffsets[k] = reader.ReadInt32();

            reader.BaseStream.Position = chunkStart + vertexFormatOffset;
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
                int totalLength = GetVertexDataLength(vertexDataOffsets, k, (int)(chunkSize));
                J3DFileResource.VertexFormat vertexFormat = null;
                reader.BaseStream.Position = chunkStart + vertexDataOffsets[k];

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
                         vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Color1);
                        dataHolder.Color1 = LoadVertexAttribute<Color>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Color1, J3DFileResource.VertexDataType.None, vertexFormat.ColorDataType);
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

                    // Tex 2 Data
                    case 7:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex2);
                        dataHolder.Tex2 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex2, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Tex 3 Data
                    case 8:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex3);
                        dataHolder.Tex3 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex3, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Tex 4 Data
                    case 9:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex4);
                        dataHolder.Tex4 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex4, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Tex 5 Data
                    case 10:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex5);
                        dataHolder.Tex5 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex5, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Tex 6 Data
                    case 11:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex6);
                        dataHolder.Tex6 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex6, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
                        break;

                    // Tex 7 Data
                    case 12:
                        vertexFormat = vertexFormats.Find(x => x.ArrayType == J3DFileResource.VertexArrayType.Tex7);
                        dataHolder.Tex7 = LoadVertexAttribute<Vector2>(reader, totalLength, vertexFormat.DecimalPoint, J3DFileResource.VertexArrayType.Tex7, vertexFormat.DataType, J3DFileResource.VertexColorType.None);
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

                case J3DFileResource.VertexDataType.None:
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
                            value[i] = (float)reader.ReadUInt16() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Signed16:
                            value[i] = (float)reader.ReadInt16() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Unsigned8:
                            value[i] = (float)reader.ReadByte() * scaleFactor;
                            break;

                        case J3DFileResource.VertexDataType.Signed8:
                            value[i] = (float)reader.ReadSByte() * scaleFactor;
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
