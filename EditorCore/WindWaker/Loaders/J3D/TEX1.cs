using GameFormatReader.Common;
using System.Collections.Generic;
using System.IO;
using WEditor.Common.Nintendo.J3D;
using WEditor.Rendering;

namespace WEditor.WindWaker.Loaders
{
    public static partial class J3DLoader
    {
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
    }
}
