using GameFormatReader.Common;
using OpenTK;
using System.Collections.Generic;
using System.Diagnostics;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public partial class J3DLoader
    {
        #region Internal Classes
        private sealed class MeshVertexAttributeHolder
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

            public List<VertexFormat> Attributes;

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
                Attributes = new List<VertexFormat>();

                PositionMatrixIndexes = new List<int>();
                Indexes = new List<int>();
            }
        }

        private sealed class SceneNode
        {
            public List<SceneNode> Children;
            public HierarchyDataTypes Type;
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

        private sealed class DrawInfo
        {
            public List<bool> IsWeighted;
            public List<ushort> Indexes;

            public DrawInfo()
            {
                IsWeighted = new List<bool>();
                Indexes = new List<ushort>();
            }
        }

        private sealed class ShapeAttribute
        {
            public VertexArrayType ArrayType;
            public VertexDataType DataType;

            public override string ToString()
            {
                return string.Format("{0} - {1}", ArrayType, DataType);
            }
        }

        #endregion

        public Mesh LoadFromStream(EndianBinaryReader reader)
        {
            MeshVertexAttributeHolder vertexData = null;
            SceneNode rootNode = new SceneNode();
            List<Texture2D> textureList = new List<Texture2D>();
            List<WEditor.Common.Nintendo.J3D.Material> materialList = null;
            List<SkeletonBone> joints = new List<SkeletonBone>();
            DrawInfo drawInfo = null;
            Envelopes envelopes = null;

            Mesh j3dMesh = new Mesh();

            // Read the Header
            int magic = reader.ReadInt32(); // J3D1, J3D2, etc
            if (magic != 1244873778)
            {
                WLog.Warning(LogCategory.ModelLoading, null, "Attempted to load model with invalid magic, ignoring!");
                return null;
            }

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
                        vertexData = LoadVTX1FromFile(reader, chunkStart, chunkSize);
                        break;
                    // ENVELOPES - Defines vertex weights for skinning.
                    case "EVP1":
                        envelopes = LoadEVP1FromStream(reader, chunkStart);
                        break;
                    // DRAW (Skeletal Animation Data) - Stores which matrices are weighted, and which are used directly.
                    case "DRW1":
                        drawInfo = LoadDRW1FromStream(reader, chunkStart);
                        break;
                    // JOINTS - Stores the skeletal joints (position, rotation, scale, etc.)
                    case "JNT1":
                        joints = LoadJNT1SectionFromStream(reader, chunkStart);
                        break;
                    // SHAPE - Face/Triangle information for model.
                    case "SHP1":
                        LoadSHP1SectionFromFile(vertexData, j3dMesh, reader, chunkStart);
                        break;
                    // MATERIAL - Stores materials (which describes how textures, etc. are drawn)
                    case "MAT3":
                        materialList = LoadMAT3SectionFromStream(reader, chunkStart, chunkSize);
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

            // Resolve the texture indexes into actual textures now that we've loaded the TEX1 section.
            foreach (Material mat in materialList)
            {
                for (int i = 0; i < mat.TextureIndexes.Length; i++)
                {
                    short index = mat.TextureIndexes[i];
                    if (index < 0)
                        continue;

                    mat.Textures[i] = textureList[index];
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
            AssignMaterialsToMeshRecursive(rootNode, j3dMesh, ref curMat, materialList);


            List<SkeletonBone> skeleton = new List<SkeletonBone>();
            BuildSkeletonRecursive(rootNode, skeleton, joints, 0);

            j3dMesh.Skeleton = skeleton;
            j3dMesh.BindPoses = envelopes.inverseBindPose;

            // Let's do some ugly post-processing here to see if we can't resolve all of the cross-references and turn it into
            // a normal computer-readable format that we can digest in our RenderSytem.
            {
                for (int i = 0; i < j3dMesh.SubMeshes.Count; i++)
                {
                    MeshBatch batch = j3dMesh.SubMeshes[i];
                    batch.BoneWeights = new BoneWeight[batch.Vertices.Length];

                    for (int j = 0; j < batch.PositionMatrixIndexs.Count; j++)
                    {
                        // Okay so this is where it gets more complicated. The PMI gives us an index into the MatrixTable for the packet, which we
                        // resolve and call "drawIndexes" - however we have to divide the number they give us by three for some reason, so that is 
                        // already done and now our drawIndexes array should be one-index-for-every-vertex-in-batch and it should be the index into
                        // the draw section we need.
                        ushort drw1Index = batch.drawIndexes[j];

                        // The drw1Index can be set as 0xFFFF - if so, this means that you need to use the dr1Index of the previous one.
                        // until it is no longer 0xFFFF.
                        int counter = 0;
                        while (drw1Index == 0xFFFF)
                        {
                            drw1Index = batch.drawIndexes[j - counter];
                            counter++;
                        }


                        bool isWeighted = drawInfo.IsWeighted[drw1Index];
                        BoneWeight weight = new BoneWeight();

                        if (isWeighted)
                        {
                            // Something on this doesn't work for models that actually specify a PositionMatrixIndex.
                            // So... some math is off somewhere and I don't know where for the moment.
                            ushort numBonesAffecting = envelopes.numBonesAffecting[drw1Index];
                            weight.BoneIndexes = new ushort[numBonesAffecting];
                            weight.BoneWeights = new float[numBonesAffecting];

                            // "Much WTFs"
                            ushort offset = 0;
                            for (ushort e = 0; e < envelopes.indexRemap[drw1Index]; e++)
                            {
                                offset += envelopes.numBonesAffecting[e];
                            }

                            offset *= 2;
                            Matrix4 finalTransform = Matrix4.Identity;
                            for (ushort k = 0; k < numBonesAffecting; k++)
                            {
                                ushort boneIndex = envelopes.indexRemap[offset + (k * 0x2)];
                                float boneWeight = envelopes.weights[(offset / 2) + k];

                                weight.BoneIndexes[k] = boneIndex;
                                weight.BoneWeights[k] = boneWeight;

                                // This was apaprently a partial thought I never finished or got working in the old one? :S
                            }
                        }
                        else
                        {
                            // If the vertex isn't weighted, we just use the position from the bone matrix.
                            SkeletonBone joint = skeleton[drawInfo.Indexes[drw1Index]];
                            Matrix4 translation = Matrix4.CreateTranslation(joint.Translation);
                            Matrix4 rotation = Matrix4.CreateFromQuaternion(joint.Rotation);
                            Matrix4 finalMatrix = rotation * translation;

                            // Move the mesh by transforming the position by this much.

                            // I think we can just assign full weight to the first bone index and call it good.
                            weight.BoneIndexes = new[] { drawInfo.Indexes[drw1Index] };
                            weight.BoneWeights = new[] { 1f };
                        }

                        batch.BoneWeights[j] = weight;
                    }

                }
            }

            return j3dMesh;
        }

        private static void LoadSHP1SectionFromFile(MeshVertexAttributeHolder vertexData, Mesh j3dMesh, EndianBinaryReader reader, long chunkStart)
        {
            short batchCount = reader.ReadInt16();
            short padding = reader.ReadInt16();
            int batchOffset = reader.ReadInt32();
            int unknownTableOffset = reader.ReadInt32(); // Another one of those 0->(n-1) counters. I think all sections have it? Might be part of the way they used inheritance to write files.
            int alwaysZero = reader.ReadInt32(); Trace.Assert(alwaysZero == 0);
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
                Trace.Assert(reader.ReadByte() == 0xFF); // Padding
                ushort packetCount = reader.ReadUInt16();
                ushort batchAttributeOffset = reader.ReadUInt16();
                ushort firstMatrixIndex = reader.ReadUInt16();
                ushort firstPacketIndex = reader.ReadUInt16();
                ushort unknownpadding = reader.ReadUInt16(); Trace.Assert(unknownpadding == 0xFFFF);

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
                // This also varies *per batch* as not all batches will have the things like PositionMatrixIndex.
                reader.BaseStream.Position = chunkStart + attributeOffset + batchAttributeOffset;
                var batchAttributes = new List<ShapeAttribute>();
                do
                {
                    ShapeAttribute attribute = new ShapeAttribute();
                    attribute.ArrayType = (VertexArrayType)reader.ReadInt32();
                    attribute.DataType = (VertexDataType)reader.ReadInt32();

                    if (attribute.ArrayType == VertexArrayType.NullAttr)
                        break;

                    batchAttributes.Add(attribute);

                } while (true);


                for (ushort p = 0; p < packetCount; p++)
                {
                    // Packet Location
                    reader.BaseStream.Position = chunkStart + packetLocationOffset;
                    reader.BaseStream.Position += (firstPacketIndex + p) * 0x8; // A Packet Location is 0x8 long, so we skip ahead to the right one.

                    int packetSize = reader.ReadInt32();
                    int packetOffset = reader.ReadInt32();

                    // Read the matrix data for this packet
                    reader.BaseStream.Position = chunkStart + matrixDataOffset + (firstMatrixIndex + p) * 0x08;
                    ushort matrixUnknown0 = reader.ReadUInt16();
                    ushort matrixCount = reader.ReadUInt16();
                    uint matrixFirstIndex = reader.ReadUInt32();

                    // Skip ahead to the actual data.
                    reader.BaseStream.Position = chunkStart + matrixTableOffset + (matrixFirstIndex * 0x2);
                    List<ushort> matrixTable = new List<ushort>();
                    for (int m = 0; m < matrixCount; m++)
                    {
                        matrixTable.Add(reader.ReadUInt16());
                    }

                    // Jump the read head to the location of the primitives for this packet.
                    reader.BaseStream.Position = chunkStart + primitiveDataOffset + packetOffset;
                    int numVertexesAtPacketStart = meshVertexData.PositionMatrixIndexes.Count;

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
                            WLog.Warning(LogCategory.ModelLoading, null, "Unsupported GXPrimitiveType {0}", type);
                        }

                        numPrimitiveBytesRead += 0x3; // Advance us by 3 for the Primitive header.

                        for (int v = 0; v < vertexCount; v++)
                        {
                            meshVertexData.Indexes.Add(overallVertexCount);
                            overallVertexCount++;

                            // Iterate through the attribute types. I think the actual vertices are stored in interleaved format,
                            // ie: there's say 13 vertexes but those 13 vertexes will have a pos/color/tex index listed after it
                            // depending on the overall attributes of the file.
                            for (int attrib = 0; attrib < batchAttributes.Count; attrib++)
                            {
                                // Jump to primitive location
                                //reader.BaseStream.Position = chunkStart + primitiveDataOffset + numPrimitiveBytesRead + packetOffset;

                                // Now that we know how big the vertex type is stored in (either a Signed8 or a Signed16) we can read that much data
                                // and then we can use that index and index into 
                                int val = 0;
                                uint numBytesRead = 0;
                                switch (batchAttributes[attrib].DataType)
                                {
                                    case VertexDataType.Signed8:
                                        val = reader.ReadByte();
                                        numBytesRead = 1;
                                        break;
                                    case VertexDataType.Signed16:
                                        val = reader.ReadInt16();
                                        numBytesRead = 2;
                                        break;
                                    default:
                                        WLog.Warning(LogCategory.ModelLoading, null, "Unknown Batch Index Type: {0}", batchAttributes[attrib].DataType);
                                        break;
                                }

                                // Now that we know what the index is, we can retrieve it from the appropriate array
                                // and stick it into our vertex. The J3D format removes all duplicate vertex attributes
                                // so we need to re-duplicate them here so that we can feed them to a PC GPU in a normal fashion.
                                switch (batchAttributes[attrib].ArrayType)
                                {
                                    case VertexArrayType.Position:
                                        meshVertexData.Position.Add(vertexData.Position[val]);
                                        break;
                                    case VertexArrayType.PositionMatrixIndex:
                                        meshVertexData.PositionMatrixIndexes.Add(val);
                                        break;
                                    case VertexArrayType.Normal:
                                        meshVertexData.Normal.Add(vertexData.Normal[val]);
                                        break;
                                    case VertexArrayType.Color0:
                                        meshVertexData.Color0.Add(vertexData.Color0[val]);
                                        break;
                                    case VertexArrayType.Color1:
                                        meshVertexData.Color1.Add(vertexData.Color1[val]);
                                        break;
                                    case VertexArrayType.Tex0:
                                        meshVertexData.Tex0.Add(vertexData.Tex0[val]);
                                        break;
                                    case VertexArrayType.Tex1:
                                        meshVertexData.Tex1.Add(vertexData.Tex1[val]);
                                        break;
                                    case VertexArrayType.Tex2:
                                        meshVertexData.Tex2.Add(vertexData.Tex2[val]);
                                        break;
                                    case VertexArrayType.Tex3:
                                        meshVertexData.Tex3.Add(vertexData.Tex3[val]);
                                        break;
                                    case VertexArrayType.Tex4:
                                        meshVertexData.Tex4.Add(vertexData.Tex4[val]);
                                        break;
                                    case VertexArrayType.Tex5:
                                        meshVertexData.Tex5.Add(vertexData.Tex5[val]);
                                        break;
                                    case VertexArrayType.Tex6:
                                        meshVertexData.Tex6.Add(vertexData.Tex6[val]);
                                        break;
                                    case VertexArrayType.Tex7:
                                        meshVertexData.Tex7.Add(vertexData.Tex7[val]);
                                        break;
                                    default:
                                        WLog.Warning(LogCategory.ModelLoading, null, "Unsupported attribType {0}", batchAttributes[attrib].ArrayType);
                                        break;
                                }

                                numPrimitiveBytesRead += numBytesRead;
                            }

                            // Gonna try a weird hack, where if the mesh doesn't have PMI values, we're going to use just use the packet index into the matrixtable
                            // so that all meshes always have PMI values, to abstract out the ones that don't seem to (but still have matrixtable) junk. It's a guess
                            // here. 
                            if (batchAttributes.Find(x => x.ArrayType == VertexArrayType.PositionMatrixIndex) == null)
                            {
                                meshVertexData.PositionMatrixIndexes.Add(p);
                            }
                        }

                        // After we write a primitive, write a special null-terminator which signifies the GPU to do a primitive restart for the next tri-strip.
                        meshVertexData.Indexes.Add(0xFFFF);
                    }

                    // The Matrix Table is per-packet, so we need to reach into the the matrix table after processing each packet
                    // and transform the indexes. Yuck. Yay.
                    for (int j = numVertexesAtPacketStart; j < meshVertexData.PositionMatrixIndexes.Count; j++)
                    {
                        // Yes you divide this by 3. No, no one knows why. $20 to the person who figures out why.
                        meshBatch.drawIndexes.Add(matrixTable[meshVertexData.PositionMatrixIndexes[j] / 3]);
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
                meshBatch.PositionMatrixIndexs = meshVertexData.PositionMatrixIndexes; // This should be obsolete as they should be transformed already.
            }
        }

        private static void BuildSkeletonRecursive(SceneNode node, List<SkeletonBone> skeleton, List<SkeletonBone> rawJoints, int parentJointIndex)
        {
            switch (node.Type)
            {
                case HierarchyDataTypes.NewNode:
                    parentJointIndex = skeleton.Count - 1;
                    break;

                case HierarchyDataTypes.Joint:
                    var joint = rawJoints[node.Value];

                    if (parentJointIndex < skeleton.Count)
                        joint.Parent = skeleton[parentJointIndex];
                    skeleton.Add(joint);
                    break;
            }

            foreach (SceneNode child in node.Children)
                BuildSkeletonRecursive(child, skeleton, rawJoints, parentJointIndex);
        }

        private static void AssignMaterialsToMeshRecursive(SceneNode node, Mesh mesh, ref Material curMaterial, List<Material> materialList)
        {
            switch (node.Type)
            {
                case HierarchyDataTypes.Material:
                    curMaterial = materialList[node.Value];
                    break;

                case HierarchyDataTypes.Batch:
                    mesh.SubMeshes[node.Value].Material = curMaterial;
                    break;
            }

            foreach (var child in node.Children)
                AssignMaterialsToMeshRecursive(child, mesh, ref curMaterial, materialList);
        }
    }


}
