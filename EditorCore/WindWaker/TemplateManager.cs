using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using WEditor.Maps;
using WEditor.Maps.Entities;

namespace WEditor
{
    public class TemplateManager
    {
        public List<MapEntityDataDescriptor> MapEntityDataDescriptors { get; private set; }
        public List<MapObjectDataDescriptor> MapObjectDataDescriptors { get; private set; }

        public MapObjectDataDescriptor DefaultMapObjectDataDescriptor { get; private set; }

        public void LoadTemplates(string entityDescriptorPath, string objectDescriptorPath)
        {
            // Load Entity Data, which describes the layout of various entities in a map.
            DirectoryInfo mapEntityDescriptorDirectory = new DirectoryInfo(entityDescriptorPath);
            MapEntityDataDescriptors = new List<MapEntityDataDescriptor>();

            foreach (var file in mapEntityDescriptorDirectory.GetFiles())
            {
                var template = JsonConvert.DeserializeObject<MapEntityDataDescriptor>(File.ReadAllText(file.FullName));
                MapEntityDataDescriptors.Add(template);
            }

            // Then load the Object Data, which describes the layout of specific actors since their parameters change
            // depending on the actor used.
            DirectoryInfo objDataDI = new DirectoryInfo(objectDescriptorPath);
            MapObjectDataDescriptors = new List<MapObjectDataDescriptor>();

            foreach (var file in objDataDI.GetFiles())
            {
                var descriptor = JsonConvert.DeserializeObject<MapObjectDataDescriptor>(File.ReadAllText(file.FullName));
                MapObjectDataDescriptors.Add(descriptor);

                if (descriptor.TechnicalName == "DEFAULT_TEMPLATE")
                {
                    if (DefaultMapObjectDataDescriptor != null)
                    {
                        WLog.Warning(LogCategory.EntityLoading, null, "Found multiple default MapObjectDataDescriptors, ignoring.");
                        continue;
                    }

                    DefaultMapObjectDataDescriptor = descriptor;
                }
            }

            if(DefaultMapObjectDataDescriptor == null)
                throw new FileNotFoundException("Default MapObjectDataDescriptor not found!", objectDescriptorPath);
        }
    }
}
