using GameFormatReader.Common;
using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using WEditor.Maps;
using WEditor.WindWaker.Maps;

namespace WEditor.WindWaker.Loaders
{
    public class MapEntityLoader
    {
        private sealed class ChunkHeader
        {
            /// <summary> FourCC Tag of the Chunk </summary>
            public string FourCC;
            /// <summary> How many elements of this type exist. </summary>
            public int ElementCount;
            /// <summary> Offset from the start of the file to the chunk data. </summary>
            public int ChunkOffset;


            /// <summary>
            // Used to fix up ACTR, TRES, and SCOB which can support up to 12 layers (+base)
            // this is resolved at chunk load time and then stored in the chunk and passed
            // to the entities being created.
            /// </summary>
            public MapLayer Layer = MapLayer.Default;

            public override string ToString()
            {
                return string.Format("[{0}]", FourCC);
            }
        }

        public sealed class RawMapEntity
        {
            public MapLayer Layer = MapLayer.Default;
            public string FourCC;
            public PropertyCollection Fields;
        }

        private List<ItemJsonTemplate> m_templates;
        private Dictionary<Scene, List<RawMapEntity>> m_entityData;
        private Map m_map;

        public MapEntityLoader(Map parentMap)
        {
            LoadItemTemplates();
            m_entityData = new Dictionary<Scene, List<RawMapEntity>>();
            m_map = parentMap;
        }

        public void LoadFromStream(Scene parentScene, EndianBinaryReader reader)
        {
            var mapEntities = new List<RawMapEntity>();
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
                    RawMapEntity entityInstance = LoadMapEntityFromStream(chunk.FourCC, reader, template);
                    entityInstance.Layer = chunk.Layer;
                    mapEntities.Add(entityInstance);
                }
            }

