using GameFormatReader.Common;
using OpenTK;
using System.Collections.Generic;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
        private static void LoadEVP1FromStream(EndianBinaryReader reader, long chunkStart)
        {
            ushort numEnvelopes = reader.ReadUInt16();
            reader.ReadUInt16(); // Padding

            // numEnvelope many uint8 - each one describes how many bones belong to this index.
            uint boneCountOffset = reader.ReadUInt32();
            // "sum over all bytes in boneCountOffset many shorts (index into some joint stuff? into matrix table?)"
            uint indexDataOffset = reader.ReadUInt32();
            // Bone Weights (as many floats here as there are ushorts at indexDataOffset)
            uint weightOffset = reader.ReadUInt32();
            // Matrix Table (3x4 float array) - Inverse Bind Pose
            uint boneMatrixOffset = reader.ReadUInt32();


            // - Is this the number of bones which influence the vert?
            reader.BaseStream.Position = chunkStart + boneCountOffset;
            List<byte> boneCounts = new List<byte>();
            for (int b = 0; b < numEnvelopes; b++)
                boneCounts.Add(reader.ReadByte());

            // ???
            reader.BaseStream.Position = chunkStart + indexDataOffset;
            List<ushort> indexes = new List<ushort>();
            for (int m = 0; m < boneCounts.Count; m++)
            {
                for (int j = 0; j < boneCounts[m]; j++)
                {
                    indexes.Add(reader.ReadUInt16());
                }
            }

            // Bone Weights
            reader.BaseStream.Position = chunkStart + weightOffset;
            List<float> weights = new List<float>();
            for (int w = 0; w < boneCounts.Count; w++)
            {
                for (int j = 0; j < boneCounts[w]; j++)
                {
                    weights.Add(reader.ReadSingle());
                }
            }

            // Inverse Bind Pose Matrices
            reader.BaseStream.Position = chunkStart + boneMatrixOffset;
            List<Matrix3x4> inverseBindPose = new List<Matrix3x4>();
            for (int w = 0; w < numEnvelopes; w++)
            {
                Matrix3x4 matrix = new Matrix3x4();
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 4; k++)
                        matrix[j, k] = reader.ReadSingle();
                }

                inverseBindPose.Add(matrix);
            }
        }
    }
}
