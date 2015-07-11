using WEditor.Common;
using GameFormatReader.Common;
using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WEditor.Common.Maps;
using WEditor.Maps;

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

            // Used to fix up ACTR, TRES, and SCOB which can support up to 12 layers (+base)
            // this is resolved at chunk load time and then stored in the chunk and passed
            // to the entities being created.
            public MapLayer Layer = MapLayer.Default;

            public override string ToString()
            {
                return string.Format("[{0}]", FourCC);
            }
        }

        private List<ItemJsonTemplate> m_templates;

        public MapEntityLoader()
        {
            m_templates = LoadItemTemplates();
        }

        public BindingList<MapEntity> LoadFromStream(EndianBinaryReader reader)
        {
            BindingList<MapEntity> entities = new BindingList<MapEntity>();

            long fileOffsetStart = reader.BaseStream.Position;

            // File Header
            int chunkCount = reader.ReadInt32();

            // Read the chunk headers
            List<ChunkHeader> chunks = new List<ChunkHeader>();
            for (int i = 0; i < chunkCount; i++)
            {
                ChunkHeader chunk = new ChunkHeader();
                chunk.FourCC = reader.ReadString(4);
                chunk.ElementCount = reader.ReadInt32();
                chunk.ChunkOffset = reader.ReadInt32();

                chunk.Layer = ResolveChunkFourCCToLayer(chunk.FourCC);
                chunk.FourCC = ResolveFourCCWithLayerToName(chunk.FourCC);

                chunks.Add(chunk);
            }

            // For each chunk, read all elements of that type of chunk.
            for (int i = 0; i < chunks.Count; i++)
            {
                ChunkHeader chunk = chunks[i];

                // Find the appropriate JSON template that describes this chunk.
                ItemJsonTemplate template = m_templates.Find(x => string.Compare(x.FourCC, chunk.FourCC, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (template == null)
                {
                    WLog.Error(LogCategory.EntityLoading, null, "Unsupported entity FourCC: {0}. Map will save without this data!", chunk.FourCC);
                    continue;
                }

                reader.BaseStream.Position = chunk.ChunkOffset;

                for (int k = 0; k < chunk.ElementCount; k++)
                {
                    MapEntity entityInstance = LoadMapEntityFromStream(chunk.FourCC, reader, template, entities);
                    entityInstance.Layer = chunk.Layer;
                    entities.Add(entityInstance);
                }
            }

            return entities;
        }

        private MapLayer ResolveChunkFourCCToLayer(string fourCC)
        {
            // Only ACTR, SCOB, and TRES support multiple layers so if it's not one of them, early out.
            if (!(fourCC.StartsWith("ACT") || fourCC.StartsWith("SCO") || fourCC.StartsWith("TRE")))
                return MapLayer.Default;

            // Examine the last character, 0-9, a-b. 
            char lastChar = fourCC[3];
            switch(lastChar)
            {
                case '0': return MapLayer.Layer0;
                case '1': return MapLayer.Layer1;
                case '2': return MapLayer.Layer2;
                case '3': return MapLayer.Layer3;
                case '4': return MapLayer.Layer4;
                case '5': return MapLayer.Layer5;
                case '6': return MapLayer.Layer6;
                case '7': return MapLayer.Layer7;
                case '8': return MapLayer.Layer8;
                case '9': return MapLayer.Layer9;
                case 'a': return MapLayer.LayerA;
                case 'b': return MapLayer.LayerB;
            }

            // It's passed the above check, so it's ACTR, SCOB, or TRES (default layer)
            return MapLayer.Default;
        }

        private string ResolveFourCCWithLayerToName(string fourCC)
        {
            if (fourCC.StartsWith("ACT")) return "ACTR";
            if (fourCC.StartsWith("TRE")) return "TRES";
            if (fourCC.StartsWith("SCO")) return "SCOB";

            return fourCC;
        }

        private List<ItemJsonTemplate> LoadItemTemplates()
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

        private MapEntity LoadMapEntityFromStream(string chunkFourCC, EndianBinaryReader reader, ItemJsonTemplate template, BindingList<MapEntity> loadedEntities)
        {
            // Determine if this particular entity can have its layer changed after it is created.
            // Only ACTR, SCOB, and TRES support multiple layers.
            bool layerCanChange = (chunkFourCC == "TRES" || chunkFourCC == "ACTR" || chunkFourCC == "SCOB");


            MapEntity obj = new MapEntity(chunkFourCC, layerCanChange);

            // We're going to examine the Template's properties and load based on the current template type.
            for (int i = 0; i < template.Properties.Count; i++)
            {
                ItemJsonTemplate.Property templateProperty = template.Properties[i];
                string propertyName = templateProperty.Name;
                PropertyType type = PropertyType.None;
                object value = null;

                switch (templateProperty.Type)
                {
                    case "fixedLengthString":
                        type = PropertyType.String;
                        value = reader.ReadString(templateProperty.Length).Trim(new[] { '\0' });
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
                        byte enumIndexBytes = reader.ReadByte(); // ToDo: Resolve to actual Enum later.
                        value = enumIndexBytes;
                        break;

                    case "objectReference":
                        // When we first resolve them, we're going to keep the value as the reference byte, 
                        // and then when they are post-processed they'll be turned into a proper type.
                        type = PropertyType.ObjectReference;
                        value = (int)reader.ReadByte();
                        //byte refByte = reader.ReadByte();
                        //value = ResolveEntityReferenceNew(chunkFourCC, templateProperty, refByte, loadedEntities);
                        break;

                    case "objectReferenceShort":
                        // When we first resolve them, we're going to keep the value as the reference byte, 
                        // and then when they are post-processed they'll be turned into a proper type.
                        type = PropertyType.ObjectReference;
                        value = (int)reader.ReadUInt16();
                        //ushort refShort = reader.ReadUInt16();
                        //value = ResolveEntityReferenceNew(chunkFourCC, templateProperty, refShort, loadedEntities);
                        break;

                    case "objectReferenceArray":
                        // When we first resolve them, we're going to keep the value as the reference byte, 
                        // and then when they are post-processed they'll be turned into a proper type.
                        type = PropertyType.ObjectReference;
                        var refList = new BindingList<object>();
                        for (int refArray = 0; refArray < templateProperty.Length; refArray++)
                        {
                            refList.Add((int)reader.ReadByte());
                            //byte refByteArray = reader.ReadByte();
                            //refList.Add(ResolveEntityReferenceNew(chunkFourCC, templateProperty, refByteArray, loadedEntities));
                        }
                        value = refList;
                        break;

                    case "xyRotation":
                        type = PropertyType.XYRotation;
                        var xyRot = new XYRotation(reader.ReadUInt16(), reader.ReadUInt16());

                        // Convert from -32768,32768 to -180,180
                        xyRot.X = xyRot.X / 32768f * 180;
                        xyRot.Y = xyRot.Y / 32768f * 180;
                        value = xyRot;
                        
                        break;

                    case "xyzRotation":
                        type = PropertyType.XYZRotation;
                        var xyzRot = new XYZRotation(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());

                        // Convert from -32768,32768 to -180,180
                        xyzRot.X = xyzRot.X / 32768f * 180;
                        xyzRot.Y = xyzRot.Y / 32768f * 180;
                        xyzRot.Z = xyzRot.Z / 32768f * 180;
                        value = xyzRot;
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

                    case "int32BitField":
                        type = PropertyType.Int32BitField;
                        value = reader.ReadInt32();
                        break;

                    case "bits":
                        type = PropertyType.Int32;
                        value = (int)reader.ReadBits(templateProperty.Length);
                        break;
                }

                EntityProperty instanceProp = new EntityProperty(templateProperty.Name, type, value);
                obj.Properties.Add(instanceProp);
            }

            return obj;
        }

        public void PostProcess(Scene scene, Map map)
        {
            foreach(MapEntity entity in scene.Entities)
            {
                ItemJsonTemplate origTemplate = m_templates.Find(x => string.Compare(x.FourCC, entity.FourCC, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (origTemplate == null)
                {
                    WLog.Warning(LogCategory.EntityLoading, map, "Failed to find template for entity {0}, not attempting to post-process.", entity);
                    continue;
                }

                foreach(EntityProperty property in entity.Properties)
                {
                    ItemJsonTemplate.Property origTemplateProperty = origTemplate.Properties.Find(x => string.Compare(x.Name, property.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if(origTemplateProperty == null)
                    {
                        WLog.Warning(LogCategory.EntityLoading, map, "Failed to find property {0} on template {1} for entity {2}, not attempting to post-process.", property.Name, origTemplate.FourCC, entity);
                        continue;
                    }

                    // We cheated earlier and stored the various reference-type ones as their index values. That means the type of the object doesn't actually
                    // reflect the Type field. Thus, we now need to go back, patch up the references, and set them to be their proper type. Yeah!
                    switch(origTemplateProperty.Type)
                    {
                        case "objectReference":
                        case "objectReferenceShort":
                            {
                                int objIndex = (int)property.Value;
                                property.Value = ResolveEntityReference(entity.FourCC, origTemplateProperty, objIndex, scene.Entities, map);
                            }
                            break;
                        case "objectReferenceArray":
                            {
                                BindingList<object> indexes = (BindingList<object>)property.Value;
                                BindingList<object> resolvedRefs = new BindingList<object>();
                                for(int i = 0; i < indexes.Count; i++)
                                {
                                    var obj = ResolveEntityReference(entity.FourCC, origTemplateProperty, (int)indexes[i], scene.Entities, map);
                                    resolvedRefs.Add(obj);
                                }
                                property.Value = resolvedRefs;
                            }
                            break;
                    }
                }
            }
        }

        private object ResolveEntityReference(string askingChunkFourCC, ItemJsonTemplate.Property templateProperty, int index, BindingList<MapEntity> loadedEntities, Map map)
        {
            switch (templateProperty.ReferenceType)
            {
                case "Room":
                    // Some things will specify a Room index of 255 for "This isn't Used", so we're going to special-case handle that.
                    if (index == 0xFF)
                        return null;

                    if (index < map.Rooms.Count)
                    {
                        return map.Rooms[index];
                    }
                    else
                    {
                        WLog.Warning(LogCategory.EntityLoading, null, "Chunk {0} requested reference for room but index is out of range. (Property Name: {1}, Index: {2})", askingChunkFourCC, templateProperty.Name, index);
                    }
                    return null;

                case "FourCC":
                    // We can (hopefully) know that the array it's about to index is already loaded
                    // thanks to the TemplateOrder.json defining them that way. Thus, we can just
                    // resolve the dependency by looking into the right array!

                    // Get an (ordered) list of all chunks of that type.
                    List<MapEntity> potentialRefs = new List<MapEntity>();
                    for (int i = 0; i < loadedEntities.Count; i++)
                    {
                        // Check against all potential reference types.
                        if (string.Compare(loadedEntities[i].FourCC, templateProperty.ReferenceFourCCType) == 0)
                            potentialRefs.Add(loadedEntities[i]);
                    }

                    // There's an edge-case here where some maps omit an entity (such as Fairy01 not having a Virt chunk) but use index 0 (Fairy01's Pale chunk)
                    // and so it was finding no potentialRefs 
                    if (index < potentialRefs.Count)
                    {
                        return potentialRefs[index];
                    }
                    else
                    {
                        WLog.Warning(LogCategory.EntityLoading, null, "Chunk {0} requested reference for property {1} but index ({2}) is out of range.", askingChunkFourCC, templateProperty.Name, index);
                    }
                    return null;
            }

            return null;
        }
    }
}
