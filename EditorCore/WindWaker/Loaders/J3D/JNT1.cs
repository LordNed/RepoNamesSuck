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
        private List<SkeletonBone> LoadJNT1SectionFromStream(EndianBinaryReader reader, long chunkStart)
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
                ushort unknown1 = reader.ReadUInt16();  // Values of 0, 2 or 1. yaz0r calls it Matrix Type (referring to 'MatrixTable' which is an index into DRW1 - Draw type?
                                                        // If value is 1 or 2 then Bounding Box / Radius is Vector3.Zero / 0f
                                                        // And seems to be 0 if a joint has direct non-joint children (Maybe 0 is a 'bone with children' and the BBMin/Max contain
                                                        // the bounds of children?
                ushort unknown2 = reader.ReadUInt16();  // No one seems to know what it is, often 0xFF or 0 or 1. May be two individual bytes with a pad afterwards.

                for (int f = 0; f < 3; f++)
                    bone.Scale[f] = reader.ReadSingle();

                Vector3 eulerAngles = new Vector3();
                for (int f = 0; f < 3; f++)
                    eulerAngles[f] = (reader.ReadInt16() * (180 / 32786f));

                Quaternion xAxis = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), eulerAngles.X * MathE.Deg2Rad);
                Quaternion yAxis = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), eulerAngles.Y * MathE.Deg2Rad);
                Quaternion zAxis = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), eulerAngles.Z * MathE.Deg2Rad);

                // Swizzling to the ZYX order seems to be the right one.
                Quaternion finalRot = zAxis * yAxis * xAxis;

                bone.Rotation = finalRot;
                Trace.Assert(reader.ReadUInt16() == 0xFFFF); // Padding
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
