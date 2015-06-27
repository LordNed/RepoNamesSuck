using GameFormatReader.Common;
using System.Collections.Generic;
using System.Diagnostics;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
        private static List<SkeletonBone> LoadJNT1SectionFromStream(EndianBinaryReader reader, long chunkStart)
        {
            List<SkeletonBone> skeletonBones;
            skeletonBones = new List<SkeletonBone>();

            ushort numJoints = reader.ReadUInt16();
            ushort padding = reader.ReadUInt16();
            uint jointDataOffset = reader.ReadUInt32(); // Relative to JNT1 Header Start
            uint jointRemapOffset = reader.ReadUInt32();
            uint stringTableOffset = reader.ReadUInt32();

            // Grab the joint names from file
            reader.BaseStream.Position = chunkStart + stringTableOffset;
            StringTable jointNames = StringTable.FromStream(reader);

            // This is always 0 to (numJoints-1) - unsure if it's a joint
            // remap (likely) or a string-table remap (less likely). It's unknown
            // if any models don't follow this 0 to (n-1) pattern.
            ushort[] jointRemaps = new ushort[numJoints];
            reader.BaseStream.Position = chunkStart + jointRemapOffset;
            for (int j = 0; j < numJoints; j++)
                jointRemaps[j] = reader.ReadUInt16();

            // Grab the actual joints
            reader.BaseStream.Position = chunkStart + jointDataOffset;
            for (int j = 0; j < numJoints; j++)
            {
                SkeletonBone bone = new SkeletonBone();

                bone.Name = jointNames[j];
                bone.Unknown1 = reader.ReadUInt16();
                bone.Unknown2 = reader.ReadUInt16();
                for (int f = 0; f < 3; f++)
                    bone.Scale[f] = reader.ReadSingle();
                for (int f = 0; f < 3; f++)
                    bone.Rotation[f] = reader.ReadInt16() * (180/32786f);
                ushort jntPadding = reader.ReadUInt16(); Debug.Assert(jntPadding == 0xFFFF);
                for (int f = 0; f < 3; f++)
                    bone.Translation[f] = reader.ReadSingle();
                bone.BoundingSphereDiameter = reader.ReadSingle();
                for (int f = 0; f < 3; f++)
                    bone.BoundingBoxMin[f] = reader.ReadSingle();
                for (int f = 0; f < 3; f++)
                    bone.BoundingBoxMax[f] = reader.ReadSingle();

                skeletonBones.Add(bone);
            }
            return skeletonBones;
        }
    }
}
