using WEditor.Common;
using WEditor.WindWaker.MapEntities;
using GameFormatReader.Common;
using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WEditor.Common.Maps;

namespace WEditor.WindWaker.Loaders
{
    public class MapEntityLoader
    {
        private class ChunkHeader
        {
            /// <summary> FourCC Tag of the Chunk </summary>
            public string FourCC;
            /// <summary> How many elements of this type exist. </summary>
            public int ElementCount;
            /// <summary> Offset from the start of the file to the chunk data. </summary>
            public int ChunkOffset;

            public override string ToString()
            {
                return string.Format("[{0}]", FourCC);
            }
        }

        public static void Load(MapEntities.MapEntityResource resource, Map map, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("filePath not found");

            var templates = LoadItemTemplates();
            var templateOrder = LoadTemplateOrder();

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

                // Now sort the chunk headers to match the JSON defined Template Order, so that when we load a particular chunk,
                // we can ensure its dependency chunks have been loaded in advanced.
                chunks = SortChunksByTemplateOrder(chunks, templateOrder);

                // For each chunk, read all elements of that type of chunk.
                for(int i = 0; i < chunks.Count; i++)
                {
                    ChunkHeader chunk = chunks[i];

                    // Find the appropriate JSON template that describes this chunk.
                    ItemJsonTemplate template = templates.Find(x => string.Compare(x.FourCC, chunk.FourCC, StringComparison.InvariantCultureIgnoreCase) == 0);

                    if(template == null)
                    {
                        WLog.Warning(LogCategory.EntityLoading, resource, "Unsupported entity format: " + chunk.FourCC);
                        continue;
                    }

                    reader.BaseStream.Position = chunk.ChunkOffset;

                    for(int k = 0; k < chunk.ElementCount; k++)
                    {
                        MapEntityObject entity = LoadFromStreamIntoObjectUsingTemplate(chunk.FourCC, reader, template, map, resource);
                        resource.Objects.Add(entity);

                        /*Console.WriteLine("===== {0} =====", entity.FourCC);
                        for(int l = 0; l < entity.Properties.Count; l++)
                        {
                            Console.WriteLine("[{0}] ({1}): {2}", entity.Properties[l].Name, entity.Properties[l].Type,  entity.Properties[l].Value.ToString());
                        }

                        Console.WriteLine("====");*/
                    }
                }
            }
        }

        private static List<ItemJsonTemplate> LoadItemTemplates()
        {
            string executionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            executionPath += "/WindWaker/Templates/EntityData/";

            DirectoryInfo dI = new DirectoryInfo(executionPath);
            List<ItemJsonTemplate> itemTemplates = new List<ItemJsonTemplate>();

            foreach(var file in dI.GetFiles())
            {
                var template = JsonConvert.DeserializeObject<ItemJsonTemplate>(File.ReadAllText(file.FullName));
                itemTemplates.Add(template);
            }

            // Now that all templates have been loaded, resolve any templates that refer to other templates.
            foreach(var template in itemTemplates)
            {
                if (!string.IsNullOrEmpty(template.Template))
                {
                    // If the template isn't null, we're going to replace its properties with another entities properties
                    // so that we don't end up duplicating template code.
                    var otherTemplate = itemTemplates.Find(x => string.Compare(x.FourCC, template.Template, StringComparison.InvariantCultureIgnoreCase) == 0);
                    template.Properties = new List<ItemJsonTemplate.Property>(otherTemplate.Properties);
                }
            }

            return itemTemplates;
        }