            m_entityData[parentScene] = mapEntities;
        }

        private void LoadItemTemplates()
        {
            string executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            executionPath += "/WindWaker/Templates/EntityData/";

            DirectoryInfo dI = new DirectoryInfo(executionPath);
            m_templates = new List<ItemJsonTemplate>();

            foreach (var file in dI.GetFiles())
            {
                var template = JsonConvert.DeserializeObject<ItemJsonTemplate>(File.ReadAllText(file.FullName));
                m_templates.Add(template);
            }

            // Now that all templates have been loaded, resolve any templates that refer to other templates.
            foreach (var template in m_templates)
            {
                if (!string.IsNullOrEmpty(template.Template))
                {
                    // If the template isn't null, we're going to replace its properties with another entities properties
                    // so that we don't end up duplicating template code.
                    var otherTemplate = m_templates.Find(x => string.Compare(x.FourCC, template.Template, StringComparison.InvariantCultureIgnoreCase) == 0);
                    template.Properties = new List<ItemJsonTemplate.Property>(otherTemplate.Properties);
                }
            }
        }

        private RawMapEntity LoadMapEntityFromStream(string chunkFourCC, EndianBinaryReader reader, ItemJsonTemplate template)
        {
            RawMapEntity obj = new RawMapEntity();
            obj.Fields = new PropertyCollection();
            obj.FourCC = chunkFourCC;

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
                        break;

                    case "objectReferenceShort":
                        // When we first resolve them, we're going to keep the value as the reference byte, 
                        // and then when they are post-processed they'll be turned into a proper type.
                        type = PropertyType.ObjectReference;
                        value = (int)reader.ReadUInt16();
                        break;

                    case "objectReferenceArray":
                        // When we first resolve them, we're going to keep the value as the reference byte, 
                        // and then when they are post-processed they'll be turned into a proper type.
                        type = PropertyType.ObjectReference;
                        var refList = new BindingList<object>();
                        for (int refArray = 0; refArray < templateProperty.Length; refArray++)
                        {
                            refList.Add((int)reader.ReadByte());
                        }
                        value = refList;
                        break;

                    case "xyRotation":
                        {
                            type = PropertyType.XYRotation;
                            Vector3 eulerAngles = new Vector3();
                            for (int f = 0; f < 2; f++)
                                eulerAngles[f] = (reader.ReadInt16() * (180 / 32786f));

                            Quaternion xAxis = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), eulerAngles.X * MathE.Deg2Rad);
                            Quaternion yAxis = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), eulerAngles.Y * MathE.Deg2Rad);

                            // Swizzling to the ZYX order seems to be the right one.
                            Quaternion finalRot = yAxis * xAxis;
                            value = finalRot;
                        }
                        break;

                    case "xyzRotation":
                        {
                            type = PropertyType.XYZRotation;
                            Vector3 eulerAngles = new Vector3();
                            for (int f = 0; f < 3; f++)
                                eulerAngles[f] = (reader.ReadInt16() * (180 / 32786f));

                            Quaternion xAxis = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), eulerAngles.X * MathE.Deg2Rad);
                            Quaternion yAxis = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), eulerAngles.Y * MathE.Deg2Rad);
                            Quaternion zAxis = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), eulerAngles.Z * MathE.Deg2Rad);

                            // Swizzling to the ZYX order seems to be the right one.
                            Quaternion finalRot = zAxis * yAxis * xAxis;
                            value = finalRot;
                        }
                        break;
                    case "yRotation":
                        {
                            type = PropertyType.XYZRotation;
                            float yRotation = reader.ReadInt16() * (180 / 32786f);

                            Quaternion yAxis = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), yRotation * MathE.Deg2Rad);
                            value = yAxis;
                        }
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

                Property instanceProp = new Property(templateProperty.Name, type, value);
                obj.Fields.Properties.Add(instanceProp);
            }

            return obj;
        }

        public void PostProcessEntities()
        {
            foreach (var kvp in m_entityData)
            {
                foreach (var entity in kvp.Value)
                {
                    ItemJsonTemplate origTemplate = m_templates.Find(x => string.Compare(x.FourCC, entity.FourCC, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (origTemplate == null)
                    {
                        WLog.Warning(LogCategory.EntityLoading, null, "Failed to find template for entity {0}, not attempting to post-process.", entity);
                        continue;
                    }

                    foreach (Property property in entity.Fields.Properties)
                    {
                        ItemJsonTemplate.Property origTemplateProperty = origTemplate.Properties.Find(x => string.Compare(x.Name, property.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                        if (origTemplateProperty == null)
                        {
                            WLog.Warning(LogCategory.EntityLoading, null, "Failed to find property {0} on template {1} for entity {2}, not attempting to post-process.", property.Name, origTemplate.FourCC, entity);
                            continue;
                        }

                        // We cheated earlier and stored the various reference-type ones as their index values. That means the type of the object doesn't actually
                        // reflect the Type field. Thus, we now need to go back, patch up the references, and set them to be their proper type. Yeah!
                        switch (origTemplateProperty.Type)
                        {
                            case "objectReference":
                            case "objectReferenceShort":
                                {
                                    int objIndex = (int)property.Value;
                                    property.Value = ResolveEntityReference(entity.FourCC, origTemplateProperty, objIndex, kvp.Key);
                                }
                                break;
                            case "objectReferenceArray":
                                {
                                    BindingList<object> indexes = (BindingList<object>)property.Value;
                                    BindingList<object> resolvedRefs = new BindingList<object>();
                                    for (int i = 0; i < indexes.Count; i++)
                                    {
                                        var obj = ResolveEntityReference(entity.FourCC, origTemplateProperty, (int)indexes[i], kvp.Key);
                                        resolvedRefs.Add(obj);
                                    }
                                    property.Value = resolvedRefs;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private object ResolveEntityReference(string askingChunkFourCC, ItemJsonTemplate.Property templateProperty, int index, Scene scene)
        {
            switch (templateProperty.ReferenceType)
            {
                case "Room":
                    // Some things will specify a Room index of 255 for "This isn't Used", so we're going to special-case handle that.
                    if (index == 0xFF)
                        return null;

                    if (index < m_map.Rooms.Count)
                    {
                        return m_map.Rooms[index];
                    }
                    else
                    {
                        WLog.Warning(LogCategory.EntityLoading, null, "Chunk {0} requested reference for room but index is out of range. (Property Name: {1}, Index: {2})", askingChunkFourCC, templateProperty.Name, index);
                    }
                    return null;

                case "FourCC":
                    // Get an (ordered) list of all chunks of that type.
                    List<RawMapEntity> potentialRefs = new List<RawMapEntity>();
                    foreach (var entity in m_entityData[scene])
                    {
                        if (entity.FourCC == templateProperty.ReferenceFourCCType)
                            potentialRefs.Add(entity);
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

        private static MapLayer ResolveChunkFourCCToLayer(string fourCC)
        {
            // Only ACTR, SCOB, and TRES support multiple layers so if it's not one of them, early out.
            if (!(fourCC.StartsWith("ACT") || fourCC.StartsWith("SCO") || fourCC.StartsWith("TRE")))
                return MapLayer.Default;

            // Examine the last character, 0-9, a-b. 
            char lastChar = fourCC[3];
            switch (lastChar)
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

        private static string ResolveFourCCWithLayerToName(string fourCC)
        {
            if (fourCC.StartsWith("ACT")) return "ACTR";
            if (fourCC.StartsWith("TRE")) return "TRES";
            if (fourCC.StartsWith("SCO")) return "SCOB";

            return fourCC;
        }


        public Dictionary<Scene, List<RawMapEntity>> GetData()
        {
            return m_entityData;
        }
    }
}
