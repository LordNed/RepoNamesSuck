using GameFormatReader.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
        private static DrawInfo LoadDRW1FromStream(EndianBinaryReader reader, long chunkStart)
        {
            DrawInfo drawInfo = new DrawInfo();

            ushort sectionCount = reader.ReadUInt16();
            Debug.Assert(reader.ReadUInt16() == 0xFFFF); // Padding
            uint isWeightedOffset = reader.ReadUInt32();
            uint indexOffset = reader.ReadUInt32();

            reader.BaseStream.Position = chunkStart + isWeightedOffset;
            for (int k = 0; k < sectionCount; k++)
                drawInfo.IsWeighted.Add(reader.ReadBoolean());

            reader.BaseStream.Position = chunkStart + indexOffset;
            for (int k = 0; k < sectionCount; k++)
                drawInfo.Indexes.Add(reader.ReadUInt16());
            return drawInfo;
        }
    }
}