        private static List<string> LoadTemplateOrder()
        {
            string executionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            executionPath += "/WindWaker/Templates/TemplateOrder.json";

            return JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(executionPath));
        }

        private static List<ChunkHeader> SortChunksByTemplateOrder(List<ChunkHeader> unsortedChunks, List<string> templateOrder)
        {
            var sortedChunks = new List<ChunkHeader>();

            for(int i = 0; i < templateOrder.Count; i++)
            {
                var chunk = unsortedChunks.Find(x => string.Compare(x.FourCC, templateOrder[i]) == 0);
                if (chunk != null)
                {
                    sortedChunks.Add(chunk);
                }
            }

            if(sortedChunks.Count != unsortedChunks.Count)
            {
                // Find the missing one that was not in our templateOrder list.
                foreach(var unsortedChunk in unsortedChunks)
                {
                    if (!templateOrder.Contains(unsortedChunk.FourCC))
                    {
                        WLog.Warning(LogCategory.EntityLoading, null, "TemplateOrder is missing type {0}, adding to end of list.", unsortedChunk.FourCC);
                        sortedChunks.Add(unsortedChunk);
                    }
                }
            }

            return sortedChunks;
        }


        private static MapEntityObject LoadFromStreamIntoObjectUsingTemplate(string FourCC, EndianBinaryReader reader, ItemJsonTemplate template, Map map, MapEntityResource resource)
        {
            MapEntityObject obj = new MapEntityObject(FourCC);

            // We're going to examine the Template's properties and load based on the current template type.
            for(int i = 0; i < template.Properties.Count; i++)
            {
                ItemJsonTemplate.Property templateProperty = template.Properties[i];
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
                        byte refByte = reader.ReadByte();
                        value = ResolveObjectReference(templateProperty, refByte, map, resource);
                        break;

                    case "objectReferenceShort":
                        type = PropertyType.ObjectReference;
                        ushort refShort = reader.ReadUInt16();
                        value = ResolveObjectReference(templateProperty, refShort, map, resource);
                        break;

                    case "objectReferenceArray":
                        type = PropertyType.ObjectReference;
                        var refList = new BindingList<object>();
                        for(int refArray = 0; refArray < templateProperty.Length; refArray++)
                        {
                            byte refByteArray = reader.ReadByte();
                            refList.Add(ResolveObjectReference(templateProperty, refByteArray, map, resource));
                        }

                        value = refList;
                        break;

                    case "xyRotation":
                        type = PropertyType.XYRotation;
                        value = new XYRotation(reader.ReadUInt16(), reader.ReadUInt16());
                        break;

                    case "xyzRotation":
                        type = PropertyType.XYZRotation;
                        value = new XYZRotation(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
                        break;

                    case "color32":
                        type = PropertyType.Color32;
                        value = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;

                    case "color24":
                        type = PropertyType.Color24;
                        value = new Color24(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;

                    case "vector3byte":
                        type = PropertyType.Vector3Byte;
                        value = new Vector3(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;
                }

                MapEntityObject.Property instanceProp = new MapEntityObject.Property(templateProperty.Name, type, value);
                obj.Properties.Add(instanceProp);
            }

            return obj;
        }

        private static object ResolveObjectReference(ItemJsonTemplate.Property templateProperty, int index, Map map, MapEntityResource resource)
        {
            switch(templateProperty.ReferenceType)
            {
                case "Room":
                    // Some things will specify a Room index of 255 for "This isn't Used", so we're going to special-case handle that.
                    if (index == 0xFF)
                        return null;

                    if(index < map.Rooms.Count-1)
                        return map.Rooms[index];
                    return null;

                case "FourCC":
                    // We can (hopefully) know that the array it's about to index is already loaded
                    // thanks to the TemplateOrder.json defining them that way. Thus, we can just
                    // resolve the dependency by looking into the right array!

                    // Get an (ordered) list of all chunks of that type.
                    List<MapEntityObject> potentialRefs = new List<MapEntityObject>();
                    for(int i = 0; i < resource.Objects.Count; i++)
                    {
                        // Check against all potential reference types. This resolves the issue where things like  RCAM/CAMR point to AROB/RARO and they're sharing
                        // a template (maybe a bad idea...) but need to object-reference against a different type. I don't think you'll have AROB/RARO in the same file
                        // so this should work...
                        for(int k = 0; k < templateProperty.ReferenceFourCCType.Length; k++)
                        {
                            if (string.Compare(resource.Objects[i].FourCC, templateProperty.ReferenceFourCCType[k]) == 0)
                                potentialRefs.Add(resource.Objects[i]);
                        }
                    }

                    // There's an edge-case here where some maps omit an entity (such as Fairy01 not having a Virt chunk) but use index 0 (Fairy01's Pale chunk)
                    // and so it was finding no potentialRefs 
                    if(index < potentialRefs.Count)
                        return potentialRefs[index];
                    break;
            }

            return null;
        }

        public List<MapEntityData> LoadFromStream(EndianBinaryReader reader)
        {
            return null;
        }
    }
}
