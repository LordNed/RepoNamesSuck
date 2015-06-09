using EditorCore.WindWaker.Templates;
using GameFormatReader.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using EditorCore.WindWaker.MapEntities;
using OpenTK;

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

        public static void Load(MapEntities.MapEntityResource resource, Map map, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found");

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

                    chunks.Add(chunk);
                }

                // For each chunk, read all elements of that type of chunk.
                for(int i = 0; i < chunks.Count; i++)
                {
                    ChunkHeader chunk = chunks[i];

                    // Find the appropriate JSON template that describes this chunk.
                    ItemTemplate template = templates.Find(x => string.Compare(x.FourCC, chunk.FourCC, StringComparison.InvariantCultureIgnoreCase) == 0);

                    if(template == null)
                    {
                        Console.WriteLine("Unsupported entity format: " + chunk.FourCC);
                        continue;
                    }

                    reader.BaseStream.Position = chunk.ChunkOffset;

                    for(int k = 0; k < chunk.ElementCount; k++)
                    {
                        MapEntityObject entity = LoadFromStreamIntoObjectUsingTemplate(chunk.FourCC, reader, template, map);
                        Console.WriteLine("===== PLYR ===== ");
                        for(int l = 0; l < entity.Properties.Count; l++)
                        {
                            Console.WriteLine("[{0}] ({1}): {2}", entity.Properties[l].Name, entity.Properties[l].Type,  entity.Properties[l].Value.ToString());
                        }
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

        private static MapEntityObject LoadFromStreamIntoObjectUsingTemplate(string FourCC, EndianBinaryReader reader, ItemTemplate template, Map map)
        {
            MapEntityObject obj = new MapEntityObject(FourCC);

            // We're going to examine the Template's properties and load based on the current template type.
            for(int i = 0; i < template.Properties.Count; i++)
            {
                ItemTemplateProperty templateProperty = template.Properties[i];
                string propertyName = templateProperty.Name;
                PropertyType type = PropertyType.None;
                object value = null;

                switch(templateProperty.Type)
                {
                    case "fixedLengthString":
                        type = PropertyType.String;
                        value = reader.ReadString((uint)templateProperty.Length).Trim(new[] { '\0' });
                        break;

                    case "string":
                        type = PropertyType.String;
                        value = reader.ReadStringUntil('\0');
                        break;

                    case "byte":
                        type = PropertyType.Byte;
                        value = reader.ReadByte();
                        break;

                    case "short":
                        type = PropertyType.Short;
                        value = reader.ReadInt16();
                        break;

                    case "int":
                        type = PropertyType.Int32;
                        value = reader.ReadInt32();
                        break;

                    case "float":
                        type = PropertyType.Float;
                        value = reader.ReadSingle();
                        break;

                    case "vector3":
                        type = PropertyType.Vector3;
                        value = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;

                    case "vector2":
                        type = PropertyType.Vector2;
                        value = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        break;

                    case "enum":
                        type = PropertyType.Enum;
                        
                        // ToDo: Resolve to actual Enum later.
                        byte enumIndexBytes = reader.ReadByte();

                        value = enumIndexBytes;
                        break;

                    case "objectReference":
                        type = PropertyType.ObjectReference;

                        // ToDo: Resolve the object reference.
                        byte refByte = reader.ReadByte();
                        value = ResolveObjectReference(templateProperty, refByte, map);
                        break;
                }

                MapEntityObject.Property instanceProp = new MapEntityObject.Property(templateProperty.Name, type, value);
                obj.Properties.Add(instanceProp);
            }

            return obj;
        }

        private static object ResolveObjectReference(ItemTemplateProperty templateProperty, int index, Map map)
        {
            switch(templateProperty.ReferenceType)
            {
                case "Room":
                    return map.Rooms[index];
            }

            return null;
        }
    }
}
