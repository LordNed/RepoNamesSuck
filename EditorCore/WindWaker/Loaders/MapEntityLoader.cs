using EditorCore.WindWaker.Templates;
using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EditorCore.WindWaker.Loaders
{
    public static class MapEntityLoader
    {
        private class ChunkHeader
        {
            /// <summary> FourCC Tag of the Chunk </summary>
            public string FourCC;
            /// <summary> How many elements of this type exist. </summary>
            public int ElementCount;
            /// <summary> Offset from the start of the file to the chunk data. </summary>
            public int ChunkOffset;
        }

        public static void Load(MapEntities.MapEntityObject resource, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found ensure");

            var templates = LoadItemTemplates();

            // Simply load the contents of the data into the Data array and preserve it for now.
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open), Endian.Big))
            {
                long fileOffsetStart = reader.BaseStream.Position;

                // Read the File Header
                int chunkCount = reader.ReadInt32();

                // Read the chunk headers
                List<ChunkHeader> chunks = new List<ChunkHeader>();
                for(int i = 0; i < chunkCount; i++)
                {
                    ChunkHeader chunk = new ChunkHeader();
                    chunk.FourCC = reader.ReadString(4);
                    chunk.ElementCount = reader.ReadInt32();
                    chunk.ChunkOffset = reader.ReadInt32();
                }

                // For each chunk, read all elements of that type of chunk.
                for(int i = 0; i < chunks.Count; i++)
                {
                    ChunkHeader chunk = chunks[i];
                    reader.BaseStream.Position = chunk.ChunkOffset;

                    for(int k = 0; k < chunk.ElementCount; k++)
                    {

                    }
                }
            }
        }

        private static List<ItemTemplate> LoadItemTemplates()
        {
            string executionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            executionPath += "/WindWaker/Templates/";

            DirectoryInfo dI = new DirectoryInfo(executionPath);
            List<ItemTemplate> itemTemplates = new List<ItemTemplate>();

            foreach(var file in dI.GetFiles())
            {
                var template = JsonConvert.DeserializeObject<ItemTemplate>(File.ReadAllText(file.FullName));
                itemTemplates.Add(template);
            }

            return itemTemplates;
        }
    }
}
